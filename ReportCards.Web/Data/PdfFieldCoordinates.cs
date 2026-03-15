namespace ReportCards.Web.Data;

/// <summary>
/// Verified PDF field coordinates extracted from elementary-report-card.pdf via pypdf.
/// PDF coordinate system: origin is bottom-left, y increases upward.
/// Page size: 612 x 792 pts for all pages.
/// </summary>
public static class PdfFieldCoordinates
{
    public record FieldRect(string Name, int Page, double X0, double Y0, double X1, double Y1, string FieldType)
    {
        /// <summary>Convert from PDF coords (origin bottom-left) to screen coords (origin top-left).</summary>
        public (double left, double top, double right, double bottom) ToScreen(double pageHeight = 792)
            => (X0, pageHeight - Y1, X1, pageHeight - Y0);
    }

    public static readonly List<FieldRect> ElementaryReportCard = new()
    {
        // ── Page 1: Header ──────────────────────────────────────────────
        new("Student",          1,  60.2, 705.0, 210.2, 719.6, "Text"),
        new("OEN",              1, 289.7, 705.4, 370.4, 718.3, "Text"),
        new("DaysAbsent",       1, 425.5, 705.4, 478.6, 719.1, "Text"),
        new("TotalDaysAbsent",  1, 551.6, 705.4, 584.6, 718.3, "Text"),
        new("Grade",            1,  54.4, 688.3, 107.5, 702.0, "Text"),
        new("Teacher",          1, 156.1, 687.4, 367.1, 702.0, "Text"),
        new("TimesLate",        1, 426.2, 688.3, 479.3, 702.0, "Text"),
        new("TotalTimesLate",   1, 551.0, 688.7, 584.0, 701.5, "Text"),
        new("Board",            1,  52.6, 671.7, 259.4, 686.3, "Text"),
        new("School",           1, 294.4, 670.4, 584.5, 685.0, "Text"),
        new("Address",          1, 303.3, 636.8, 586.4, 666.2, "Text"),
        new("Principal",        1, 301.1, 619.8, 454.6, 632.6, "Text"),
        new("Telephone",        1, 505.4, 618.5, 586.0, 631.3, "Text"),
        new("GradeInSeptember", 1, 182.0, 590.2, 249.5, 612.2, "Text"),

        // ── Page 1: Learning Skills ─────────────────────────────────────
        new("Term1Responsibiity",   1, 248.5, 540.3, 277.4, 557.0, "Text"),
        new("Term2Responsibiity",   1, 277.3, 539.7, 306.2, 556.5, "Text"),
        new("Term1Organization",    1, 531.7, 539.7, 560.6, 556.5, "Text"),
        new("Term2Organization",    1, 561.3, 539.7, 590.2, 556.5, "Text"),
        new("Term1IndependentWork", 1, 248.7, 450.8, 277.7, 467.6, "Text"),
        new("Term2IndependentWork", 1, 277.3, 451.1, 306.2, 467.9, "Text"),
        new("Term1Collaboration",   1, 530.8, 450.7, 559.7, 467.4, "Text"),
        new("Term2Collaboration",   1, 560.5, 451.1, 589.4, 467.9, "Text"),
        new("Term1Initiative",      1, 248.7, 331.3, 277.7, 348.0, "Text"),
        new("Term2Initiative",      1, 277.3, 331.5, 306.2, 348.3, "Text"),
        new("Term1SelfRegulation",  1, 530.3, 331.4, 560.9, 348.2, "Text"),
        new("Term2SelfRegulation",  1, 561.3, 331.5, 590.2, 348.3, "Text"),

        // ── Page 2: Language ────────────────────────────────────────────
        new("LanguageNA",    2, 125.3, 718.8, 134.0, 727.7, "Checkbox"),
        new("LanguageESLELD",2,  29.5, 672.0,  38.3, 680.9, "Checkbox"),
        new("LanguageIEP",   2,  29.5, 655.9,  38.3, 664.8, "Checkbox"),
        new("LanguageTerm1", 2, 133.0, 689.5, 153.6, 705.9, "Text"),
        new("LanguageTerm2", 2, 152.9, 689.5, 173.4, 705.9, "Text"),
        new("LanguageNotes", 2, 173.0, 593.1, 585.9, 715.6, "Text"),

        // ── Page 2: French ──────────────────────────────────────────────
        new("FrenchNA",            2, 125.3, 578.3, 134.0, 587.2, "Checkbox"),
        new("LIsteningESLELD",     2,  29.5, 552.4,  38.3, 561.1, "Checkbox"),
        new("ListeningIEP",        2,  79.2, 552.4,  88.0, 561.1, "Checkbox"),
        new("SpeakingESLELD",      2,  29.5, 526.2,  38.3, 535.1, "Checkbox"),
        new("SpeakingIEP",         2,  79.2, 526.2,  88.0, 535.1, "Checkbox"),
        new("ReadingESLELD",       2,  29.5, 500.2,  38.3, 508.9, "Checkbox"),
        new("ReadingIEP",          2,  79.2, 500.2,  88.0, 508.9, "Checkbox"),
        new("WritingESLELD",       2,  29.5, 474.1,  38.3, 482.9, "Checkbox"),
        new("WritingIEP",          2,  79.2, 474.1,  88.0, 482.9, "Checkbox"),
        new("FrenchCore",          2,  29.5, 459.0,  38.3, 467.9, "Checkbox"),
        new("FrenchImmersion",     2,  62.4, 459.0,  71.2, 467.9, "Checkbox"),
        new("FrenchExtended",      2, 115.2, 459.0, 124.0, 467.9, "Checkbox"),
        new("FrenchListeningTerm1",2, 132.7, 550.7, 153.2, 567.1, "Text"),
        new("FrenchListeningTerm2",2, 152.5, 550.7, 173.1, 567.1, "Text"),
        new("FrenchSpeakingTerm1", 2, 132.3, 524.3, 152.9, 540.7, "Text"),
        new("FrenchSpeakingTerm2", 2, 152.7, 524.3, 172.9, 540.7, "Text"),
        new("FrenchReadingTerm1",  2, 132.5, 498.5, 153.1, 514.9, "Text"),
        new("FrenchReadingTerm2",  2, 152.9, 498.5, 173.1, 514.9, "Text"),
        new("FrenchWritingTerm1",  2, 132.7, 472.3, 153.2, 488.7, "Text"),
        new("FrenchWritingTerm2",  2, 153.1, 472.3, 173.3, 488.7, "Text"),
        new("FrenchNotes",         2, 175.0, 452.2, 587.9, 592.0, "Text"),

        // ── Page 2: Native Language ─────────────────────────────────────
        new("NativeLanguageESLELD", 2,  29.5, 361.3,  38.3, 370.1, "Checkbox"),
        new("NativeLanguageIEP",    2,  29.5, 345.2,  38.3, 354.0, "Checkbox"),
        new("NativeLanguageNA",     2,  29.5, 329.2,  38.3, 337.9, "Checkbox"),
        new("NativeLanguageTerm1",  2, 132.6, 407.0, 153.1, 423.5, "Text"),
        new("NativeLanguageTerm2",  2, 153.0, 407.0, 173.2, 423.5, "Text"),
        new("NativeLanguageNotes",  2, 175.0, 311.0, 587.9, 439.2, "Text"),

        // ── Page 2: Mathematics ─────────────────────────────────────────
        new("MathematicsESLELD",  2,  29.5, 248.9,  38.3, 257.6, "Checkbox"),
        new("MathematicsIEP",     2,  29.5, 232.8,  38.3, 241.6, "Checkbox"),
        new("MathematicsFrench",  2,  29.5, 216.6,  38.3, 225.5, "Checkbox"),
        new("MathematicsTerm1",   2, 132.5, 266.1, 153.1, 282.5, "Text"),
        new("MathematicsTerm2",   2, 152.9, 266.1, 173.1, 282.5, "Text"),
        new("MathematicsNotes",   2, 175.0, 169.9, 587.9, 298.1, "Text"),

        // ── Page 2: Science & Technology ────────────────────────────────
        new("ScienceAndTechESLELD",  2,  29.5, 108.0,  38.3, 116.9, "Checkbox"),
        new("ScienceAndTechIEP",     2,  29.5,  91.9,  38.3, 100.7, "Checkbox"),
        new("ScienceAndTechFrench",  2,  29.5,  75.8,  38.3,  84.6, "Checkbox"),
        new("ScienceAndTechTerm1",   2, 132.6, 125.3, 153.1, 141.7, "Text"),
        new("ScienceAndTechTerm2",   2, 153.0, 125.3, 173.2, 141.7, "Text"),
        new("ScienceAndTechNotes",   2, 173.2,  29.7, 586.2, 155.1, "Text"),

        // ── Page 3: Social Studies ───────────────────────────────────────
        new("SocialStudiesESLELD",  3,  29.5, 677.4,  38.3, 686.2, "Checkbox"),
        new("SocialStudiesIEP",     3,  29.5, 661.2,  38.3, 670.1, "Checkbox"),
        new("SocialStudiesFrench",  3,  29.5, 645.1,  38.3, 654.0, "Checkbox"),
        new("SocialStudiesTerm1",   3, 132.6, 688.1, 153.2, 704.5, "Text"),
        new("SocialStudiesTerm2",   3, 153.1, 688.1, 173.2, 704.5, "Text"),
        new("SocialStudiesNotes",   3, 172.9, 599.2, 585.9, 734.6, "Text"),

        // ── Page 3: Health & Physical Education ─────────────────────────
        new("HealthyESLELD",           3,  29.5, 543.6,  38.3, 552.5, "Checkbox"),
        new("HealthyIEP",              3,  76.6, 543.6,  85.3, 552.5, "Checkbox"),
        new("HealthyFrench",           3,  29.5, 530.0,  38.3, 538.9, "Checkbox"),
        new("HealthHealthyLivingTerm1",3, 132.6, 535.7, 153.1, 552.1, "Text"),
        new("HealthHealthyLivingTerm2",3, 153.0, 535.7, 173.2, 552.1, "Text"),
        new("HealthActiveLivingTerm1", 3, 132.5, 498.5, 153.1, 514.9, "Text"),
        new("HealthActiveLivingTerm2", 3, 152.9, 498.5, 173.1, 514.9, "Text"),
        new("MovementESLELD",          3,  29.5, 483.7,  38.3, 492.5, "Checkbox"),
        new("MovementIEP",             3,  76.6, 483.7,  85.3, 492.5, "Checkbox"),
        new("MovementFrench",          3,  29.5, 470.2,  38.3, 478.9, "Checkbox"),
        new("HealthMovementTerm1",     3, 132.7, 475.8, 153.2, 492.2, "Text"),
        new("HealthMovementTerm2",     3, 153.1, 475.8, 173.2, 492.2, "Text"),
        new("HealthPhysEdNotes",       3, 175.0, 461.0, 588.0, 596.4, "Text"),

        // ── Page 3: The Arts ─────────────────────────────────────────────
        new("DanceESLELD",    3,  29.5, 415.6,  38.4, 424.3, "Checkbox"),
        new("DanceIEP",       3,  79.6, 415.6,  88.3, 424.3, "Checkbox"),
        new("DanceFrench",    3,  79.6, 429.1,  88.3, 437.9, "Checkbox"),
        new("DanceNA",        3, 107.5, 415.6, 116.4, 424.3, "Checkbox"),
        new("DanceTerm1",     3, 132.6, 414.0, 153.2, 430.5, "Text"),
        new("DanceTerm2",     3, 153.1, 414.0, 173.2, 430.5, "Text"),
        new("DramaESLELD",    3,  29.5, 388.3,  38.4, 397.2, "Checkbox"),
        new("DramaIEP",       3,  79.6, 388.3,  88.3, 397.2, "Checkbox"),
        new("DramaFrench",    3,  79.6, 402.0,  88.3, 410.8, "Checkbox"),
        new("DramaNA",        3, 107.5, 388.3, 116.4, 397.2, "Checkbox"),
        new("DramaTerm1",     3, 132.6, 387.0, 153.1, 403.4, "Text"),
        new("DramaTerm2",     3, 153.0, 387.0, 173.2, 403.4, "Text"),
        new("MusicESLELD",    3,  29.5, 361.2,  38.4, 370.1, "Checkbox"),
        new("MusicIEP",       3,  79.6, 361.2,  88.3, 370.1, "Checkbox"),
        new("MusicFrench",    3,  79.6, 374.8,  88.3, 383.6, "Checkbox"),
        new("MusicNA",        3, 107.5, 361.2, 116.4, 370.1, "Checkbox"),
        new("MusicTerm1",     3, 132.5, 359.8, 153.1, 376.2, "Text"),
        new("MusicTerm2",     3, 152.9, 359.8, 173.1, 376.2, "Text"),
        new("VisualArtsESLELD",3,  29.5, 334.1,  38.4, 342.8, "Checkbox"),
        new("VisualArtsIEP",   3,  79.6, 334.1,  88.3, 342.8, "Checkbox"),
        new("VisualArtsFrench",3,  79.6, 347.6,  88.3, 356.4, "Checkbox"),
        new("VisualArtsNA",    3, 107.5, 334.1, 116.4, 342.8, "Checkbox"),
        new("VisualArtsTerm1", 3, 132.6, 332.5, 153.1, 348.9, "Text"),
        new("VisualArtsTerm2", 3, 153.0, 332.5, 173.2, 348.9, "Text"),
        new("TheArtsNotes",    3, 175.0, 324.4, 588.0, 459.7, "Text"),
        new("CustomESLELD",   3,  29.5, 288.6,  38.3, 297.4, "Checkbox"),
        new("CustomIEP",      3,  76.6, 288.6,  85.3, 297.4, "Checkbox"),
        new("CustomFrench",   3,  29.5, 275.0,  38.3, 283.8, "Checkbox"),
        new("CustomNA",       3,  76.6, 275.0,  85.3, 283.8, "Checkbox"),
        new("CustomTerm1",    3, 132.5, 302.5, 153.1, 319.0, "Text"),
        new("CustomTerm2",    3, 152.9, 302.5, 173.1, 319.0, "Text"),
        new("CustomNotes",    3, 175.0, 268.3, 588.0, 323.0, "Text"),

        // ── Page 3: ERS ──────────────────────────────────────────────────
        new("ERS",          3, 118.9, 240.7, 126.7, 248.7, "Checkbox"),
        new("BenchmarkYes", 3, 534.6, 239.8, 543.4, 249.2, "Checkbox"),
        new("BenchmarkNo",  3, 500.8, 240.5, 509.5, 249.2, "Checkbox"),
        new("ERSyear",      3, 248.5, 239.4, 288.6, 252.1, "Text"),
        new("ERSmonth",     3, 296.2, 239.4, 336.1, 252.1, "Text"),
        new("ERSday",       3, 343.7, 239.4, 383.6, 252.1, "Text"),
    };

