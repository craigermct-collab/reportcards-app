namespace ReportCards.Web.Data;

public static class ReportDestinationKeys
{
    public const string StudentName         = "student.name";
    public const string StudentOen          = "student.oen";
    public const string StudentGrade        = "student.grade";
    public const string TeacherName         = "teacher.name";
    public const string SchoolName          = "school.name";
    public const string SchoolBoard         = "school.board";
    public const string SchoolAddress       = "school.address";
    public const string SchoolPhone         = "school.phone";
    public const string TermDate            = "term.date";
    public const string DaysAbsent          = "attendance.daysAbsent";
    public const string TotalDaysAbsent     = "attendance.totalDaysAbsent";
    public const string TimesLate           = "attendance.timesLate";
    public const string TotalTimesLate      = "attendance.totalTimesLate";

    public const string Responsibility      = "skills.responsibility";
    public const string Organization        = "skills.organization";
    public const string IndependentWork     = "skills.independentWork";
    public const string Collaboration       = "skills.collaboration";
    public const string Initiative          = "skills.initiative";
    public const string SelfRegulation      = "skills.selfRegulation";
    public const string StrengthsNextSteps  = "skills.strengthsNextSteps";

    public const string Language            = "subject.language";
    public const string LanguageNotes       = "subject.language.notes";
    public const string French              = "subject.french";
    public const string NativeLanguage      = "subject.nativeLanguage";
    public const string Mathematics         = "subject.mathematics";
    public const string MathematicsNotes    = "subject.mathematics.notes";
    public const string ScienceAndTech      = "subject.scienceAndTech";
    public const string ScienceAndTechNotes = "subject.scienceAndTech.notes";
    public const string SocialStudies       = "subject.socialStudies";
    public const string SocialStudiesNotes  = "subject.socialStudies.notes";
    public const string Health              = "subject.health";
    public const string PhysEd              = "subject.physEd";
    public const string Dance               = "subject.dance";
    public const string Drama               = "subject.drama";
    public const string Music               = "subject.music";
    public const string VisualArts          = "subject.visualArts";
    public const string TheArtsNotes        = "subject.theArts.notes";
}

public static class ReportCardFieldMapSeeder
{
    /// <summary>Elementary Progress Report (RC2 — fall term). Single-term card.</summary>
    public static IEnumerable<(string DestKey, string PdfField)> ProgressReportMaps()
    {
        yield return (ReportDestinationKeys.StudentName,        "Student");
        yield return (ReportDestinationKeys.StudentOen,         "OEN");
        yield return (ReportDestinationKeys.StudentGrade,       "Grade");
        yield return (ReportDestinationKeys.TeacherName,        "Teacher");
        yield return (ReportDestinationKeys.SchoolName,         "School");
        yield return (ReportDestinationKeys.SchoolBoard,        "Board");
        yield return (ReportDestinationKeys.SchoolAddress,      "Address");
        yield return (ReportDestinationKeys.SchoolPhone,        "Telephone");
        yield return (ReportDestinationKeys.DaysAbsent,         "Days Absent");
        yield return (ReportDestinationKeys.TotalDaysAbsent,    "Total Days Absent");
        yield return (ReportDestinationKeys.TimesLate,          "Times Late");
        yield return (ReportDestinationKeys.TotalTimesLate,     "Total Times Late");
        yield return (ReportDestinationKeys.Responsibility,     "Responsibility");
        yield return (ReportDestinationKeys.Organization,       "Organization");
        yield return (ReportDestinationKeys.IndependentWork,    "Independent Work");
        yield return (ReportDestinationKeys.Collaboration,      "Collaboration");
        yield return (ReportDestinationKeys.Initiative,         "Initiative");
        yield return (ReportDestinationKeys.SelfRegulation,     "Self Regulation");
        yield return (ReportDestinationKeys.StrengthsNextSteps, "StrengthsNext Steps for Improvement");
        yield return (ReportDestinationKeys.Language,           "LanguageSkill");
        yield return (ReportDestinationKeys.French,             "FrenchSkill");
        yield return (ReportDestinationKeys.NativeLanguage,     "NativeLanguageSkill");
        yield return (ReportDestinationKeys.Mathematics,        "MathematicsSkill");
        yield return (ReportDestinationKeys.ScienceAndTech,     "ScienceTechSkill");
        yield return (ReportDestinationKeys.SocialStudies,      "SocialStudiesSkill");
        yield return (ReportDestinationKeys.Health,             "HealthSkill");
        yield return (ReportDestinationKeys.PhysEd,             "PhysEdSkill");
        yield return (ReportDestinationKeys.Dance,              "DanceSkill");
        yield return (ReportDestinationKeys.Drama,              "DramaSkill");
        yield return (ReportDestinationKeys.Music,              "MusicSkill");
        yield return (ReportDestinationKeys.VisualArts,         "VisualArtsSkill");
    }

