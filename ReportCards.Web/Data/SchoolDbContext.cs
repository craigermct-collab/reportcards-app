using Microsoft.EntityFrameworkCore;

namespace ReportCards.Web.Data;

public class SchoolDbContext : DbContext
{
    public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options) { }

    // Reference data
    public DbSet<ClassGroupType> ClassGroupTypes => Set<ClassGroupType>();
    public DbSet<Grade> Grades => Set<Grade>();

    // Grading
    public DbSet<GradingScale> GradingScales => Set<GradingScale>();
    public DbSet<GradingScaleOption> GradingScaleOptions => Set<GradingScaleOption>();

    // School year / terms
    public DbSet<SchoolYear> SchoolYears => Set<SchoolYear>();
    public DbSet<TermInstance> TermInstances => Set<TermInstance>();
    public DbSet<ClassGroupInstance> ClassGroupInstances => Set<ClassGroupInstance>();
    public DbSet<TermGradeGradingRule> TermGradeGradingRules => Set<TermGradeGradingRule>();

    // People
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<TeacherAssignment> TeacherAssignments => Set<TeacherAssignment>();

    // Enrollment + grading
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<StudentLearningItem> StudentLearningItems => Set<StudentLearningItem>();
    public DbSet<Assessment> Assessments => Set<Assessment>();

    // Curriculum
    public DbSet<CurriculumSchema> CurriculumSchemas => Set<CurriculumSchema>();
    public DbSet<CurriculumGradeTemplate> CurriculumGradeTemplates => Set<CurriculumGradeTemplate>();
    public DbSet<CurriculumClassTemplate> CurriculumClassTemplates => Set<CurriculumClassTemplate>();
    public DbSet<CurriculumSubjectTemplate> CurriculumSubjectTemplates => Set<CurriculumSubjectTemplate>();
    public DbSet<YearCurriculum> YearCurriculums => Set<YearCurriculum>();
    public DbSet<YearClassOffering> YearClassOfferings => Set<YearClassOffering>();
    public DbSet<YearSubjectOffering> YearSubjectOfferings => Set<YearSubjectOffering>();

    // Report mapping
    public DbSet<ReportTemplateFieldMap> ReportTemplateFieldMaps => Set<ReportTemplateFieldMap>();
    public DbSet<HomeworkAnalysis> HomeworkAnalyses => Set<HomeworkAnalysis>();
    public DbSet<HomeworkAnalysisImage> HomeworkAnalysisImages => Set<HomeworkAnalysisImage>();

    // AI prompt config
    public DbSet<SchoolAiConfig> SchoolAiConfigs => Set<SchoolAiConfig>();
    public DbSet<TeacherAiConfig> TeacherAiConfigs => Set<TeacherAiConfig>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        base.OnModelCreating(m);

        // ═══════════════════════════════════════════════════════════
        // UNIQUE INDEXES
        // ═══════════════════════════════════════════════════════════

        m.Entity<Grade>()
            .HasIndex(g => new { g.ClassGroupTypeId, g.Name }).IsUnique();

        m.Entity<GradingScaleOption>()
            .HasIndex(o => new { o.GradingScaleId, o.Label }).IsUnique();

        m.Entity<Teacher>()
            .HasIndex(t => t.Email).IsUnique();

        m.Entity<AppUser>()
            .HasIndex(u => u.Email).IsUnique();

        m.Entity<YearCurriculum>()
            .HasIndex(y => y.SchoolYearId).IsUnique();

        m.Entity<TermGradeGradingRule>()
            .HasIndex(r => new { r.TermInstanceId, r.ClassGroupTypeId, r.GradeId }).IsUnique();

        m.Entity<Assessment>()
            .HasIndex(a => new { a.StudentLearningItemId, a.TermInstanceId }).IsUnique();

        // ═══════════════════════════════════════════════════════════
        // DECIMAL PRECISION
        // ═══════════════════════════════════════════════════════════

        m.Entity<GradingScale>().Property(g => g.MinValue).HasPrecision(8, 2);
        m.Entity<GradingScale>().Property(g => g.MaxValue).HasPrecision(8, 2);
        m.Entity<GradingScale>().Property(g => g.Step).HasPrecision(8, 2);
        m.Entity<Assessment>().Property(a => a.ValueNumber).HasPrecision(8, 2);

        // ═══════════════════════════════════════════════════════════
        // CASCADE POLICY — COMPLETE AUDIT
        //
        // SQL Server rule: if two cascade paths can reach the same
        // table, ALL paths get rejected. Strategy:
        //   CASCADE  → simple leaf chains with no ambiguity
        //   NoAction → anything touching a shared "hub" table
        //              (Grade, ClassGroupType, GradingScale,
        //               TermInstance, Enrollment, YearClassOffering)
        //
        // Deletion is handled in application code for NoAction FKs.
        // ═══════════════════════════════════════════════════════════

        // ── ClassGroupType → Grade (CASCADE: simple spine) ─────────
        // EF default is CASCADE here, no override needed.

        // ── GradingScale → GradingScaleOption (CASCADE: simple) ────
        // EF default is CASCADE here, no override needed.

        // ── SchoolYear → TermInstance (CASCADE: simple spine) ──────
        // EF default is CASCADE here, no override needed.

        // ── TermInstance → ClassGroupInstance (CASCADE: ok, single path) ─
        // EF default is CASCADE here, no override needed.

        // ── CurriculumSchema → CurriculumGradeTemplate (CASCADE) ───
        // EF default is CASCADE here, no override needed.

        // ── CurriculumGradeTemplate → CurriculumClassTemplate (CASCADE) ─
        // EF default is CASCADE here, no override needed.

        // ── CurriculumClassTemplate → CurriculumSubjectTemplate (CASCADE) ─
        // EF default is CASCADE here, no override needed.

        // ── SchoolYear → YearCurriculum (CASCADE: single path) ─────
        // EF default is CASCADE here, no override needed.

        // ── YearCurriculum → YearClassOffering (CASCADE: single) ───
        // EF default is CASCADE here, no override needed.

        // ──────────────────────────────────────────────────────────
        // NoAction on everything below — these all fan into "hub"
        // tables that are reachable via multiple cascade paths.
        // ──────────────────────────────────────────────────────────

        // CurriculumGradeTemplate.GradeId
        // (Grade reachable via ClassGroupType AND via CurriculumSchema)
        m.Entity<CurriculumGradeTemplate>()
            .HasOne(t => t.Grade)
            .WithMany(g => g.CurriculumGradeTemplates)
            .HasForeignKey(t => t.GradeId)
            .OnDelete(DeleteBehavior.NoAction);

        // TermInstance → ReportTemplateFieldMap
        // (TermInstance already cascades to ClassGroupInstance & Enrollment)
        m.Entity<ReportTemplateFieldMap>()
            .HasOne(r => r.TermInstance)
            .WithMany(t => t.ReportTemplateFieldMaps)
            .HasForeignKey(r => r.TermInstanceId)
            .OnDelete(DeleteBehavior.NoAction);

        // TermGradeGradingRule — three FKs all into hub tables
        m.Entity<TermGradeGradingRule>()
            .HasOne(r => r.TermInstance)
            .WithMany(t => t.GradingRules)
            .HasForeignKey(r => r.TermInstanceId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<TermGradeGradingRule>()
            .HasOne(r => r.ClassGroupType)
            .WithMany(c => c.GradingRules)
            .HasForeignKey(r => r.ClassGroupTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<TermGradeGradingRule>()
            .HasOne(r => r.Grade)
            .WithMany(g => g.GradingRules)
            .HasForeignKey(r => r.GradeId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<TermGradeGradingRule>()
            .HasOne(r => r.GradingScale)
            .WithMany(g => g.GradingRules)
            .HasForeignKey(r => r.GradingScaleId)
            .OnDelete(DeleteBehavior.NoAction);

        // TeacherAssignment — all FKs into hub tables
        m.Entity<TeacherAssignment>()
            .HasOne(a => a.Teacher)
            .WithMany(t => t.Assignments)
            .HasForeignKey(a => a.TeacherId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<TeacherAssignment>()
            .HasOne(a => a.ClassGroupInstance)
            .WithMany(c => c.TeacherAssignments)
            .HasForeignKey(a => a.ClassGroupInstanceId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<TeacherAssignment>()
            .HasOne(a => a.Grade)
            .WithMany(g => g.TeacherAssignments)
            .HasForeignKey(a => a.GradeId)
            .OnDelete(DeleteBehavior.NoAction);

        // Enrollment — all FKs into hub tables
        m.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<Enrollment>()
            .HasOne(e => e.TermInstance)
            .WithMany(t => t.Enrollments)
            .HasForeignKey(e => e.TermInstanceId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<Enrollment>()
            .HasOne(e => e.ClassGroupInstance)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.ClassGroupInstanceId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<Enrollment>()
            .HasOne(e => e.Grade)
            .WithMany(g => g.Enrollments)
            .HasForeignKey(e => e.GradeId)
            .OnDelete(DeleteBehavior.NoAction);

        // YearClassOffering — GradeId and CurriculumClassTemplateId into hubs
        m.Entity<YearClassOffering>()
            .HasOne(o => o.Grade)
            .WithMany(g => g.YearClassOfferings)
            .HasForeignKey(o => o.GradeId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<YearClassOffering>()
            .HasOne(o => o.CurriculumClassTemplate)
            .WithMany(c => c.YearClassOfferings)
            .HasForeignKey(o => o.CurriculumClassTemplateId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<YearClassOffering>()
            .HasOne(o => o.GradingScale)
            .WithMany()
            .HasForeignKey(o => o.GradingScaleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        // YearSubjectOffering — all FKs into hubs
        m.Entity<YearSubjectOffering>()
            .HasOne(o => o.YearClassOffering)
            .WithMany(c => c.SubjectOfferings)
            .HasForeignKey(o => o.YearClassOfferingId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<YearSubjectOffering>()
            .HasOne(o => o.CurriculumSubjectTemplate)
            .WithMany(c => c.YearSubjectOfferings)
            .HasForeignKey(o => o.CurriculumSubjectTemplateId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<YearSubjectOffering>()
            .HasOne(o => o.GradingScale)
            .WithMany()
            .HasForeignKey(o => o.GradingScaleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        // StudentLearningItem — EnrollmentId is the main owner (CASCADE ok)
        // but GradingScaleId, YearClassOfferingId, YearSubjectOfferingId are hubs
        m.Entity<StudentLearningItem>()
            .HasOne(s => s.GradingScale)
            .WithMany(g => g.StudentLearningItems)
            .HasForeignKey(s => s.GradingScaleId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<StudentLearningItem>()
            .HasOne(s => s.YearClassOffering)
            .WithMany(o => o.StudentLearningItems)
            .HasForeignKey(s => s.YearClassOfferingId)
            .OnDelete(DeleteBehavior.NoAction);

        m.Entity<StudentLearningItem>()
            .HasOne(s => s.YearSubjectOffering)
            .WithMany(o => o.StudentLearningItems)
            .HasForeignKey(s => s.YearSubjectOfferingId)
            .OnDelete(DeleteBehavior.NoAction);

        // Assessment — StudentLearningItemId is main owner (CASCADE ok)
        // but TermInstanceId is a hub
        m.Entity<Assessment>()
            .HasOne(a => a.TermInstance)
            .WithMany(t => t.Assessments)
            .HasForeignKey(a => a.TermInstanceId)
            .OnDelete(DeleteBehavior.NoAction);

        // TeacherAiConfig — one per teacher
        m.Entity<TeacherAiConfig>()
            .HasIndex(t => t.TeacherId).IsUnique();

        m.Entity<TeacherAiConfig>()
            .HasOne(t => t.Teacher)
            .WithMany()
            .HasForeignKey(t => t.TeacherId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
