using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.AcroForms;
using ReportCards.Web.Data;

namespace ReportCards.Web.Services
{
    /// <summary>
    /// Generates filled PDF report cards by:
    /// 1. Loading student assessment data for a term
    /// 2. Mapping DB fields to PDF field IDs via ReportTemplateFieldMap
    /// 3. Filling PDF AcroForm fields using PdfSharp (pure .NET, no Python)
    /// </summary>
    public class ReportCardGeneratorService
    {
        private readonly SchoolDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ReportCardGeneratorService> _logger;

        private const string TemplatesFolder = "ReportCardTemplates";

        public ReportCardGeneratorService(
            SchoolDbContext db,
            IWebHostEnvironment env,
            ILogger<ReportCardGeneratorService> logger)
        {
            _db     = db;
            _env    = env;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────
        // PUBLIC API
        // ─────────────────────────────────────────────────────────────

        public async Task<byte[]> GenerateForStudentAsync(int enrollmentId, int termInstanceId)
        {
            var (template, fieldData) = await BuildFieldDataAsync(enrollmentId, termInstanceId);
            return await FillPdfAsync(template, fieldData);
        }

        public async Task<byte[]> GenerateZipForClassAsync(int classGroupInstanceId, int termInstanceId)
        {
            var enrollments = await _db.Enrollments
                .Where(e => e.ClassGroupInstanceId == classGroupInstanceId
                         && e.TermInstanceId == termInstanceId)
                .Include(e => e.Student)
                .OrderBy(e => e.Student!.LastName)
                .ThenBy(e => e.Student!.FirstName)
                .ToListAsync();

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var enrollment in enrollments)
                {
                    try
                    {
                        var pdfBytes = await GenerateForStudentAsync(enrollment.Id, termInstanceId);
                        var fileName = $"{enrollment.Student!.LastName}_{enrollment.Student.FirstName}.pdf";
                        var entry    = zip.CreateEntry(fileName, CompressionLevel.Fastest);
                        using var entryStream = entry.Open();
                        await entryStream.WriteAsync(pdfBytes);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to generate report card for enrollment {Id}", enrollment.Id);
                    }
                }
            }

            return ms.ToArray();
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE: BUILD FIELD DATA
        // ─────────────────────────────────────────────────────────────

        private async Task<(ReportCardTemplate template, PdfFieldData data)>
            BuildFieldDataAsync(int enrollmentId, int termInstanceId)
        {
            var enrollment = await _db.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Grade)
                .Include(e => e.ClassGroupInstance)
                    .ThenInclude(c => c.TermInstance)
                .Include(e => e.ClassGroupInstance)
                    .ThenInclude(c => c.ReportCardTemplate)
                .Include(e => e.LearningItems)
                    .ThenInclude(li => li.YearClassOffering)
                        .ThenInclude(o => o!.CurriculumClassTemplate)
                .Include(e => e.LearningItems)
                    .ThenInclude(li => li.YearSubjectOffering)
                        .ThenInclude(o => o!.CurriculumSubjectTemplate)
                .Include(e => e.LearningItems)
                    .ThenInclude(li => li.Assessments)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId)
            ?? throw new InvalidOperationException($"Enrollment {enrollmentId} not found.");

            var term = enrollment.ClassGroupInstance?.TermInstance
                ?? throw new InvalidOperationException("Term not found on enrollment.");

            var template = enrollment.ClassGroupInstance?.ReportCardTemplate
                ?? throw new InvalidOperationException(
                    $"No report card template assigned to '{enrollment.ClassGroupInstance?.DisplayName}' in term '{term.Name}'. " +
                    "Please assign a template in RC Templates.");

            // Load field maps — prefer per-template maps (new), fall back to per-term (legacy)
            var fieldMaps = await _db.ReportTemplateFieldMaps
                .Where(m => m.ReportCardTemplateId == template.Id)
                .ToListAsync();

            if (fieldMaps.Count == 0)
            {
                // Fall back to legacy per-term maps
                fieldMaps = await _db.ReportTemplateFieldMaps
                    .Where(m => m.TermInstanceId == termInstanceId)
                    .ToListAsync();
            }

            // Use last-wins for duplicate destination keys (e.g. student.grade → Grade + GradeInSeptember)
            var mapByKey = fieldMaps
                .GroupBy(m => m.ReportDestinationKey)
                .ToDictionary(g => g.Key, g => g.Last().PdfFieldName);

