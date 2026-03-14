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
                            .ThenInclude(st => st!.CurriculumClassTemplate)
                .Include(e => e.LearningItems)
                    .ThenInclude(li => li.Assessments)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId)
            ?? throw new InvalidOperationException($"Enrollment {enrollmentId} not found.");

            // Reload term fresh to get ReportCardTermSlot (navigation property may be stale)
            var term = await _db.TermInstances
                .FirstOrDefaultAsync(t => t.Id == enrollment.ClassGroupInstance!.TermInstanceId)
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
                TermInstance? term1Instance = null;

                if (term.ReportCardTermSlot == ReportCardTermSlot.Term2)
                {
                    // Explicitly find the Term1 slot in the same school year
                    term1Instance = await _db.TermInstances
                        .FirstOrDefaultAsync(t => t.SchoolYearId == term.SchoolYearId
                                              && t.ReportCardTermSlot == ReportCardTermSlot.Term1);
                }
                else if (term.ReportCardTermSlot == ReportCardTermSlot.NotApplicable)
                {
                    // Legacy fallback: look for any earlier term by SortOrder
                    term1Instance = await _db.TermInstances
                        .Where(t => t.SchoolYearId == term.SchoolYearId
                                 && t.SortOrder < term.SortOrder)
                        .OrderByDescending(t => t.SortOrder)
                        .FirstOrDefaultAsync();
                }
                // If current term IS Term1, term1Assessments stays empty (nothing in left column yet)

                if (term1Instance != null)
                {
                    term1Assessments = await _db.Assessments
                        .Include(a => a.StudentLearningItem)
                        .Where(a => a.TermInstanceId == term1Instance.Id
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
            SetFields(fields, multiMapByKey, ReportDestinationKeys.Principal,       Cfg(SchoolConfigKeys.Principal));
            SetFields(fields, multiMapByKey, ReportDestinationKeys.DaysAbsent,      absences.ToString());
            SetFields(fields, multiMapByKey, ReportDestinationKeys.TotalDaysAbsent, absences.ToString());
            SetFields(fields, multiMapByKey, ReportDestinationKeys.TimesLate,       lates.ToString());
            SetFields(fields, multiMapByKey, ReportDestinationKeys.TotalTimesLate,  lates.ToString());

            // ── Accumulate strand comments per parent subject notes field ──────────
            // When multiple strands map to the same parent subject (e.g. Language strands
            // all roll up to LanguageNotes), we collect them and join rather than overwrite.
            // Key = PDF notes field name, Value = ordered list of "Strand: comment" lines.
            var notesAccumulator = new Dictionary<string, List<string>>();

            foreach (var li in enrollment.LearningItems)
            {
                // Prefer explicit ReportDestinationKey on the offering; fall back to
                // deriving it from the curriculum template name.
                var destKey = li.YearClassOffering?.ReportDestinationKey
                           ?? li.YearSubjectOffering?.ReportDestinationKey
                           ?? DeriveDestinationKey(
                               li.YearClassOffering?.CurriculumClassTemplate?.Name
                               ?? li.YearSubjectOffering?.CurriculumSubjectTemplate?.CurriculumClassTemplate?.Name,
                               li.YearSubjectOffering?.CurriculumSubjectTemplate?.Name);
                if (destKey == null) continue;

                // The notes field for this item lives under the PARENT subject key, not
                // the strand-specific key. E.g. subject.french.listening → subject.french.notes
                // subject.language (whole subject) → subject.language.notes
                var parentNotesKey = DeriveParentNotesKey(destKey);

                bool isTwoTermCard = template.TemplateType == ReportCardTemplateType.ElementaryReportCard;

                if (isTwoTermCard)
                {
                    // Determine which PDF column the current term maps to
                    var currentSlot = term.ReportCardTermSlot;
                    var currentColSuffix = currentSlot == ReportCardTermSlot.Term1 ? ".term1" : ".term2";
                    var otherColSuffix   = currentSlot == ReportCardTermSlot.Term1 ? ".term2" : ".term1";

                    // Write the OTHER term's data into the other column
                    if (term1ByItemId.TryGetValue(li.Id, out var t1a)
                        && mapByKey.TryGetValue(destKey + otherColSuffix, out var t1f))
                        fields[t1f] = t1a.ValueText ?? "";

                    // Write current term grade into the current column
                    if (currentAssessments.TryGetValue(li.Id, out var t2a))
                    {
                        if (mapByKey.TryGetValue(destKey + currentColSuffix, out var t2f))
                            fields[t2f] = t2a.ValueText ?? "";

                        // Accumulate comment into the parent notes field
                        if (!string.IsNullOrWhiteSpace(t2a.Comment) && parentNotesKey != null
                            && mapByKey.TryGetValue(parentNotesKey, out var nf))
                        {
                            if (!notesAccumulator.ContainsKey(nf))
                                notesAccumulator[nf] = new List<string>();
                            notesAccumulator[nf].Add(t2a.Comment.Trim());
                        }
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

                    if (!string.IsNullOrWhiteSpace(assess.Comment) && parentNotesKey != null
                        && mapByKey.TryGetValue(parentNotesKey, out var nf))
                    {
                        if (!notesAccumulator.ContainsKey(nf))
                            notesAccumulator[nf] = new List<string>();
                        notesAccumulator[nf].Add(assess.Comment);
                    }
                }
            }

            // Flush accumulated notes — join multiple strand comments with single newlines
            foreach (var (pdfField, lines) in notesAccumulator)
                fields[pdfField] = string.Join("\n", lines.Where(l => !string.IsNullOrWhiteSpace(l)));

            // ── Subject modifier checkboxes ──────────────────────────────────────────
            // Load all StudentSubjectModifiers for this enrollment + term, then map
            // each enabled option to the appropriate PDF checkbox field(s).
            var studentModifiers = await _db.StudentSubjectModifiers
                .Include(m => m.CurriculumClassTemplate)
                .Where(m => m.EnrollmentId == enrollmentId && m.TermInstanceId == termInstanceId)
                .ToListAsync();

            foreach (var mod in studentModifiers)
            {
                var enabledOptions = System.Text.Json.JsonSerializer
                    .Deserialize<List<string>>(mod.EnabledOptionsJson) ?? new();
                if (!enabledOptions.Any()) continue;

                var subjectName = mod.CurriculumClassTemplate?.Name ?? "";

                // Derive the parent subject dest key so we know which PDF checkbox prefix to use
                var subjectDestKey = NormalizeToKey(subjectName);
                if (subjectDestKey == null) continue;

                // Also check if this subject is per-strand (French, Health) — those have
                // strand-level checkboxes in the PDF rather than subject-level ones.
                // For per-strand subjects we map modifiers per strand using the mapped dest keys.
                var isPerStrand = enrollment.LearningItems
                    .Any(li => li.ItemType == LearningItemType.Subject &&
                               (li.YearSubjectOffering?.CurriculumSubjectTemplate?.CurriculumClassTemplateId
                                == mod.CurriculumClassTemplateId));

                if (isPerStrand)
                {
                    // Get all strand dest keys for this subject from the learning items
                    var strandDestKeys = enrollment.LearningItems
                        .Where(li => li.ItemType == LearningItemType.Subject &&
                                     li.YearSubjectOffering?.CurriculumSubjectTemplate?.CurriculumClassTemplateId
                                     == mod.CurriculumClassTemplateId)
                        .Select(li => DeriveDestinationKey(
                            li.YearSubjectOffering?.CurriculumSubjectTemplate?.CurriculumClassTemplate?.Name,
                            li.YearSubjectOffering?.CurriculumSubjectTemplate?.Name))
                        .Where(k => k != null)
                        .Distinct()
                        .ToList();

                    foreach (var strandKey in strandDestKeys)
                    {
                        foreach (var option in enabledOptions)
                        {
                            var cbKey = MapModifierToCheckboxKey(strandKey!, option);
                            if (cbKey != null && mapByKey.TryGetValue(cbKey, out var cbField))
                                checkboxes[cbField] = true;
                        }
                    }
                }
                else
                {
                    // Whole-subject modifiers — one checkbox per option
                    foreach (var option in enabledOptions)
                    {
                        var cbKey = MapModifierToCheckboxKey(subjectDestKey, option);
                        if (cbKey != null && mapByKey.TryGetValue(cbKey, out var cbField))
                            checkboxes[cbField] = true;
                    }
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

            // Set NeedAppearances so viewer renders the filled values
            if (form.Elements.ContainsKey("/NeedAppearances"))
                form.Elements["/NeedAppearances"] = new PdfSharp.Pdf.PdfBoolean(true);
            else
                form.Elements.Add("/NeedAppearances", new PdfSharp.Pdf.PdfBoolean(true));

            // Fill via raw widget annotations on every page (Strategy 2)
            // This catches ALL fields — both those in the AcroForm tree and those only in /Annots
            foreach (var page in doc.Pages)
            {
                try
                {
                    var annotsRef = page.Elements["/Annots"];
                    if (annotsRef == null) continue;

                    // Resolve indirect reference if needed
                    PdfArray? annots = null;
                    if (annotsRef is PdfArray pa) annots = pa;
                    else if (annotsRef is PdfSharp.Pdf.Advanced.PdfReference pr)
                        annots = pr.Value as PdfArray;
                    if (annots == null) continue;

                    foreach (var item in annots.Elements)
                    {
                        try
                        {
                            PdfDictionary? annot = null;
                            if (item is PdfDictionary d) annot = d;
                            else if (item is PdfSharp.Pdf.Advanced.PdfReference r)
                                annot = r.Value as PdfDictionary;
                            if (annot == null) continue;

                            if (annot.Elements.GetName("/Subtype") != "/Widget") continue;

                            // Get field name from /T
                            var fieldName = annot.Elements.GetString("/T");
                            if (string.IsNullOrWhiteSpace(fieldName))
                            {
                                // Try parent
                                var parentEl = annot.Elements["/Parent"];
                                PdfDictionary? parent = parentEl as PdfDictionary;
                                if (parent == null && parentEl is PdfSharp.Pdf.Advanced.PdfReference pref)
                                    parent = pref.Value as PdfDictionary;
                                fieldName = parent?.Elements.GetString("/T") ?? "";
                            }
                            if (string.IsNullOrWhiteSpace(fieldName)) continue;

                            // Set text value
                            if (data.Fields.TryGetValue(fieldName, out var val))
                            {
                                annot.Elements["/V"]  = new PdfSharp.Pdf.PdfString(val);
                                annot.Elements["/MK"] = new PdfDictionary(); // no highlight
                                annot.Elements.Remove("/AP");
                                if (annot.Elements.ContainsKey("/MaxLen"))
                                    annot.Elements.Remove("/MaxLen");

                                bool isNotesField = fieldName.EndsWith("Notes", StringComparison.OrdinalIgnoreCase);
                                if (isNotesField)
                                {
                                    // Multiline, top-aligned, read-only
                                    annot.Elements["/DA"] = new PdfSharp.Pdf.PdfString("/Helv 9 Tf 0 g");
                                    annot.Elements["/Ff"] = new PdfSharp.Pdf.PdfInteger(4097); // ReadOnly + Multiline
                                    annot.Elements["/Q"]  = new PdfSharp.Pdf.PdfInteger(0);    // Left align
                                }
                                else
                                {
                                    annot.Elements["/DA"] = new PdfSharp.Pdf.PdfString("/Helv 9 Tf 0 g");
                                    annot.Elements["/Ff"] = new PdfSharp.Pdf.PdfInteger(1); // ReadOnly
                                }
                            }
                            // Set checkbox
                            else if (data.Checkboxes.TryGetValue(fieldName, out var cbVal))
                            {
                                annot.Elements["/V"] = cbVal
                                    ? new PdfSharp.Pdf.PdfName("/Yes")
                                    : new PdfSharp.Pdf.PdfName("/Off");
                                annot.Elements["/AS"] = cbVal
                                    ? new PdfSharp.Pdf.PdfName("/Yes")
                                    : new PdfSharp.Pdf.PdfName("/Off");
                                annot.Elements["/MK"] = new PdfDictionary();
                            }
                        }
                        catch { /* skip malformed annotation */ }
                    }
                }
                catch { /* skip malformed page */ }
            }

            // Strategy 3: for fields in AcroForm /Fields array but not in page /Annots,
            // write /V directly on the field's PDF object and remove /AP
            // These are: Student, OEN, Grade, Teacher, School, DaysAbsent, etc.
            try
            {
                var fieldsArray = form.Elements["/Fields"] as PdfArray;
                if (fieldsArray != null)
                    WriteValuesToFieldsArray(fieldsArray, data.Fields, doc);
            }
            catch { /* non-critical */ }

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

        /// <summary>
        /// Recursively walks the AcroForm /Fields array and writes values directly
        /// to field PDF objects. Handles indirect references.
        /// </summary>
        private static void WriteValuesToFieldsArray(
            PdfArray fields,
            Dictionary<string, string> values,
            PdfDocument doc)
        {
            foreach (var item in fields.Elements)
            {
                try
                {
                    PdfDictionary? field = item as PdfDictionary;
                    if (field == null && item is PdfSharp.Pdf.Advanced.PdfReference r)
                        field = r.Value as PdfDictionary;
                    if (field == null) continue;

                    // Get field name
                    var t = field.Elements.GetString("/T");
                    if (!string.IsNullOrWhiteSpace(t) && values.TryGetValue(t, out var val))
                    {
                        // Set value + flatten on parent
                        bool isNotes = t.EndsWith("Notes", StringComparison.OrdinalIgnoreCase);
                        field.Elements["/V"]  = new PdfSharp.Pdf.PdfString(val);
                        field.Elements["/DV"] = new PdfSharp.Pdf.PdfString(val);
                        field.Elements["/DA"] = new PdfSharp.Pdf.PdfString("/Helv 9 Tf 0 g");
                        field.Elements["/Ff"] = new PdfSharp.Pdf.PdfInteger(isNotes ? 4097 : 1); // ReadOnly [+ Multiline]
                        field.Elements["/MK"] = new PdfDictionary(); // No highlight
                        if (isNotes) field.Elements["/Q"] = new PdfSharp.Pdf.PdfInteger(0);
                        if (field.Elements.ContainsKey("/AP"))
                            field.Elements.Remove("/AP");
                        if (field.Elements.ContainsKey("/MaxLen"))
                            field.Elements.Remove("/MaxLen");

                        // Set value + flatten on every kid widget too
                        var kids = field.Elements["/Kids"] as PdfArray;
                        if (kids != null)
                        {
                            foreach (var kidItem in kids.Elements)
                            {
                                try
                                {
                                    PdfDictionary? kid = kidItem as PdfDictionary;
                                    if (kid == null && kidItem is PdfSharp.Pdf.Advanced.PdfReference kr)
                                        kid = kr.Value as PdfDictionary;
                                    if (kid == null) continue;
                                    kid.Elements["/V"]  = new PdfSharp.Pdf.PdfString(val);
                                    kid.Elements["/DV"] = new PdfSharp.Pdf.PdfString(val);
                                    kid.Elements["/DA"] = new PdfSharp.Pdf.PdfString("/Helv 9 Tf 0 g");
                                    kid.Elements["/Ff"] = new PdfSharp.Pdf.PdfInteger(1); // ReadOnly
                                    kid.Elements["/MK"] = new PdfDictionary();             // No highlight
                                    if (kid.Elements.ContainsKey("/AP"))
                                        kid.Elements.Remove("/AP");
                                    if (kid.Elements.ContainsKey("/AS"))
                                        kid.Elements.Remove("/AS");
                                    if (kid.Elements.ContainsKey("/MaxLen"))
                                        kid.Elements.Remove("/MaxLen");
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        // No value — recurse into kids for nested named fields
                        var kids = field.Elements["/Kids"] as PdfArray;
                        if (kids != null)
                            WriteValuesToFieldsArray(kids, values, doc);
                    }
                }
                catch { }
            }
        }

        private static bool IsRadioField(string pdfFieldName)
            => pdfFieldName.EndsWith("Skill", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Maps a subject dest key + modifier option label to the checkbox dest key.
        /// e.g. (subject.language, "ESL/ELD") → subject.language.esleld
        ///      (subject.french.listening, "IEP") → subject.french.listening.iep
        /// Returns null if the option isn't recognised.
        /// </summary>
        private static string? MapModifierToCheckboxKey(string destKey, string option)
        {
            var suffix = option.ToLowerInvariant().Trim() switch
            {
                var o when o.Contains("esl") || o.Contains("eld") => ".esleld",
                var o when o.Contains("iep")                      => ".iep",
                var o when o.Contains("french") || o == "f"       => ".french",
                var o when o is "na" or "n/a" or "not applicable" => ".na",
                _ => null
            };
            return suffix == null ? null : destKey + suffix;
        }

        /// <summary>
        /// Derives a ReportDestinationKey from curriculum template names when one
        /// hasn't been explicitly set on the offering record.
        /// Matches on the class-level name (subject group) first, then subject name.
        /// </summary>

        /// <summary>
        /// Given a destination key, returns the PDF notes key that should receive
        /// this item's comment.
        ///
        /// Rules:
        ///   subject.french.listening  → subject.french.notes   (French strand → French notes)
        ///   subject.language          → subject.language.notes  (whole subject → own notes)
        ///   subject.dance / .drama / .music / .visualArts
        ///                             → subject.theArts.notes   (Arts siblings → shared notes)
        /// </summary>
        private static string? DeriveParentNotesKey(string destKey)
        {
            if (destKey.EndsWith(".notes")) return destKey;

            // Arts subjects all share a single notes field
            if (destKey == ReportDestinationKeys.Dance      ||
                destKey == ReportDestinationKeys.Drama      ||
                destKey == ReportDestinationKeys.Music      ||
                destKey == ReportDestinationKeys.VisualArts)
                return ReportDestinationKeys.TheArtsNotes;

            var parts = destKey.Split('.');

            // Strand-specific key (3+ segments): subject.french.listening → subject.french.notes
            if (parts.Length >= 3 && parts[0] == "subject")
                return $"{parts[0]}.{parts[1]}.notes";

            // Whole-subject key: subject.language → subject.language.notes
            if (parts.Length == 2 && parts[0] == "subject")
                return $"{destKey}.notes";

            return null;
        }

        private static string? DeriveDestinationKey(string? className, string? subjectName)
        {
            // For subjects that have strand-level PDF fields (French, Health/PhysEd),
            // try the strand name first — it resolves to a more specific key.
            // For all other subjects, the class/parent name is sufficient.
            if (subjectName != null)
            {
                var strandKey = NormalizeStrandKey(subjectName);
                if (strandKey != null) return strandKey;
            }

            // Fall back to parent class template name for whole-subject grading
            if (className != null)
            {
                var key = NormalizeToKey(className);
                if (key != null) return key;
            }

            // Last resort: try the strand name as a subject key
            if (subjectName != null)
            {
                var key = NormalizeToKey(subjectName);
                if (key != null) return key;
            }

            return null;
        }

        /// <summary>
        /// Matches strand names that have their OWN dedicated PDF fields
        /// (i.e. the PDF has separate grade boxes per strand, not just one per subject).
        /// Returns null for strands that should roll up to the parent subject key.
        /// </summary>
        private static string? NormalizeStrandKey(string name) => name.ToLowerInvariant().Trim() switch
        {
            // French strands — each has FrenchListeningTerm1/2, FrenchSpeakingTerm1/2, etc.
            var n when n.Contains("listen")                           => ReportDestinationKeys.French + ".listening",
            var n when n.Contains("speak") || n.Contains("oral comm") => ReportDestinationKeys.French + ".speaking",
            var n when n.Contains("read") && !n.Contains("social")    => ReportDestinationKeys.French + ".reading",
            var n when n.Contains("writ") && !n.Contains("social")    => ReportDestinationKeys.French + ".writing",

            // Health/PhysEd strands — HealthHealthyLivingTerm1/2, HealthActiveLivingTerm1/2, HealthMovementTerm1/2
            var n when n.Contains("healthy living") || n.Contains("healthy")     => ReportDestinationKeys.Health,
            var n when n.Contains("active living")  || n.Contains("active")      => ReportDestinationKeys.PhysEd,
            var n when n.Contains("movement")                                     => ReportDestinationKeys.PhysEdMovement,

            _ => null
        };

        private static string? NormalizeToKey(string name) => name.ToLowerInvariant().Trim() switch
        {
            var n when n.Contains("language") && !n.Contains("native") && !n.Contains("french")
                                              => ReportDestinationKeys.Language,
            var n when n.Contains("native language") || n.Contains("native lang")
                                              => ReportDestinationKeys.NativeLanguage,
            var n when n.Contains("french")   => ReportDestinationKeys.French,
            var n when n.Contains("math")     => ReportDestinationKeys.Mathematics,
            var n when n.Contains("science") && n.Contains("tech")
                                              => ReportDestinationKeys.ScienceAndTech,
            var n when n.Contains("science")  => ReportDestinationKeys.ScienceAndTech,
            var n when n.Contains("social")   => ReportDestinationKeys.SocialStudies,
            var n when n.Contains("health")   => ReportDestinationKeys.Health,
            var n when n.Contains("physical") || n.Contains("phys ed") || n.Contains("active living")
                                              => ReportDestinationKeys.PhysEd,
            var n when n.Contains("movement") => ReportDestinationKeys.PhysEdMovement,
            var n when n.Contains("dance")    => ReportDestinationKeys.Dance,
            var n when n.Contains("drama")    => ReportDestinationKeys.Drama,
            var n when n.Contains("music")    => ReportDestinationKeys.Music,
            var n when n.Contains("visual art") || n.Contains("visual arts")
                                              => ReportDestinationKeys.VisualArts,
            _ => null
        };
    }

    public record PdfFieldData(
        Dictionary<string, string> Fields,
        Dictionary<string, bool>   Checkboxes,
        Dictionary<string, string> Radios);
}
