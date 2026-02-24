namespace ReportCards.Web.Data;

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

    public List<TermInstance> TermInstances { get; set; } = new();
    public List<YearCurriculum> YearCurriculums { get; set; } = new();
}

public class TermInstance
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

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

// ═══════════════════════════════════════════════════════════════════
// D) PEOPLE
// ═══════════════════════════════════════════════════════════════════

public class Teacher
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
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
    public DateOnly? DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? InactivatedOn { get; set; }
    public string? InactivatedReason { get; set; }

    public List<Enrollment> Enrollments { get; set; } = new();
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
    public string? Version { get; set; }
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? RawArtifactData { get; set; }

    public List<CurriculumGradeTemplate> GradeTemplates { get; set; } = new();
    public List<YearCurriculum> YearCurriculums { get; set; } = new();
}

public class CurriculumGradeTemplate
{
    public int Id { get; set; }

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

    public List<YearSubjectOffering> YearSubjectOfferings { get; set; } = new();
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

/// <summary>
/// Saved AI analysis result. No student name, no photo stored.
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
