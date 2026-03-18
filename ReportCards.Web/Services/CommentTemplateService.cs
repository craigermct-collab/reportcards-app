using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ReportCards.Web.Data;

namespace ReportCards.Web.Services;

/// <summary>
/// Handles comment template import (XML) and placeholder substitution.
/// </summary>
public class CommentTemplateService(SchoolDbContext db)
{
    // ─────────────────────────────────────────────────────────────────────────
    // PLACEHOLDER SUBSTITUTION
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Substitutes all ETeach-style placeholders in <paramref name="template"/>
    /// using the student's first name and pronoun set.
    /// </summary>
    public static string Substitute(string template, string firstName, PronounSet pronouns)
    {
        // Pronoun lookup
        var (subjLower, subjTitle) = pronouns switch
        {
            PronounSet.HeHim    => ("he",   "He"),
            PronounSet.SheHer   => ("she",  "She"),
            _                   => ("they", "They"),
        };

        var (possLower, possTitle) = pronouns switch
        {
            PronounSet.HeHim    => ("his",   "His"),
            PronounSet.SheHer   => ("her",   "Her"),
            _                   => ("their", "Their"),
        };

        var objLower = pronouns switch
        {
            PronounSet.HeHim    => "him",
            PronounSet.SheHer   => "her",
            _                   => "them",
        };

        var result = template;

        // Name — try exact case variants first, then case-insensitive fallback
        result = Regex.Replace(result, @"~Name\b",       CapsFirst(firstName));
        result = Regex.Replace(result, @"~name\b",       firstName.ToLower());

        // Subject pronoun   ~H/s/e  / ~h/s/e
        result = Regex.Replace(result, @"~H/s/e\b",      subjTitle,  RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"~h/s/e\b",      subjLower);

        // Possessive pronoun  ~H/s/r  / ~h/s/r  (note: source XML has trailing slash variants too)
        result = Regex.Replace(result, @"~H/s/r/?",      possTitle);
        result = Regex.Replace(result, @"~h/s/r/?",      possLower);

        // Object pronoun  ~him/her
        result = Regex.Replace(result, @"~him/her\b",    objLower,   RegexOptions.IgnoreCase);

        // Tidy up any double-spaces left by substitution
        result = Regex.Replace(result, @"  +", " ");

        return result.Trim();
    }

    private static string CapsFirst(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..].ToLower();

    // ─────────────────────────────────────────────────────────────────────────
    // FILTERED QUERY
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns templates filtered by subject and/or grade label.
    /// Pass null to get templates that have no scope restriction.
    /// </summary>
    public async Task<List<CommentTemplate>> GetTemplatesAsync(
        string? subject = null,
        string? gradeLabel = null)
    {
        var query = db.CommentTemplates.AsQueryable();

        if (subject != null)
            query = query.Where(t => t.Subject == null || t.Subject == subject);

        if (gradeLabel != null)
            query = query.Where(t => t.GradeLabel == null || t.GradeLabel == gradeLabel);

        return await query
            .OrderBy(t => t.Category)
            .ThenBy(t => t.SortOrder)
            .ThenBy(t => t.Id)
            .ToListAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // XML IMPORT
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Parses the ETeach library XML export and upserts templates into the database.
    /// Returns (inserted, updated, skipped) counts.
    /// </summary>
    public async Task<(int Inserted, int Updated, int Skipped)> ImportXmlAsync(Stream xmlStream)
    {
        var doc = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);
        var root = doc.Root ?? throw new InvalidDataException("Empty XML document.");

        // Build filter ID → name maps (Level = grade, Category = category)
        var levelMap    = BuildFilterMap(root, "Level");
        var categoryMap = BuildFilterMap(root, "Category");

        int inserted = 0, updated = 0, skipped = 0;
        var seenKeys = new HashSet<string>();

        // Walk every folder hierarchy: top-level folder = subject, sub-folder = grade
        foreach (var subjectFolder in root.Elements("folder"))
        {
            var subject = (string?)subjectFolder.Element("name") ?? "General";

            // Comments directly in the subject folder (no grade scoping)
            foreach (var comment in subjectFolder.Elements("comment"))
            {
                var (ins, upd, skip) = await UpsertCommentAsync(comment, subject, null, levelMap, categoryMap, seenKeys);
                inserted += ins; updated += upd; skipped += skip;
            }

            // Grade sub-folders
            foreach (var gradeFolder in subjectFolder.Elements("folder"))
            {
                var grade = (string?)gradeFolder.Element("name");

                foreach (var comment in gradeFolder.Elements("comment"))
                {
                    var (ins, upd, skip) = await UpsertCommentAsync(comment, subject, grade, levelMap, categoryMap, seenKeys);
                    inserted += ins; updated += upd; skipped += skip;
                }
            }
        }

        await db.SaveChangesAsync();
        return (inserted, updated, skipped);
    }

