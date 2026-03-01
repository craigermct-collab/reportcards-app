using Microsoft.EntityFrameworkCore;
using ReportCards.Web.Data;

namespace ReportCards.Web.Services;

/// <summary>
/// Stamps a CurriculumSchema template into a SchoolYear, creating frozen
/// YearCurriculum / YearClassOffering / YearSubjectOffering records.
///
/// After stamping, changes to the template do NOT affect the school year.
/// ReportDestinationKeys are auto-generated as stable slugs for PDF field mapping.
/// </summary>
public class CurriculumStampService
{
    private readonly SchoolDbContext _db;

    public CurriculumStampService(SchoolDbContext db) => _db = db;

    /// <summary>
    /// Stamps the given curriculum schema into the school year.
    /// Idempotent — if a YearCurriculum already exists for this year it is removed first.
    /// </summary>
    public async Task StampAsync(int schoolYearId, int curriculumSchemaId)
    {
        // Load template with full tree
        var schema = await _db.CurriculumSchemas
            .Include(s => s.GradeTemplates)
                .ThenInclude(g => g.Grade)
            .Include(s => s.GradeTemplates)
                .ThenInclude(g => g.ClassTemplates)
                    .ThenInclude(c => c.SubjectTemplates)
            .FirstOrDefaultAsync(s => s.Id == curriculumSchemaId)
            ?? throw new InvalidOperationException($"CurriculumSchema {curriculumSchemaId} not found.");

        // Remove any existing stamp for this year (re-stamp scenario)
        var existing = await _db.YearCurriculums
            .Where(y => y.SchoolYearId == schoolYearId)
            .ToListAsync();
        if (existing.Any())
            _db.YearCurriculums.RemoveRange(existing);

        // Create root YearCurriculum
        var yearCurriculum = new YearCurriculum
        {
            SchoolYearId      = schoolYearId,
            CurriculumSchemaId = curriculumSchemaId,
            AppliedAt         = DateTimeOffset.UtcNow
        };
        _db.YearCurriculums.Add(yearCurriculum);
        await _db.SaveChangesAsync(); // get yearCurriculum.Id

        // Stamp each grade → subject → strand
        foreach (var gradeTemplate in schema.GradeTemplates.OrderBy(g => g.SortOrder))
        {
            foreach (var subjectTemplate in gradeTemplate.ClassTemplates.OrderBy(c => c.SortOrder))
            {
                // Generate a stable slug for this subject:
                // e.g. "mathematics", "language", "health-and-physical-education"
                var subjectKey = Slugify(subjectTemplate.Name);

                var classOffering = new YearClassOffering
                {
                    YearCurriculumId          = yearCurriculum.Id,
                    GradeId                   = gradeTemplate.GradeId,
                    CurriculumClassTemplateId = subjectTemplate.Id,
                    IsEnabled                 = true,
                    ReportDestinationKey      = subjectKey
                };
                _db.YearClassOfferings.Add(classOffering);
                await _db.SaveChangesAsync(); // get classOffering.Id

                // Stamp strands
                foreach (var strandTemplate in subjectTemplate.SubjectTemplates.OrderBy(s => s.SortOrder))
                {
                    // e.g. "mathematics.number", "language.comprehension"
                    var strandKey = $"{subjectKey}.{Slugify(strandTemplate.Name)}";

                    _db.YearSubjectOfferings.Add(new YearSubjectOffering
                    {
                        YearClassOfferingId          = classOffering.Id,
                        CurriculumSubjectTemplateId  = strandTemplate.Id,
                        IsEnabled                    = true,
                        ReportDestinationKey         = strandKey
                    });
                }

                await _db.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Converts a display name to a stable lowercase hyphenated slug.
    /// e.g. "Health and Physical Education" → "health-and-physical-education"
    /// </summary>
    private static string Slugify(string name)
        => System.Text.RegularExpressions.Regex
            .Replace(name.ToLowerInvariant().Trim(), @"[^a-z0-9]+", "-")
            .Trim('-');
}
