namespace ReportCards.Web.Data;

// ═══════════════════════════════════════════════════════════════════
// Z) APP CONFIGURATION
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// Key/value store for school-wide app configuration.
/// Add new keys as the app grows — no migrations needed for new settings.
/// Well-known keys are defined as constants in SchoolConfigKeys.
/// </summary>
public class SchoolConfig
{
    public int Id { get; set; }
    public required string Key { get; set; }
    public string? Value { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>Well-known SchoolConfig key constants.</summary>
public static class SchoolConfigKeys
{
    public const string SchoolName        = "school.name";
    public const string LogoUrl           = "school.logo_url";
    public const string PrimaryColor      = "theme.primary_color";
    public const string SecondaryColor    = "theme.secondary_color";
    public const string NavDarkColor      = "theme.nav_dark_color";
    public const string ContactEmail      = "school.contact_email";
    public const string ContactPhone      = "school.contact_phone";
    public const string Address           = "school.address";
}

// ═══════════════════════════════════════════════════════════════════
// A) REFERENCE / IMMUTABLE DATA
// ═══════════════════════════════════════════════════════════════════

/// <summary>Immutable top-level grouping: "Kindergarten", "Primary"</summary>
public class ClassGroupType
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }

    public List<Grade> Grades { get; set; } = new();
    public List<ClassGroupInstance> ClassGroupInstances { get; set; } = new();
    public List<TermGradeGradingRule> GradingRules { get; set; } = new();
    public List<TeacherAssignment> TeacherAssignments { get; set; } = new();
}

/// <summary>Immutable grade levels: JK, SK, Grade 1 … Unique per ClassGroupType + Name.</summary>
public class Grade
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }

    public int ClassGroupTypeId { get; set; }
    public ClassGroupType? ClassGroupType { get; set; }

    public List<Enrollment> Enrollments { get; set; } = new();
    public List<TermGradeGradingRule> GradingRules { get; set; } = new();
    public List<CurriculumGradeTemplate> CurriculumGradeTemplates { get; set; } = new();
    public List<YearClassOffering> YearClassOfferings { get; set; } = new();
    public List<TeacherAssignment> TeacherAssignments { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// B) GRADING SCALES
// ═══════════════════════════════════════════════════════════════════

public enum GradingValueType { OptionList, Numeric }

public class GradingScale
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public GradingValueType ValueType { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? Step { get; set; }
    public string? DisplaySuffix { get; set; }

    public List<GradingScaleOption> Options { get; set; } = new();
    public List<TermGradeGradingRule> GradingRules { get; set; } = new();
    public List<StudentLearningItem> StudentLearningItems { get; set; } = new();
}

/// <summary>One option in an OptionList scale. Unique per GradingScale + Label.</summary>
public class GradingScaleOption
{
    public int Id { get; set; }
    public required string Label { get; set; }
    public string? Code { get; set; }
    public int SortOrder { get; set; }

    public int GradingScaleId { get; set; }
    public GradingScale? GradingScale { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// C) SCHOOL YEAR + TERM INSTANCES
// ═══════════════════════════════════════════════════════════════════

public class SchoolYear
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>The curriculum template this school year is based on. Set at creation time.</summary>
    public int? CurriculumSchemaId { get; set; }
    public CurriculumSchema? CurriculumSchema { get; set; }

    public List<TermInstance> TermInstances { get; set; } = new();
    public List<YearCurriculum> YearCurriculums { get; set; } = new();
    public List<SchoolCalendarException> CalendarExceptions { get; set; } = new();
    public List<ClassGroupReportFormat> ClassGroupReportFormats { get; set; } = new();
}

/// <summary>Well-known report card format codes.</summary>
public static class ReportCardFormatCodes
{
    public const string KindergartenInitial  = "KG-INITIAL";   // Term 1 Kindergarten
    public const string KindergartenReport   = "KG-REPORT";    // Term 2/3 Kindergarten
    public const string ElementaryProgress   = "EL-PROGRESS";  // Term 1 Elementary
    public const string ElementaryReport     = "EL-REPORT";    // Term 2/3 Elementary

    public static readonly string[] All =
    [
        KindergartenInitial, KindergartenReport,
        ElementaryProgress,  ElementaryReport
    ];

    public static string DisplayName(string code) => code switch
    {
        KindergartenInitial => "Kindergarten Initial Observations",
        KindergartenReport  => "Kindergarten Communication of Learning",
        ElementaryProgress  => "Elementary Progress Report",
        ElementaryReport    => "Elementary Provincial Report Card",
        _                   => code
    };
}

public class TermInstance
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    /// <summary>Which report card format is produced at the end of this term.</summary>
    public string? ReportCardFormatCode { get; set; }

    public int SchoolYearId { get; set; }
    public SchoolYear? SchoolYear { get; set; }

    public List<ClassGroupInstance> ClassGroupInstances { get; set; } = new();
    public List<Enrollment> Enrollments { get; set; } = new();
    public List<TermGradeGradingRule> GradingRules { get; set; } = new();
    public List<Assessment> Assessments { get; set; } = new();
    public List<ReportTemplateFieldMap> ReportTemplateFieldMaps { get; set; } = new();
}

/// <summary>A ClassGroupType activated within a specific term.</summary>
public class ClassGroupInstance
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }

    public int TermInstanceId { get; set; }
    public TermInstance? TermInstance { get; set; }

    public int ClassGroupTypeId { get; set; }
    public ClassGroupType? ClassGroupType { get; set; }

    public List<Enrollment> Enrollments { get; set; } = new();
    public List<TeacherAssignment> TeacherAssignments { get; set; } = new();
}

