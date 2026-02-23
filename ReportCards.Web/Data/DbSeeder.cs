using Microsoft.EntityFrameworkCore;

namespace ReportCards.Web.Data;

public static class DbSeeder
{
    public static async Task SeedReferenceDataAsync(SchoolDbContext db)
    {
        // ── ClassGroupTypes + Grades ────────────────────────────────
        if (!await db.ClassGroupTypes.AnyAsync())
        {
            var kindergarten = new ClassGroupType
            {
                Name = "Kindergarten",
                SortOrder = 1,
                Grades = new List<Grade>
                {
                    new() { Name = "JK", SortOrder = 1 },
                    new() { Name = "SK", SortOrder = 2 },
                }
            };

            var primary = new ClassGroupType
            {
                Name = "Primary",
                SortOrder = 2,
                Grades = new List<Grade>
                {
                    new() { Name = "Grade 1", SortOrder = 1 },
                    new() { Name = "Grade 2", SortOrder = 2 },
                    new() { Name = "Grade 3", SortOrder = 3 },
                    new() { Name = "Grade 4", SortOrder = 4 },
                    new() { Name = "Grade 5", SortOrder = 5 },
                }
            };

            db.ClassGroupTypes.AddRange(kindergarten, primary);
            await db.SaveChangesAsync();
        }

        // ── Grading Scales ──────────────────────────────────────────
        if (!await db.GradingScales.AnyAsync())
        {
            // Kindergarten 3-level
            var kinderScale = new GradingScale
            {
                Name = "Kindergarten 3-Level",
                ValueType = GradingValueType.OptionList,
                Options = new List<GradingScaleOption>
                {
                    new() { Label = "Needs Assistance",  Code = "NA", SortOrder = 1 },
                    new() { Label = "At Class Level",    Code = "CL", SortOrder = 2 },
                    new() { Label = "Excelling",         Code = "EX", SortOrder = 3 },
                }
            };

            // Primary letter grades
            var letterScale = new GradingScale
            {
                Name = "Letter Grades",
                ValueType = GradingValueType.OptionList,
                Options = new List<GradingScaleOption>
                {
                    new() { Label = "A+", SortOrder = 1 },
                    new() { Label = "A",  SortOrder = 2 },
                    new() { Label = "A-", SortOrder = 3 },
                    new() { Label = "B+", SortOrder = 4 },
                    new() { Label = "B",  SortOrder = 5 },
                    new() { Label = "B-", SortOrder = 6 },
                    new() { Label = "C+", SortOrder = 7 },
                    new() { Label = "C",  SortOrder = 8 },
                    new() { Label = "C-", SortOrder = 9 },
                    new() { Label = "D+", SortOrder = 10 },
                    new() { Label = "D",  SortOrder = 11 },
                    new() { Label = "D-", SortOrder = 12 },
                    new() { Label = "E",  SortOrder = 13 },
                }
            };

            // Percent numeric
            var percentScale = new GradingScale
            {
                Name = "Percent",
                ValueType = GradingValueType.Numeric,
                MinValue = 0,
                MaxValue = 100,
                Step = 1,
                DisplaySuffix = "%"
            };

            db.GradingScales.AddRange(kinderScale, letterScale, percentScale);
            await db.SaveChangesAsync();
        }
    }
}