            // For keys that map to multiple PDF fields (like student.grade), build a multi-map
            var multiMapByKey = fieldMaps
                .GroupBy(m => m.ReportDestinationKey)
                .ToDictionary(g => g.Key, g => g.Select(m => m.PdfFieldName).ToList());

            var configs = await _db.SchoolConfigs.ToListAsync();
            string Cfg(string key) => configs.FirstOrDefault(c => c.Key == key)?.Value ?? "";

            var teacher = await _db.TeacherAssignments
                .Include(a => a.Teacher)
                .Where(a => a.ClassGroupInstanceId == enrollment.ClassGroupInstanceId)
                .Select(a => a.Teacher)
                .FirstOrDefaultAsync();

            var absences = await _db.AttendanceEvents
                .CountAsync(a => a.StudentId == enrollment.StudentId
                              && a.Date >= term.StartDate && a.Date <= term.EndDate
                              && a.Type == AttendanceType.Absent);

            var lates = await _db.AttendanceEvents
                .CountAsync(a => a.StudentId == enrollment.StudentId
                              && a.Date >= term.StartDate && a.Date <= term.EndDate
                              && a.Type == AttendanceType.Late);

            List<Assessment> term1Assessments = new();
            if (template.TemplateType == ReportCardTemplateType.ElementaryReportCard)
            {
                var earlierTerm = await _db.TermInstances
                    .Where(t => t.SchoolYearId == term.SchoolYearId
                             && t.SortOrder < term.SortOrder)
                    .OrderByDescending(t => t.SortOrder)
                    .FirstOrDefaultAsync();

                if (earlierTerm != null)
                {
                    term1Assessments = await _db.Assessments
                        .Include(a => a.StudentLearningItem)
                        .Where(a => a.TermInstanceId == earlierTerm.Id
                                 && a.StudentLearningItem!.EnrollmentId == enrollmentId)
                        .ToListAsync();
                }
            }

            var currentAssessments = enrollment.LearningItems
                .SelectMany(li => li.Assessments)
                .Where(a => a.TermInstanceId == termInstanceId)
                .ToDictionary(a => a.StudentLearningItemId);

            var term1ByItemId = term1Assessments.ToDictionary(a => a.StudentLearningItemId);

            var fields     = new Dictionary<string, string>();
            var checkboxes = new Dictionary<string, bool>();
            var radios     = new Dictionary<string, string>();

            var student = enrollment.Student!;
            SetFields(fields, multiMapByKey, ReportDestinationKeys.StudentName,     $"{student.FirstName} {student.LastName}");
            SetFields(fields, multiMapByKey, ReportDestinationKeys.StudentOen,      student.OenNumber ?? "");
            SetFields(fields, multiMapByKey, ReportDestinationKeys.StudentGrade,    enrollment.Grade?.Name ?? "");
            SetFields(fields, multiMapByKey, ReportDestinationKeys.TeacherName,     teacher?.DisplayName ?? "");
            SetFields(fields, multiMapByKey, ReportDestinationKeys.SchoolName,      Cfg(SchoolConfigKeys.SchoolName));
            SetFields(fields, multiMapByKey, ReportDestinationKeys.SchoolBoard,     Cfg(SchoolConfigKeys.Board));
            SetFields(fields, multiMapByKey, ReportDestinationKeys.SchoolAddress,   Cfg(SchoolConfigKeys.Address));
            SetFields(fields, multiMapByKey, ReportDestinationKeys.SchoolPhone,     Cfg(SchoolConfigKeys.ContactPhone));
            SetFields(fields, multiMapByKey, ReportDestinationKeys.DaysAbsent,      absences.ToString());
            SetFields(fields, multiMapByKey, ReportDestinationKeys.TotalDaysAbsent, absences.ToString());
            SetFields(fields, multiMapByKey, ReportDestinationKeys.TimesLate,       lates.ToString());
            SetFields(fields, multiMapByKey, ReportDestinationKeys.TotalTimesLate,  lates.ToString());