/// <summary>Default grading scale for a term + class group + grade combination.</summary>
public class TermGradeGradingRule
{
    public int Id { get; set; }

    public int TermInstanceId { get; set; }
    public TermInstance? TermInstance { get; set; }

    public int ClassGroupTypeId { get; set; }
    public ClassGroupType? ClassGroupType { get; set; }

    public int GradeId { get; set; }
    public Grade? Grade { get; set; }

    public int GradingScaleId { get; set; }
    public GradingScale? GradingScale { get; set; }
}

/// <summary>
/// Defines which report card format family applies to a class group type within a school year.
/// e.g. Kindergarten → KG, Primary → EL
/// This drives which report card document is generated for students in that class group.
/// </summary>
public class ClassGroupReportFormat
{
    public int Id { get; set; }

    /// <summary>Format family prefix: "KG" or "EL"</summary>
    public required string FormatFamily { get; set; }

    public int SchoolYearId { get; set; }
    public SchoolYear? SchoolYear { get; set; }

    public int ClassGroupTypeId { get; set; }
    public ClassGroupType? ClassGroupType { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// K) COMMENT TEMPLATES
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// Pronoun set controlling placeholder substitution in comment templates.
/// </summary>
 public enum PronounSet
{
    HeHim,     // he / him / his / his
    SheHer,    // she / her / her / hers
    TheyThem,  // they / them / their / theirs
}

/// <summary>
/// A reusable comment template, optionally scoped to a subject and/or grade.
/// Placeholders are substituted at use-time from the student's profile.
///
/// Supported placeholders:
///   ~name / ~Name      → student first name (lower / Title)
///   ~h/s/e / ~H/s/e    → he | she | they (lower / Title)
///   ~h/s/r / ~H/s/r    → his | her | their (lower / Title)
///   ~him/her           → him | her | them
/// </summary>
public class CommentTemplate
{
    public int Id { get; set; }
    public required string TemplateText { get; set; }

    /// <summary>Subject area the template belongs to, e.g. "English". Null = all subjects.</summary>
    public string? Subject { get; set; }

    /// <summary>Grade label, e.g. "4" or "JK". Null = all grades.</summary>
    public string? GradeLabel { get; set; }

    /// <summary>Category tag, e.g. "Strength", "Next Steps", "Weaknesses".</summary>
    public string? Category { get; set; }

    /// <summary>Original code or name from the source system (for deduplication on re-import).</summary>
    public string? SourceCode { get; set; }

    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

// ═══════════════════════════════════════════════════════════════════
// D) PEOPLE
// ═══════════════════════════════════════════════════════════════════

public class Teacher
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public DateOnly? HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public string? Notes { get; set; }
    public string? AvatarConfigJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? InactivatedOn { get; set; }
    public string? InactivatedReason { get; set; }

    public List<TeacherAssignment> Assignments { get; set; } = new();
    public List<AppUser> AppUsers { get; set; } = new();
}

public class Student
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public PronounSet Pronouns { get; set; } = PronounSet.TheyThem;
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly? EnrollmentDate { get; set; }
    public DateOnly? CompletionDate { get; set; }
    public string? OenNumber { get; set; }
    public string? ParentGuardianContact { get; set; }
    public string? Notes { get; set; }
    public string? AvatarConfigJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? InactivatedOn { get; set; }
    public string? InactivatedReason { get; set; }

    public List<Enrollment> Enrollments { get; set; } = new();
    public List<AttendanceEvent> AttendanceEvents { get; set; } = new();
}

public class AppUser
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }

    public int? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}

