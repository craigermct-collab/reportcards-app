using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ReportCards.Web.Data;

namespace ReportCards.Web.Services
{
    /// <summary>
    /// Generates filled PDF report cards by:
    /// 1. Loading student assessment data for a term
    /// 2. Mapping DB fields to PDF field IDs via ReportTemplateFieldMap
    /// 3. Calling fill_report_card.py to produce a filled PDF
    /// </summary>
    public class ReportCardGeneratorService
    {
        private readonly SchoolDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ReportCardGeneratorService> _logger;

        private const string TemplatesFolder = "ReportCardTemplates";
        private const string FillScriptName  = "fill_report_card.py";

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

            var fieldMaps = await _db.ReportTemplateFieldMaps
                .Where(m => m.TermInstanceId == termInstanceId)
                .ToListAsync();
            var mapByKey = fieldMaps.ToDictionary(m => m.ReportDestinationKey, m => m.PdfFieldName);

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
            SetField(fields, mapByKey, ReportDestinationKeys.StudentName,     $"{student.FirstName} {student.LastName}");
            SetField(fields, mapByKey, ReportDestinationKeys.StudentOen,      student.OenNumber ?? "");
            SetField(fields, mapByKey, ReportDestinationKeys.StudentGrade,    enrollment.Grade?.Name ?? "");
            SetField(fields, mapByKey, ReportDestinationKeys.TeacherName,     teacher?.DisplayName ?? "");
            SetField(fields, mapByKey, ReportDestinationKeys.SchoolName,      Cfg(SchoolConfigKeys.SchoolName));
            SetField(fields, mapByKey, ReportDestinationKeys.SchoolBoard,     "KinderKollege");
            SetField(fields, mapByKey, ReportDestinationKeys.SchoolAddress,   Cfg(SchoolConfigKeys.Address));
            SetField(fields, mapByKey, ReportDestinationKeys.SchoolPhone,     Cfg(SchoolConfigKeys.ContactPhone));
            SetField(fields, mapByKey, ReportDestinationKeys.DaysAbsent,      absences.ToString());
            SetField(fields, mapByKey, ReportDestinationKeys.TotalDaysAbsent, absences.ToString());
            SetField(fields, mapByKey, ReportDestinationKeys.TimesLate,       lates.ToString());
            SetField(fields, mapByKey, ReportDestinationKeys.TotalTimesLate,  lates.ToString());

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
        // PRIVATE: FILL PDF
        // ─────────────────────────────────────────────────────────────

        private async Task<byte[]> FillPdfAsync(ReportCardTemplate template, PdfFieldData data)
        {
            var templatePath = Path.Combine(_env.ContentRootPath, TemplatesFolder, template.FileName);
            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Template file not found: {templatePath}");

            var scriptPath = Path.Combine(_env.ContentRootPath, FillScriptName);
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException($"Fill script not found: {scriptPath}");

            var tmpDir     = Path.GetTempPath();
            var valuesPath = Path.Combine(tmpDir, $"rc_values_{Guid.NewGuid():N}.json");
            var outputPath = Path.Combine(tmpDir, $"rc_output_{Guid.NewGuid():N}.pdf");

            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented        = false,
                });
                await File.WriteAllTextAsync(valuesPath, json);

                var psi = new ProcessStartInfo
                {
                    FileName               = "python3",
                    Arguments              = $"\"{scriptPath}\" \"{templatePath}\" \"{valuesPath}\" \"{outputPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                };

                using var proc = Process.Start(psi)
                    ?? throw new InvalidOperationException("Failed to start python3 process.");

                var stderr = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();

                if (proc.ExitCode != 0)
                    throw new InvalidOperationException(
                        $"fill_report_card.py failed (exit {proc.ExitCode}): {stderr}");

                return await File.ReadAllBytesAsync(outputPath);
            }
            finally
            {
                if (File.Exists(valuesPath)) File.Delete(valuesPath);
                if (File.Exists(outputPath)) File.Delete(outputPath);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────

        private static void SetField(
            Dictionary<string, string> fields,
            Dictionary<string, string> map,
            string destKey,
            string value)
        {
            if (map.TryGetValue(destKey, out var pdfField))
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
