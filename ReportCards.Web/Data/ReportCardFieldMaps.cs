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
    public const string Principal          = "school.principal";
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
    public const string StrengthsNextSteps        = "skills.strengthsNextSteps";
    public const string StrengthsNextStepsTerm1    = "skills.strengthsNextSteps.term1";
    public const string StrengthsNextStepsTerm2    = "skills.strengthsNextSteps.term2";

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

    // French sub-skills
    public const string FrenchListening     = "subject.french.listening";
    public const string FrenchSpeaking      = "subject.french.speaking";
    public const string FrenchReading       = "subject.french.reading";
    public const string FrenchWriting       = "subject.french.writing";
    public const string FrenchNotes         = "subject.french.notes";

    // Native Language notes
    public const string NativeLanguageNotes = "subject.nativeLanguage.notes";

    // Health sub-skills
    public const string PhysEdMovement      = "subject.physEd.movement";
    public const string PhysEdNotes         = "subject.physEd.notes";

    // Subject-level modifier checkboxes (whole-subject graded)
    public const string LanguageEslEld          = "subject.language.esleld";
    public const string LanguageIep             = "subject.language.iep";
    public const string LanguageNa              = "subject.language.na";
    public const string MathematicsEslEld       = "subject.mathematics.esleld";
    public const string MathematicsIep          = "subject.mathematics.iep";
    public const string MathematicsFrench       = "subject.mathematics.french";
    public const string ScienceAndTechEslEld    = "subject.scienceAndTech.esleld";
    public const string ScienceAndTechIep       = "subject.scienceAndTech.iep";
    public const string ScienceAndTechFrench    = "subject.scienceAndTech.french";
    public const string SocialStudiesEslEld     = "subject.socialStudies.esleld";
    public const string SocialStudiesIep        = "subject.socialStudies.iep";
    public const string SocialStudiesFrench     = "subject.socialStudies.french";
    public const string NativeLanguageEslEld    = "subject.nativeLanguage.esleld";
    public const string NativeLanguageIep       = "subject.nativeLanguage.iep";
    public const string NativeLanguageNa2       = "subject.nativeLanguage.na";
    public const string HealthEslEld            = "subject.health.esleld";
    public const string HealthIep               = "subject.health.iep";
    public const string HealthFrench            = "subject.health.french";
    public const string PhysEdEslEld            = "subject.physEd.esleld";
    public const string PhysEdIep               = "subject.physEd.iep";
    public const string PhysEdFrench            = "subject.physEd.french";
    public const string PhysEdMovementEslEld    = "subject.physEd.movement.esleld";
    public const string PhysEdMovementIep       = "subject.physEd.movement.iep";
    public const string PhysEdMovementFrench    = "subject.physEd.movement.french";

    // French strand-level modifier checkboxes
    public const string FrenchListeningEslEld   = "subject.french.listening.esleld";
    public const string FrenchListeningIep      = "subject.french.listening.iep";
    public const string FrenchSpeakingEslEld    = "subject.french.speaking.esleld";
    public const string FrenchSpeakingIep       = "subject.french.speaking.iep";
    public const string FrenchReadingEslEld     = "subject.french.reading.esleld";
    public const string FrenchReadingIep        = "subject.french.reading.iep";
    public const string FrenchWritingEslEld     = "subject.french.writing.esleld";
    public const string FrenchWritingIep        = "subject.french.writing.iep";
    public const string FrenchNa                = "subject.french.na";

    // Arts strand-level modifier checkboxes
    public const string DanceEslEld             = "subject.dance.esleld";
    public const string DanceIep                = "subject.dance.iep";
    public const string DanceFrench             = "subject.dance.french";
    public const string DanceNa                 = "subject.dance.na";
    public const string DramaEslEld             = "subject.drama.esleld";
    public const string DramaIep                = "subject.drama.iep";
    public const string DramaFrench             = "subject.drama.french";
    public const string DramaNa                 = "subject.drama.na";
    public const string MusicEslEld             = "subject.music.esleld";
    public const string MusicIep                = "subject.music.iep";
    public const string MusicFrench             = "subject.music.french";
    public const string MusicNa                 = "subject.music.na";
    public const string VisualArtsEslEld        = "subject.visualArts.esleld";
    public const string VisualArtsIep           = "subject.visualArts.iep";
    public const string VisualArtsFrench        = "subject.visualArts.french";
    public const string VisualArtsNa            = "subject.visualArts.na";
}