/// <summary>Links a teacher to a ClassGroupInstance, optionally scoped to a specific grade.</summary>
public class TeacherAssignment
{
    public int Id { get; set; }
    public string? Role { get; set; }

    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public int ClassGroupInstanceId { get; set; }
    public ClassGroupInstance? ClassGroupInstance { get; set; }

    public int? GradeId { get; set; }
    public Grade? Grade { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// D2) CALENDAR + ATTENDANCE
// ═══════════════════════════════════════════════════════════════════

public enum CalendarExceptionType { StatHoliday, PdPaDay, SnowDay }

/// <summary>A non-school day within a school year (stat holidays, PD/PA days, snow days).</summary>
public class SchoolCalendarException
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public CalendarExceptionType ExceptionType { get; set; }
    public required string Label { get; set; }

    public int SchoolYearId { get; set; }
    public SchoolYear? SchoolYear { get; set; }
}

public enum AttendanceType { Absent, Late }

/// <summary>Sparse attendance record — only absences and lates are stored. Presence is inferred.</summary>
public class AttendanceEvent
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceType Type { get; set; }
    public string? Note { get; set; }

    public int StudentId { get; set; }
    public Student? Student { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// E) ENROLLMENT + LEARNING ITEMS + ASSESSMENTS
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// One enrollment per student per term per grade.
/// StudentLearningItems are seeded at enrollment time from the enabled offerings.
/// </summary>
public class Enrollment
{
    public int Id { get; set; }
    public DateTimeOffset EnrolledOn { get; set; } = DateTimeOffset.UtcNow;

    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public int TermInstanceId { get; set; }
    public TermInstance? TermInstance { get; set; }

    public int ClassGroupInstanceId { get; set; }
    public ClassGroupInstance? ClassGroupInstance { get; set; }

    public int GradeId { get; set; }
    public Grade? Grade { get; set; }

    public List<StudentLearningItem> LearningItems { get; set; } = new();
}

/// <summary>
/// One row per gradeable item. Seeded at enrollment time.
/// GradingScaleId is FROZEN at creation.
/// </summary>
public class StudentLearningItem
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public LearningItemType ItemType { get; set; }

    public int EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }

    public int? YearClassOfferingId { get; set; }
    public YearClassOffering? YearClassOffering { get; set; }

    public int? YearSubjectOfferingId { get; set; }
    public YearSubjectOffering? YearSubjectOffering { get; set; }

    public int GradingScaleId { get; set; }
    public GradingScale? GradingScale { get; set; }

    public List<Assessment> Assessments { get; set; } = new();
}

public enum LearningItemType { Class, Subject }

