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

        // ── Curriculum Template (seed) ────────────────────────────────
        if (!await db.CurriculumSchemas.AnyAsync())
        {
            var grades = await db.Grades
                .Include(g => g.ClassGroupType)
                .OrderBy(g => g.ClassGroupType!.SortOrder)
                .ThenBy(g => g.SortOrder)
                .ToListAsync();

            // Subjects: name, code, gradedAtStrandLevel, strands[]
            var subjectDefs = new[]
            {
                (Name: "Mathematics",           Code: "MATH", PerStrand: true,  Strands: new[] { "Number Sense", "Algebraic Thinking", "Data Management", "Spatial Reasoning", "Financial Literacy" }),
                (Name: "Language",              Code: "LANG", PerStrand: true,  Strands: new[] { "Reading Comprehension", "Written Expression", "Oral Communication", "Media Literacy" }),
                (Name: "Science",               Code: "SCI",  PerStrand: true,  Strands: new[] { "Life Systems", "Matter and Energy", "Earth and Space", "Scientific Investigation" }),
                (Name: "French",                Code: "FRE",  PerStrand: true,  Strands: new[] { "Listening and Speaking", "Reading in French", "Writing in French" }),
                (Name: "Health and Physical Education", Code: "HPE", PerStrand: false, Strands: Array.Empty<string>()),
                (Name: "Arts",                  Code: "ART",  PerStrand: false, Strands: Array.Empty<string>()),
                (Name: "Social Studies",         Code: "SS",   PerStrand: false, Strands: Array.Empty<string>()),
            };

            // Lorem-ish sub-strand guidance snippets (4-word-style names + blurb)
            var subStrandSuffixes = new[]
            {
                (Name: "Concepts and Vocabulary",     Desc: "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."),
                (Name: "Skills and Application",      Desc: "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat."),
                (Name: "Problem Solving Strategies",  Desc: "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur."),
            };

            var schema = new CurriculumSchema
            {
                Name        = "Ontario Curriculum 2026",
                Description = "Seeded template — KG + Grades 1–5 with standard KinderKollege subjects.",
                Version     = "1.0",
                UploadedAt  = DateTimeOffset.UtcNow
            };
            db.CurriculumSchemas.Add(schema);
            await db.SaveChangesAsync();

            int gradeSort = 0;
            foreach (var grade in grades)
            {
                gradeSort++;
                var gradeTemplate = new CurriculumGradeTemplate
                {
                    CurriculumSchemaId = schema.Id,
                    GradeId            = grade.Id,
                    SortOrder          = gradeSort
                };
                db.CurriculumGradeTemplates.Add(gradeTemplate);
                await db.SaveChangesAsync();

                int subjSort = 0;
                foreach (var subj in subjectDefs)
                {
                    subjSort++;
                    var classTemplate = new CurriculumClassTemplate
                    {
                        CurriculumGradeTemplateId = gradeTemplate.Id,
                        Name                      = subj.Name,
                        Code                      = subj.Code,
                        SortOrder                 = subjSort,
                        GradedAtStrandLevel       = subj.PerStrand
                    };
                    db.CurriculumClassTemplates.Add(classTemplate);
                    await db.SaveChangesAsync();

                    int strandSort = 0;
                    foreach (var strandName in subj.Strands)
                    {
                        strandSort++;
                        var strandTemplate = new CurriculumSubjectTemplate
                        {
                            CurriculumClassTemplateId = classTemplate.Id,
                            Name                      = strandName,
                            Code                      = strandName.Split(' ')[0].ToUpper()[..Math.Min(3, strandName.Split(' ')[0].Length)],
                            SortOrder                 = strandSort
                        };
                        db.CurriculumSubjectTemplates.Add(strandTemplate);
                        await db.SaveChangesAsync();

                        // Add 3 sub-strands per strand with lorem guidance text
                        int ssSort = 0;
                        foreach (var ss in subStrandSuffixes)
                        {
                            ssSort++;
                            db.CurriculumSubStrands.Add(new CurriculumSubStrand
                            {
                                CurriculumSubjectTemplateId = strandTemplate.Id,
                                Name                        = $"{strandName} — {ss.Name}",
                                Code                        = $"{strandTemplate.Code}{ssSort}",
                                SortOrder                   = ssSort,
                                Description                 = ss.Desc
                            });
                        }
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        // ── Subject Modifier Templates ──────────────────────────────
        if (!await db.SubjectModifierTemplates.AnyAsync())
        {
            db.SubjectModifierTemplates.AddRange(
                new SubjectModifierTemplate
                {
                    Name        = "Standard (3 options)",
                    OptionsJson = "[\"ESL/ELD\",\"IEP\",\"French\"]",
                    IsSystem    = true,
                    CreatedAt   = DateTimeOffset.UtcNow
                },
                new SubjectModifierTemplate
                {
                    Name        = "Extended (4 options)",
                    OptionsJson = "[\"ESL/ELD\",\"IEP\",\"French\",\"N/A\"]",
                    IsSystem    = true,
                    CreatedAt   = DateTimeOffset.UtcNow
                }
            );
            await db.SaveChangesAsync();
        }

        // ── Ontario Curriculum Schema (real data from CSV) ──────────
        var ontarioCurriculumExists = await db.CurriculumSchemas
            .AnyAsync(s => s.Name == "Ontario Elementary Curriculum (Grades 1-6)");

        if (!ontarioCurriculumExists)
        {
            var primaryGrades = await db.Grades
                .Include(g => g.ClassGroupType)
                .Where(g => g.ClassGroupType!.Name == "Primary")
                .OrderBy(g => g.SortOrder)
                .ToListAsync();

            // Map grade name → Grade entity
            var gradeMap = primaryGrades.ToDictionary(
                g => g.SortOrder, // SortOrder 1-5 = Grade 1-5 (we'll extend to 6 if needed)
                g => g
            );

            // ── Science and Technology ─────────────────────────────
            // One subject group, one subject per grade, 5 strands (A-E)
            var scienceStrands = new[]
            {
                (Code: "A", Name: "STEM Skills and Connections"),
                (Code: "B", Name: "Life Systems"),
                (Code: "C", Name: "Matter and Energy"),
                (Code: "D", Name: "Structures and Mechanisms"),
                (Code: "E", Name: "Earth and Space Systems"),
            };

            // ── Health and Physical Education ──────────────────────
            // Two subjects per grade: Healthy Living + Physical Education
            // Each has 4 strands (A-D)
            var healthSubjects = new[]
            {
                (
                    Name: "Healthy Education: Healthy Living",
                    Code: "HE",
                    Strands: new[]
                    {
                        (Code: "A", Name: "Social-Emotional Learning Skills"),
                        (Code: "B", Name: "Active Living"),
                        (Code: "C", Name: "Movement Competence: Skills, Concepts, and Strategies"),
                        (Code: "D", Name: "Healthy Living"),
                    }
                ),
                (
                    Name: "Physical Education: Active Living, Movement Competence",
                    Code: "PE",
                    Strands: new[]
                    {
                        (Code: "A", Name: "Social-Emotional Learning Skills"),
                        (Code: "B", Name: "Active Living"),
                        (Code: "C", Name: "Movement Competence: Skills, Concepts, and Strategies"),
                        (Code: "D", Name: "Healthy Living"),
                    }
                ),
            };

            var schema = new CurriculumSchema
            {
                Name        = "Ontario Elementary Curriculum (Grades 1-6)",
                Description = "Official Ontario Ministry of Education curriculum — Science and Technology, Health and Physical Education (Grades 1-6)",
                Version     = "2022",
                UploadedAt  = DateTimeOffset.UtcNow,
            };
            db.CurriculumSchemas.Add(schema);
            await db.SaveChangesAsync();

            // Grades 1-5 exist in seed; add Grade 6 if missing
            var primaryCgt = await db.ClassGroupTypes.FirstAsync(c => c.Name == "Primary");
            var grade6 = await db.Grades.FirstOrDefaultAsync(g => g.Name == "Grade 6" && g.ClassGroupTypeId == primaryCgt.Id);
            if (grade6 == null)
            {
                grade6 = new Grade { Name = "Grade 6", SortOrder = 6, ClassGroupTypeId = primaryCgt.Id };
                db.Grades.Add(grade6);
                await db.SaveChangesAsync();
            }

            // Build full grade list 1-6
            var allPrimaryGrades = await db.Grades
                .Include(g => g.ClassGroupType)
                .Where(g => g.ClassGroupType!.Name == "Primary")
                .OrderBy(g => g.SortOrder)
                .ToListAsync();

            foreach (var grade in allPrimaryGrades)
            {
                var gradeTemplate = new CurriculumGradeTemplate
                {
                    CurriculumSchemaId = schema.Id,
                    GradeId            = grade.Id,
                    SortOrder          = grade.SortOrder,
                };
                db.CurriculumGradeTemplates.Add(gradeTemplate);
                await db.SaveChangesAsync();

                // ── Subject Group 1: Science and Technology ────────
                var sciClass = new CurriculumClassTemplate
                {
                    CurriculumGradeTemplateId = gradeTemplate.Id,
                    Name                      = "Science and Technology",
                    Code                      = "SCI-TECH",
                    GradedAtStrandLevel       = false,
                    SortOrder                 = 1,
                };
                db.CurriculumClassTemplates.Add(sciClass);
                await db.SaveChangesAsync();

                // One subject under Science and Technology
                var sciSubject = new CurriculumSubjectTemplate
                {
                    CurriculumClassTemplateId = sciClass.Id,
                    Name                      = "Science And Technology",
                    Code                      = "SCI",
                    SortOrder                 = 1,
                };
                db.CurriculumSubjectTemplates.Add(sciSubject);
                await db.SaveChangesAsync();

                int sciStrandSort = 0;
                foreach (var strand in scienceStrands)
                {
                    sciStrandSort++;
                    db.CurriculumSubStrands.Add(new CurriculumSubStrand
                    {
                        CurriculumSubjectTemplateId = sciSubject.Id,
                        Name                        = strand.Name,
                        Code                        = strand.Code,
                        SortOrder                   = sciStrandSort,
                    });
                }
                await db.SaveChangesAsync();

                // ── Subject Group 2: Health and Physical Education ──
                var hpeClass = new CurriculumClassTemplate
                {
                    CurriculumGradeTemplateId = gradeTemplate.Id,
                    Name                      = "Health and Physical Education",
                    Code                      = "HPE",
                    GradedAtStrandLevel       = false,
                    SortOrder                 = 2,
                };
                db.CurriculumClassTemplates.Add(hpeClass);
                await db.SaveChangesAsync();

                int hpeSubjectSort = 0;
                foreach (var hpeSub in healthSubjects)
                {
                    hpeSubjectSort++;
                    var hpeSubject = new CurriculumSubjectTemplate
                    {
                        CurriculumClassTemplateId = hpeClass.Id,
                        Name                      = hpeSub.Name,
                        Code                      = hpeSub.Code,
                        SortOrder                 = hpeSubjectSort,
                    };
                    db.CurriculumSubjectTemplates.Add(hpeSubject);
                    await db.SaveChangesAsync();

                    int hpeStrandSort = 0;
                    foreach (var strand in hpeSub.Strands)
                    {
                        hpeStrandSort++;
                        db.CurriculumSubStrands.Add(new CurriculumSubStrand
                        {
                            CurriculumSubjectTemplateId = hpeSubject.Id,
                            Name                        = strand.Name,
                            Code                        = strand.Code,
                            SortOrder                   = hpeStrandSort,
                        });
                    }
                    await db.SaveChangesAsync();
                }
            }
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

            // Ontario Provincial (E/G/S/N) — used on Ontario report cards
            var ontarioScale = new GradingScale
            {
                Name = "Ontario E/G/S/N",
                ValueType = GradingValueType.OptionList,
                Options = new List<GradingScaleOption>
                {
                    new() { Label = "E", Code = "E", SortOrder = 1 },   // Excellent
                    new() { Label = "G", Code = "G", SortOrder = 2 },   // Good
                    new() { Label = "S", Code = "S", SortOrder = 3 },   // Satisfactory
                    new() { Label = "N", Code = "N", SortOrder = 4 },   // Needs Improvement
                }
            };

            db.GradingScales.AddRange(kinderScale, letterScale, percentScale, ontarioScale);
            await db.SaveChangesAsync();
        }

    }
}