            foreach (var li in enrollment.LearningItems)
            {
                var destKey = li.YearClassOffering?.ReportDestinationKey
                           ?? li.YearSubjectOffering?.ReportDestinationKey;
                if (destKey == null) continue;

                bool isTwoTermCard = template.TemplateType == ReportCardTemplateType.ElementaryReportCard;

                if (isTwoTermCard)
                {
                    if (term1ByItemId.TryGetValue(li.Id, out var t1a)
                        && mapByKey.TryGetValue(destKey + ".term1", out var t1f))
                        fields[t1f] = t1a.ValueText ?? "";

                    if (currentAssessments.TryGetValue(li.Id, out var t2a))
                    {
                        if (mapByKey.TryGetValue(destKey + ".term2", out var t2f))
                            fields[t2f] = t2a.ValueText ?? "";
                        if (!string.IsNullOrWhiteSpace(t2a.Comment)
                            && mapByKey.TryGetValue(destKey + ".notes", out var nf))
                            fields[nf] = t2a.Comment;
                    }
                }
                else
                {
                    if (!mapByKey.TryGetValue(destKey, out var pdfField)) continue;
                    if (!currentAssessments.TryGetValue(li.Id, out var assess)) continue;

                    var val = assess.ValueText ?? "";
                    if (IsRadioField(pdfField))
                        radios[pdfField] = "/" + val;
                    else
                        fields[pdfField] = val;

                    if (!string.IsNullOrWhiteSpace(assess.Comment)
                        && mapByKey.TryGetValue(destKey + ".notes", out var nf))
                        fields[nf] = assess.Comment;
                }
            }

            return (template, new PdfFieldData(fields, checkboxes, radios));
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE: FILL PDF (pure .NET via PdfSharp)
        // ─────────────────────────────────────────────────────────────

        private Task<byte[]> FillPdfAsync(ReportCardTemplate template, PdfFieldData data)
        {
            var templatePath = Path.Combine(_env.ContentRootPath, TemplatesFolder, template.FileName);
            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Template file not found: {templatePath}");

            using var doc = PdfReader.Open(templatePath, PdfDocumentOpenMode.Modify);

            var form = doc.AcroForm;
            if (form == null)
                throw new InvalidOperationException("PDF has no AcroForm fields.");

            // Ensure appearances are generated when opened in a viewer
            if (form.Elements.ContainsKey("/NeedAppearances"))
                form.Elements["/NeedAppearances"] = new PdfSharp.Pdf.PdfBoolean(true);
            else
                form.Elements.Add("/NeedAppearances", new PdfSharp.Pdf.PdfBoolean(true));

            // Fill text fields by name (handles nested fields that index access can't reach)
            foreach (var (fieldName, textVal) in data.Fields)
            {
                try
                {
                    var field = form.Fields[fieldName];
                    if (field != null)
                        field.Value = new PdfSharp.Pdf.PdfString(textVal ?? "");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping text field {Name}", fieldName);
                }
            }

            // Fill checkboxes and radio buttons by index (these are top-level fields)
            for (int i = 0; i < form.Fields.Count; i++)
            {
                try
                {
                    var field = form.Fields[i];
                    if (field == null) continue;
                    var name = field.Name;
                    if (string.IsNullOrEmpty(name)) continue;

                    if (data.Checkboxes.TryGetValue(name, out var cbVal))
                    {
                        if (field is PdfCheckBoxField cb)
                            cb.Checked = cbVal;
                    }
                    else if (data.Radios.TryGetValue(name, out var radioVal))
                    {
                        var val = radioVal.TrimStart('/');
                        field.Value = new PdfSharp.Pdf.PdfName("/" + val);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping PDF field at index {I}", i);
                }
            }

            using var ms = new MemoryStream();
            doc.Save(ms);
            return Task.FromResult(ms.ToArray());
        }

        // ─────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────

        /// <summary>Sets one value into all PDF fields mapped from a single destination key.</summary>
        private static void SetFields(
            Dictionary<string, string> fields,
            Dictionary<string, List<string>> multiMap,
            string destKey,
            string value)
        {
            if (multiMap.TryGetValue(destKey, out var pdfFields))
                foreach (var pdfField in pdfFields)
                    fields[pdfField] = value;
        }

        private static bool IsRadioField(string pdfFieldName)
            => pdfFieldName.EndsWith("Skill", StringComparison.OrdinalIgnoreCase);
    }

    public record PdfFieldData(
        Dictionary<string, string> Fields,
        Dictionary<string, bool>   Checkboxes,
        Dictionary<string, string> Radios);
}