/// <summary>Grade entry for one StudentLearningItem in one term.</summary>
public class Assessment
{
    public int Id { get; set; }
    public string? ValueText { get; set; }
    public decimal? ValueNumber { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public int StudentLearningItemId { get; set; }
    public StudentLearningItem? StudentLearningItem { get; set; }

    public int TermInstanceId { get; set; }
    public TermInstance? TermInstance { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// F) CURRICULUM (IMPORT + YEAR OFFERINGS)
// ═══════════════════════════════════════════════════════════════════

public class CurriculumSchema
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? RawArtifactData { get; set; }

    public List<CurriculumGradeTemplate> GradeTemplates { get; set; } = new();
    public List<YearCurriculum> YearCurriculums { get; set; } = new();
}

public class CurriculumGradeTemplate
{
    public int Id { get; set; }
    public int SortOrder { get; set; }

    public int CurriculumSchemaId { get; set; }
    public CurriculumSchema? CurriculumSchema { get; set; }

    public int GradeId { get; set; }
    public Grade? Grade { get; set; }

    public List<CurriculumClassTemplate> ClassTemplates { get; set; } = new();
}

/// <summary>A subject area within a grade: "Mathematics", "Physical Education"</summary>
public class CurriculumClassTemplate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public int SortOrder { get; set; }

    /// <summary>
    /// If true, teachers grade each Strand individually.
    /// If false, teachers assign a single grade for the whole Subject.
    /// </summary>
    public bool GradedAtStrandLevel { get; set; } = false;

    public int CurriculumGradeTemplateId { get; set; }
    public CurriculumGradeTemplate? CurriculumGradeTemplate { get; set; }

    public List<CurriculumSubjectTemplate> SubjectTemplates { get; set; } = new();
    public List<YearClassOffering> YearClassOfferings { get; set; } = new();
}

/// <summary>A strand/sub-topic within a class: "Algebra", "Soccer"</summary>
public class CurriculumSubjectTemplate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public int SortOrder { get; set; }

    public int CurriculumClassTemplateId { get; set; }
    public CurriculumClassTemplate? CurriculumClassTemplate { get; set; }

    public List<CurriculumSubStrand> SubStrands { get; set; } = new();
    public List<YearSubjectOffering> YearSubjectOfferings { get; set; } = new();
}

/// <summary>
/// Read-only reference guidance text for a strand. Describes what the strand covers
/// to help teachers understand the expectations when applying a grade.
/// Not graded — displayed as a reference panel in the grade entry UI.
/// </summary>
public class CurriculumSubStrand
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }   // full guidance text visible to teacher
    public int SortOrder { get; set; }

    public int CurriculumSubjectTemplateId { get; set; }
    public CurriculumSubjectTemplate? CurriculumSubjectTemplate { get; set; }
}

/// <summary>Ties a curriculum schema to a school year (unique per year).</summary>
public class YearCurriculum
{
    public int Id { get; set; }
    public DateTimeOffset AppliedAt { get; set; } = DateTimeOffset.UtcNow;

    public int SchoolYearId { get; set; }
    public SchoolYear? SchoolYear { get; set; }

    public int CurriculumSchemaId { get; set; }
    public CurriculumSchema? CurriculumSchema { get; set; }

    public List<YearClassOffering> ClassOfferings { get; set; } = new();
}

/// <summary>Whether a class (subject area) is enabled for a grade in this year.</summary>
public class YearClassOffering
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? ReportDestinationKey { get; set; }
    public int? GradingScaleId { get; set; }

    public int YearCurriculumId { get; set; }
    public YearCurriculum? YearCurriculum { get; set; }

    public int GradeId { get; set; }
    public Grade? Grade { get; set; }

    public int CurriculumClassTemplateId { get; set; }
    public CurriculumClassTemplate? CurriculumClassTemplate { get; set; }

    public GradingScale? GradingScale { get; set; }

    public List<YearSubjectOffering> SubjectOfferings { get; set; } = new();
    public List<StudentLearningItem> StudentLearningItems { get; set; } = new();
}