    public static readonly List<FieldRect> KindergartenCommunicationOfLearning = new()
    {
        // ── Page 1: Header ──────────────────────────────────────────────
        new("Student",           1,  57.3, 694.5, 207.3, 709.1, "Text"),
        new("OEN",               1, 276.0, 694.6, 356.6, 707.5, "Text"),
        new("DaysAbsent",        1, 418.6, 694.3, 471.7, 708.0, "Text"),
        new("TotalDaysAbsent",   1, 548.7, 694.3, 581.7, 707.1, "Text"),
        new("TimesLate",         1, 419.3, 677.1, 472.4, 690.9, "Text"),
        new("TotalTimesLate",    1, 548.1, 677.6, 581.1, 690.4, "Text"),
        new("ReportPgYr1",       1,  55.5, 679.1,  64.3, 688.0, "Checkbox"),
        new("ReportPgYr2",       1, 115.6, 679.1, 124.4, 688.1, "Checkbox"),
        new("Teacher",           1,  61.1, 657.0, 272.2, 671.5, "Text"),
        new("School",            1,  55.1, 626.2, 302.3, 640.8, "Text"),
        new("Board",             1, 338.7, 627.2, 545.4, 641.8, "Text"),
        new("Address",           1,  60.8, 590.6, 294.2, 620.1, "Text"),
        new("Principal",         1,  61.9, 567.4, 195.1, 578.6, "Text"),
        new("Telephone",         1, 220.3, 565.4, 300.9, 578.3, "Text"),
        new("Date",              1, 427.6, 709.3, 565.9, 729.2, "Text"),
        // ── Page 1: Placement ─────────────────────────────────────────────
        new("SeptemberYear2",    1, 305.2, 545.7, 313.9, 554.6, "Checkbox"),
        new("SeptemberGd1",      1, 380.6, 545.7, 389.3, 554.6, "Checkbox"),
        // ── Page 1: Belonging & Contributing ───────────────────────────
        new("BelAndConESL",      1, 472.0, 523.6, 480.8, 532.5, "Checkbox"),
        new("BelAndConIEP",      1, 536.6, 523.7, 545.3, 532.5, "Checkbox"),
        new("BelAndConNotes",    1,  25.9, 304.1, 588.5, 480.5, "Text"),
        // ── Page 1: Self-Regulation & Well-Being ───────────────────────
        new("SelfRegAndWelESL",  1, 472.0, 278.0, 480.8, 286.9, "Checkbox"),
        new("SelfRegAndWelIEP",  1, 536.6, 278.1, 545.3, 287.0, "Checkbox"),
        new("SelfRegAndWellNotes",1, 26.3,  63.7, 589.0, 234.4, "Text"),
        // ── Page 2: Lit & Math Behaviours ───────────────────────────────
        new("LitAndMathESL",     2, 472.0, 747.6, 480.8, 756.5, "Checkbox"),
        new("LitAndMathIEP",     2, 536.6, 747.7, 545.3, 756.5, "Checkbox"),
        new("LitAndMathNotes",   2,  24.7, 546.5, 587.4, 709.8, "Text"),
        // ── Page 2: Problem Solving & Innovating ───────────────────────
        new("ProbAndInnESL",     2, 472.0, 523.6, 480.8, 532.5, "Checkbox"),
        new("ProbAndInnIEP",     2, 536.6, 523.7, 545.3, 532.5, "Checkbox"),
        new("ProbAndInnNotes",   2,  24.7, 322.6, 587.4, 485.9, "Text"),
    };

    /// <summary>Get field rects for a specific PDF filename.</summary>
    public static List<FieldRect> GetForFile(string fileName)
    {
        if (fileName.Equals("elementary-report-card.pdf", StringComparison.OrdinalIgnoreCase))
            return ElementaryReportCard;
        if (fileName.StartsWith("Kindergarten", StringComparison.OrdinalIgnoreCase))
            return KindergartenCommunicationOfLearning;
        return new();
    }
}