public static class ReportCardFieldMapSeeder
{
    /// <summary>
    /// Elementary Report Card (RC1 — two-term card).
    /// Field names verified against the actual AcroForm fields in the PDF.
    /// NOTE: PDF has a typo — "Responsibiity" (missing 'l') in both term fields.
    /// </summary>
    public static IEnumerable<(string DestKey, string PdfField)> ElementaryReportCardMaps()
    {
        // ── Page 1: Header ──────────────────────────────────────────
        yield return (ReportDestinationKeys.StudentName,    "Student");
        yield return (ReportDestinationKeys.StudentOen,     "OEN");
        yield return (ReportDestinationKeys.StudentGrade,   "Grade");
        yield return (ReportDestinationKeys.TeacherName,    "Teacher");
        yield return (ReportDestinationKeys.SchoolBoard,    "Board");
        yield return (ReportDestinationKeys.SchoolName,     "School");
        yield return (ReportDestinationKeys.SchoolAddress,  "Address");
        yield return (ReportDestinationKeys.SchoolPhone,    "Telephone");
        yield return (ReportDestinationKeys.DaysAbsent,     "DaysAbsent");
        yield return (ReportDestinationKeys.TotalDaysAbsent,"TotalDaysAbsent");
        yield return (ReportDestinationKeys.TimesLate,      "TimesLate");
        yield return (ReportDestinationKeys.TotalTimesLate, "TotalTimesLate");
        yield return (ReportDestinationKeys.StudentGrade,   "GradeInSeptember");

        // ── Page 1: Learning Skills ─────────────────────────────────
        yield return (ReportDestinationKeys.Responsibility  + ".term1", "Term1Responsibiity");  // typo in PDF
        yield return (ReportDestinationKeys.Responsibility  + ".term2", "Term2Responsibiity");  // typo in PDF
        yield return (ReportDestinationKeys.Organization    + ".term1", "Term1Organization");
        yield return (ReportDestinationKeys.Organization    + ".term2", "Term2Organization");
        yield return (ReportDestinationKeys.IndependentWork + ".term1", "Term1IndependentWork");
        yield return (ReportDestinationKeys.IndependentWork + ".term2", "Term2IndependentWork");
        yield return (ReportDestinationKeys.Collaboration   + ".term1", "Term1Collaboration");
        yield return (ReportDestinationKeys.Collaboration   + ".term2", "Term2Collaboration");
        yield return (ReportDestinationKeys.Initiative      + ".term1", "Term1Initiative");
        yield return (ReportDestinationKeys.Initiative      + ".term2", "Term2Initiative");
        yield return (ReportDestinationKeys.SelfRegulation  + ".term1", "Term1SelfRegulation");
        yield return (ReportDestinationKeys.SelfRegulation  + ".term2", "Term2SelfRegulation");
        yield return (ReportDestinationKeys.StrengthsNextSteps,          "StrengthsNextSteps");

        // ── Page 2: Language ──────────────────────────────────────────
        yield return (ReportDestinationKeys.Language        + ".term1", "LanguageTerm1");
        yield return (ReportDestinationKeys.Language        + ".term2", "LanguageTerm2");
        yield return (ReportDestinationKeys.LanguageNotes,               "LanguageNotes");

        // ── Page 2: French ───────────────────────────────────────────
        yield return (ReportDestinationKeys.French          + ".listening.term1", "FrenchListeningTerm1");
        yield return (ReportDestinationKeys.French          + ".listening.term2", "FrenchListeningTerm2");
        yield return (ReportDestinationKeys.French          + ".speaking.term1",  "FrenchSpeakingTerm1");
        yield return (ReportDestinationKeys.French          + ".speaking.term2",  "FrenchSpeakingTerm2");
        yield return (ReportDestinationKeys.French          + ".reading.term1",   "FrenchReadingTerm1");
        yield return (ReportDestinationKeys.French          + ".reading.term2",   "FrenchReadingTerm2");
        yield return (ReportDestinationKeys.French          + ".writing.term1",   "FrenchWritingTerm1");
        yield return (ReportDestinationKeys.French          + ".writing.term2",   "FrenchWritingTerm2");
        yield return (ReportDestinationKeys.French          + ".notes",           "FrenchNotes");

        // ── Page 2: Native Language ───────────────────────────────────
        yield return (ReportDestinationKeys.NativeLanguage  + ".term1", "NativeLanguageTerm1");
        yield return (ReportDestinationKeys.NativeLanguage  + ".term2", "NativeLanguageTerm2");
        yield return (ReportDestinationKeys.NativeLanguageNotes,         "NativeLanguageNotes");

        // ── Page 2: Mathematics ──────────────────────────────────────
        yield return (ReportDestinationKeys.Mathematics     + ".term1", "MathematicsTerm1");
        yield return (ReportDestinationKeys.Mathematics     + ".term2", "MathematicsTerm2");
        yield return (ReportDestinationKeys.MathematicsNotes,            "MathematicsNotes");

        // ── Page 2: Science & Technology ──────────────────────────────
        yield return (ReportDestinationKeys.ScienceAndTech  + ".term1", "ScienceAndTechTerm1");
        yield return (ReportDestinationKeys.ScienceAndTech  + ".term2", "ScienceAndTechTerm2");
        yield return (ReportDestinationKeys.ScienceAndTechNotes,         "ScienceAndTechNotes");

        // ── Page 3: Social Studies ────────────────────────────────────
        yield return (ReportDestinationKeys.SocialStudies   + ".term1", "SocialStudiesTerm1");
        yield return (ReportDestinationKeys.SocialStudies   + ".term2", "SocialStudiesTerm2");
        yield return (ReportDestinationKeys.SocialStudiesNotes,          "SocialStudiesNotes");

        // ── Page 3: Health & Physical Education ────────────────────────
        yield return (ReportDestinationKeys.Health          + ".term1", "HealthHealthyLivingTerm1");
        yield return (ReportDestinationKeys.Health          + ".term2", "HealthHealthyLivingTerm2");
        yield return (ReportDestinationKeys.PhysEd          + ".term1", "HealthActiveLivingTerm1");
        yield return (ReportDestinationKeys.PhysEd          + ".term2", "HealthActiveLivingTerm2");
        yield return (ReportDestinationKeys.PhysEdMovement  + ".term1", "HealthMovementTerm1");
        yield return (ReportDestinationKeys.PhysEdMovement  + ".term2", "HealthMovementTerm2");
        yield return (ReportDestinationKeys.PhysEdNotes,                 "HealthPhysEdNotes");

        // ── Page 3: The Arts ───────────────────────────────────────────
        yield return (ReportDestinationKeys.Dance           + ".term1", "DanceTerm1");
        yield return (ReportDestinationKeys.Dance           + ".term2", "DanceTerm2");
        yield return (ReportDestinationKeys.Drama           + ".term1", "DramaTerm1");
        yield return (ReportDestinationKeys.Drama           + ".term2", "DramaTerm2");
        yield return (ReportDestinationKeys.Music           + ".term1", "MusicTerm1");
        yield return (ReportDestinationKeys.Music           + ".term2", "MusicTerm2");
        yield return (ReportDestinationKeys.VisualArts      + ".term1", "VisualArtsTerm1");
        yield return (ReportDestinationKeys.VisualArts      + ".term2", "VisualArtsTerm2");
        yield return (ReportDestinationKeys.TheArtsNotes,                "TheArtsNotes");

        // ── Modifier checkboxes ──────────────────────────────────────────
        yield return (ReportDestinationKeys.LanguageEslEld,           "LanguageESLELD");
        yield return (ReportDestinationKeys.LanguageIep,              "LanguageIEP");
        yield return (ReportDestinationKeys.LanguageNa,               "LanguageNA");

        yield return (ReportDestinationKeys.FrenchNa,                 "FrenchNA");
        yield return (ReportDestinationKeys.FrenchListeningEslEld,    "LIsteningESLELD");  // PDF typo
        yield return (ReportDestinationKeys.FrenchListeningIep,       "ListeningIEP");
        yield return (ReportDestinationKeys.FrenchSpeakingEslEld,     "SpeakingESLELD");
        yield return (ReportDestinationKeys.FrenchSpeakingIep,        "SpeakingIEP");
        yield return (ReportDestinationKeys.FrenchReadingEslEld,      "ReadingESLELD");
        yield return (ReportDestinationKeys.FrenchReadingIep,         "ReadingIEP");
        yield return (ReportDestinationKeys.FrenchWritingEslEld,      "WritingESLELD");
        yield return (ReportDestinationKeys.FrenchWritingIep,         "WritingIEP");

        yield return (ReportDestinationKeys.NativeLanguageEslEld,     "NativeLanguageESLELD");
        yield return (ReportDestinationKeys.NativeLanguageIep,        "NativeLanguageIEP");
        yield return (ReportDestinationKeys.NativeLanguageNa2,        "NativeLanguageNA");

        yield return (ReportDestinationKeys.MathematicsEslEld,        "MathematicsESLELD");
        yield return (ReportDestinationKeys.MathematicsIep,           "MathematicsIEP");
        yield return (ReportDestinationKeys.MathematicsFrench,        "MathematicsFrench");

        yield return (ReportDestinationKeys.ScienceAndTechEslEld,     "ScienceAndTechESLELD");
        yield return (ReportDestinationKeys.ScienceAndTechIep,        "ScienceAndTechIEP");
        yield return (ReportDestinationKeys.ScienceAndTechFrench,     "ScienceAndTechFrench");

        yield return (ReportDestinationKeys.SocialStudiesEslEld,      "SocialStudiesESLELD");
        yield return (ReportDestinationKeys.SocialStudiesIep,         "SocialStudiesIEP");
        yield return (ReportDestinationKeys.SocialStudiesFrench,      "SocialStudiesFrench");

        yield return (ReportDestinationKeys.HealthEslEld,             "HealthyESLELD");
        yield return (ReportDestinationKeys.HealthIep,                "HealthyIEP");
        yield return (ReportDestinationKeys.HealthFrench,             "HealthyFrench");
        yield return (ReportDestinationKeys.PhysEdEslEld,             "MovementESLELD");
        yield return (ReportDestinationKeys.PhysEdIep,                "MovementIEP");
        yield return (ReportDestinationKeys.PhysEdFrench,             "MovementFrench");

        yield return (ReportDestinationKeys.DanceEslEld,              "DanceESLELD");
        yield return (ReportDestinationKeys.DanceIep,                 "DanceIEP");
        yield return (ReportDestinationKeys.DanceFrench,              "DanceFrench");
        yield return (ReportDestinationKeys.DanceNa,                  "DanceNA");
        yield return (ReportDestinationKeys.DramaEslEld,              "DramaESLELD");
        yield return (ReportDestinationKeys.DramaIep,                 "DramaIEP");
        yield return (ReportDestinationKeys.DramaFrench,              "DramaFrench");
        yield return (ReportDestinationKeys.DramaNa,                  "DramaNA");
        yield return (ReportDestinationKeys.MusicEslEld,              "MusicESLELD");
        yield return (ReportDestinationKeys.MusicIep,                 "MusicIEP");
        yield return (ReportDestinationKeys.MusicFrench,              "MusicFrench");
        yield return (ReportDestinationKeys.MusicNa,                  "MusicNA");
        yield return (ReportDestinationKeys.VisualArtsEslEld,         "VisualArtsESLELD");
        yield return (ReportDestinationKeys.VisualArtsIep,            "VisualArtsIEP");
        yield return (ReportDestinationKeys.VisualArtsFrench,         "VisualArtsFrench");
        yield return (ReportDestinationKeys.VisualArtsNa,             "VisualArtsNA");
    }
}