/// <summary>Whether a subject strand is enabled within a class offering for this year.</summary>
public class YearSubjectOffering
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? ReportDestinationKey { get; set; }
    public int? GradingScaleId { get; set; }

    public int YearClassOfferingId { get; set; }
    public YearClassOffering? YearClassOffering { get; set; }

    public int CurriculumSubjectTemplateId { get; set; }
    public CurriculumSubjectTemplate? CurriculumSubjectTemplate { get; set; }

    public GradingScale? GradingScale { get; set; }

    public List<StudentLearningItem> StudentLearningItems { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// I) AI PROMPT CONFIGURATION
// ═══════════════════════════════════════════════════════════════════

/// <summary>Singleton school-wide AI behaviour config.</summary>
public class SchoolAiConfig
{
    public int Id { get; set; }
    public string SchoolName { get; set; } = "KinderKollege";

    // Tone & philosophy
    public string? GradingPhilosophy { get; set; }   // e.g. "encouraging, growth-mindset"
    public string? ToneGuidance { get; set; }         // e.g. "warm, age-appropriate, avoid negative language"
    public string? TerminologyNotes { get; set; }     // e.g. "use 'learning skills' not 'behaviour'"

    // Check-specific school defaults
    public string? SpellingGuidance { get; set; }
    public string? GrammarGuidance { get; set; }
    public string? RubricGuidance { get; set; }
    public string? AiDetectionGuidance { get; set; }

    // Free-form additional instructions
    public string? AdditionalInstructions { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>Per-teacher AI behaviour preferences, layered on top of SchoolAiConfig.</summary>
public class TeacherAiConfig
{
    public int Id { get; set; }

    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    // Personal tone overrides
    public string? PreferredTone { get; set; }        // e.g. "very encouraging, focus on positives first"
    public string? FocusAreas { get; set; }           // e.g. "pay special attention to sentence structure"

    // Free-form personal instructions
    public string? AdditionalInstructions { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

// ═══════════════════════════════════════════════════════════════════
// H) HOMEWORK ANALYSIS (ephemeral AI results, no student PII)
// ═══════════════════════════════════════════════════════════════════

/// <summary>One stored page image belonging to a saved HomeworkAnalysis.</summary>
public class HomeworkAnalysisImage
{
    public int Id { get; set; }
    public int SortOrder { get; set; }

    /// <summary>Full base64 data URL, e.g. data:image/jpeg;base64,...</summary>
    public required string ImageData { get; set; }

    public int HomeworkAnalysisId { get; set; }
    public HomeworkAnalysis? HomeworkAnalysis { get; set; }
}

/// <summary>
/// Saved AI analysis result. Includes page images so other teachers can review.
/// Teacher optionally saves after reviewing.
/// </summary>
public class HomeworkAnalysis
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset AnalyzedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? GradeName { get; set; }
    public string? ClassGroupName { get; set; }

    // Which checks were run
    public bool CheckedSpelling { get; set; }
    public bool CheckedGrammar { get; set; }
    public bool CheckedRubric { get; set; }
    public bool CheckedAiGenerated { get; set; }

    // Results
    public int? SpellingScore { get; set; }
    public string? SpellingSummary { get; set; }
    public int? GrammarScore { get; set; }
    public string? GrammarSummary { get; set; }
    public int? RubricScore { get; set; }
    public string? RubricSummary { get; set; }
    public string? AiLikelihood { get; set; }
    public string? AiDetectionSummary { get; set; }
    public string? OverallSummary { get; set; }

    // Optional teacher note
    public string? TeacherNote { get; set; }

    public List<HomeworkAnalysisImage> Images { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// J) PEER REVIEW
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// Records that a teacher has peer-reviewed another teacher's grade
/// entry for a specific enrollment in a specific term.
/// </summary>
public class EnrollmentPeerReview
{
    public int Id { get; set; }
    public DateTimeOffset ReviewedAt { get; set; } = DateTimeOffset.UtcNow;

    public int EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }

    public int TermInstanceId { get; set; }
    public TermInstance? TermInstance { get; set; }

    public int ReviewerTeacherId { get; set; }
    public Teacher? ReviewerTeacher { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// G) REPORT TEMPLATE FIELD MAPPING
// ═══════════════════════════════════════════════════════════════════

/// <summary>
/// Maps a stable ReportDestinationKey to an actual PDF form field for a given term.
/// Pipeline: Assessment → StudentLearningItem → Offering → DestinationKey → PDF field
/// </summary>
public class ReportTemplateFieldMap
{
    public int Id { get; set; }
    public required string ReportDestinationKey { get; set; }
    public required string PdfFieldName { get; set; }

    public int TermInstanceId { get; set; }
    public TermInstance? TermInstance { get; set; }
}