    private async Task<(int, int, int)> UpsertCommentAsync(
        XElement commentEl,
        string subject,
        string? grade,
        Dictionary<string, string> levelMap,
        Dictionary<string, string> categoryMap,
        HashSet<string> seenKeys)
    {
        var name = (string?)commentEl.Element("name") ?? "";
        var code = (string?)commentEl.Element("code") ?? "";
        var text = (string?)commentEl.Element("commentLevel")?.Element("comment") ?? "";

        // Skip blank templates
        if (string.IsNullOrWhiteSpace(text)) return (0, 0, 1);

        var title = string.IsNullOrWhiteSpace(name) ? null : name;
        var sourceCode = string.IsNullOrWhiteSpace(name) ? null : $"{name}|{subject}|{grade}";

        // Skip in-file duplicates
        if (sourceCode != null && !seenKeys.Add(sourceCode)) return (0, 0, 1);

        // Derive category from commentFilterItem IDs
        var filterIds = commentEl.Elements("commentFilterItem").Select(f => (string?)f ?? "").ToList();
        var category  = filterIds.Select(id => categoryMap.GetValueOrDefault(id, ""))
                                 .FirstOrDefault(c => !string.IsNullOrEmpty(c));

        var sortOrder = int.TryParse((string?)commentEl.Element("sortOrder"), out var so) ? so : 0;

        // Upsert by SourceCode (name|subject|grade composite)
        if (sourceCode != null)
        {
            var existing = await db.CommentTemplates.FirstOrDefaultAsync(t => t.SourceCode == sourceCode);
            if (existing != null)
            {
                existing.Title        = title;
                existing.TemplateText = text;
                existing.Subject      = subject;
                existing.GradeLabel   = grade;
                existing.Category     = category;
                existing.SortOrder    = sortOrder;
                existing.UpdatedAt    = DateTimeOffset.UtcNow;
                return (0, 1, 0);
            }
        }

        db.CommentTemplates.Add(new CommentTemplate
        {
            Title        = title,
            TemplateText = text,
            Subject      = subject,
            GradeLabel   = grade,
            Category     = category,
            SourceCode   = sourceCode,
            SortOrder    = sortOrder,
        });

        return (1, 0, 0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // XLSX IMPORT
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Parses a .xlsx file where the first row is a header containing (at minimum)
    /// a "TemplateText" column. Optional columns: Title, Subject, Grade, Category.
    /// Column names are matched case-insensitively.
    /// Returns (inserted, updated, skipped) counts.
    /// </summary>
    public async Task<(int Inserted, int Updated, int Skipped)> ImportXlsxAsync(Stream xlsxStream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(xlsxStream);
        var sheet = package.Workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidDataException("The Excel file contains no worksheets.");

        if (sheet.Dimension == null || sheet.Dimension.Rows < 2)
            return (0, 0, 0);

        // Build column index map from header row (row 1)
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int col = 1; col <= sheet.Dimension.Columns; col++)
        {
            var header = sheet.Cells[1, col].GetValue<string>()?.Trim();
            if (!string.IsNullOrEmpty(header))
                colMap[header] = col;
        }

        if (!colMap.ContainsKey("TemplateText"))
            throw new InvalidDataException("The spreadsheet must have a 'TemplateText' column header in row 1.");

        int GetCol(string name) => colMap.TryGetValue(name, out var c) ? c : -1;
        string? Cell(int row, int col) => col < 1 ? null : sheet.Cells[row, col].GetValue<string>()?.Trim();

        int templateTextCol = GetCol("TemplateText");
        int titleCol        = GetCol("Title");
        int subjectCol      = GetCol("Subject");
        int gradeCol        = GetCol("Grade");
        int categoryCol     = GetCol("Category");

        int inserted = 0, updated = 0, skipped = 0;

        for (int row = 2; row <= sheet.Dimension.Rows; row++)
        {
            var templateText = Cell(row, templateTextCol);
            if (string.IsNullOrWhiteSpace(templateText)) { skipped++; continue; }

            var title    = Cell(row, titleCol);
            var subject  = Cell(row, subjectCol);
            var grade    = Cell(row, gradeCol);
            var category = Cell(row, categoryCol);

            // Build a dedup key from title + subject + grade (same logic as XML import)
            var sourceCode = string.IsNullOrWhiteSpace(title) ? null
                : $"{title}|{subject ?? ""}|{grade ?? ""}".TrimEnd('|');

            if (sourceCode != null)
            {
                var existing = await db.CommentTemplates.FirstOrDefaultAsync(t => t.SourceCode == sourceCode);
                if (existing != null)
                {
                    existing.Title        = string.IsNullOrWhiteSpace(title)    ? null : title;
                    existing.Subject      = string.IsNullOrWhiteSpace(subject)  ? null : subject;
                    existing.GradeLabel   = string.IsNullOrWhiteSpace(grade)    ? null : grade;
                    existing.Category     = string.IsNullOrWhiteSpace(category) ? null : category;
                    existing.TemplateText = templateText;
                    existing.UpdatedAt    = DateTimeOffset.UtcNow;
                    updated++;
                    continue;
                }
            }

            db.CommentTemplates.Add(new CommentTemplate
            {
                Title        = string.IsNullOrWhiteSpace(title)    ? null : title,
                Subject      = string.IsNullOrWhiteSpace(subject)  ? null : subject,
                GradeLabel   = string.IsNullOrWhiteSpace(grade)    ? null : grade,
                Category     = string.IsNullOrWhiteSpace(category) ? null : category,
                TemplateText = templateText,
                SourceCode   = sourceCode,
                SortOrder    = row,
            });
            inserted++;
        }

        await db.SaveChangesAsync();
        return (inserted, updated, skipped);
    }

    private static Dictionary<string, string> BuildFilterMap(XElement root, string filterName)
    {
        return root.Elements("filter")
            .FirstOrDefault(f => (string?)f.Element("name") == filterName)
            ?.Elements("filterItem")
            .ToDictionary(
                fi => (string?)fi.Element("ID") ?? "",
                fi => (string?)fi.Element("name") ?? "")
            ?? new Dictionary<string, string>();
    }
}