    /// <summary>
    /// Elementary Report Card (RC1 — winter/spring terms). Two-term card.
    /// NOTE: RC1 header has no Student/OEN/Teacher/School fillable fields.
    /// NOTE: PDF has typo "Term2Responsibiity" (missing 'l').
    /// </summary>
    public static IEnumerable<(string DestKey, string PdfField)> ElementaryReportCardMaps()
    {
        yield return (ReportDestinationKeys.SchoolBoard,                  "Board");
        yield return (ReportDestinationKeys.SchoolAddress,                "Address");
        yield return (ReportDestinationKeys.SchoolPhone,                  "Telephone");
        yield return (ReportDestinationKeys.StudentGrade,                 "GradeInSeptember");
        yield return (ReportDestinationKeys.TotalTimesLate,               "TotalTimesLate");
        yield return (ReportDestinationKeys.TimesLate,                    "TimesLate");

        // Learning Skills (RC1 only has Term2 columns for skills)
        yield return (ReportDestinationKeys.Responsibility  + ".term2",   "Term2Responsibiity");
        yield return (ReportDestinationKeys.Organization    + ".term2",   "Term2Organization");
        yield return (ReportDestinationKeys.IndependentWork + ".term2",   "Term2IndependentWork");
        yield return (ReportDestinationKeys.Collaboration   + ".term2",   "Term2Collaboration");
        yield return (ReportDestinationKeys.Initiative      + ".term2",   "Term2Initiative");
        yield return (ReportDestinationKeys.SelfRegulation  + ".term2",   "Term2SelfRegulation");

        // Academic subjects
        yield return (ReportDestinationKeys.LanguageNotes,                "LanguageNotes");
        yield return (ReportDestinationKeys.NativeLanguage  + ".term1",   "NativeLanguageTerm1");
        yield return (ReportDestinationKeys.NativeLanguage  + ".term2",   "NativeLanguageTerm2");
        yield return (ReportDestinationKeys.Mathematics     + ".term1",   "MathematicsTerm1");
        yield return (ReportDestinationKeys.Mathematics     + ".term2",   "MathematicsTerm2");
        yield return (ReportDestinationKeys.MathematicsNotes,             "MathematicsNotes");
        yield return (ReportDestinationKeys.ScienceAndTech  + ".term1",   "ScienceAndTechTerm1");
        yield return (ReportDestinationKeys.ScienceAndTech  + ".term2",   "ScienceAndTechTerm2");
        yield return (ReportDestinationKeys.ScienceAndTechNotes,          "ScienceAndTechNotes");
        yield return (ReportDestinationKeys.SocialStudies   + ".term1",   "SocialStudiesTerm1");
        yield return (ReportDestinationKeys.SocialStudies   + ".term2",   "SocialStudiesTerm2");
        yield return (ReportDestinationKeys.SocialStudiesNotes,           "SocialStudiesNotes");
        yield return (ReportDestinationKeys.Dance           + ".term1",   "DanceTerm1");
        yield return (ReportDestinationKeys.Dance           + ".term2",   "DanceTerm2");
        yield return (ReportDestinationKeys.Drama           + ".term1",   "DramaTerm1");
        yield return (ReportDestinationKeys.Drama           + ".term2",   "DramaTerm2");
        yield return (ReportDestinationKeys.Music           + ".term1",   "MusicTerm1");
        yield return (ReportDestinationKeys.Music           + ".term2",   "MusicTerm2");
        yield return (ReportDestinationKeys.VisualArts      + ".term1",   "VisualArtsTerm1");
        yield return (ReportDestinationKeys.VisualArts      + ".term2",   "VisualArtsTerm2");
        yield return (ReportDestinationKeys.TheArtsNotes,                 "TheArtsNotes");
    }
}
