using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ReportCards.Web.Data;

namespace ReportCards.Web.Services;

/// <summary>
/// A single message in the chat history.
/// </summary>
public record ChatMessage(string Role, string Content, AssistantAction? Action = null);

/// <summary>
/// A structured action Claude wants to perform — returned alongside the prose reply.
/// </summary>
public class AssistantAction
{
    public string Type { get; set; } = "";          // "mark_attendance" | "query" | "none"
    public string? Summary { get; set; }            // Human-readable summary e.g. "Mark David absent today"
    public List<AttendanceChange>? AttendanceChanges { get; set; }
}

public class AttendanceChange
{
    public int     StudentId   { get; set; }
    public string  StudentName { get; set; } = "";
    public string  Date        { get; set; } = "";  // ISO yyyy-MM-dd
    public string  Status      { get; set; } = "";  // "absent" | "late" | "present"
}

public class AssistantService
{
    private readonly IConfiguration      _config;
    private readonly IDbContextFactory<SchoolDbContext> _dbFactory;
    private readonly IHttpClientFactory  _http;
    private readonly AssistantContextService _context;

    public AssistantService(IConfiguration config,
                            IDbContextFactory<SchoolDbContext> dbFactory,
                            IHttpClientFactory http,
                            AssistantContextService context)
    {
        _config   = config;
        _dbFactory = dbFactory;
        _http     = http;
        _context  = context;
    }

    // ── Public entry point ────────────────────────────────────────────────────

    /// <summary>
    /// Send a user message plus full history to Claude. Returns the assistant reply
    /// (prose) and optionally a structured action to confirm before executing.
    /// </summary>
    public async Task<ChatMessage> SendAsync(
        List<ChatMessage> history,
        string userMessage,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var context = await BuildSchoolContextAsync(db, ct);
        var systemPrompt = BuildSystemPrompt(context);

        var apiKey = _config["Anthropic:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return new ChatMessage("assistant",
                "⚠️ No Anthropic API key configured. Add `Anthropic:ApiKey` to your app settings.");

        // Build messages array for the API
        var messages = new List<object>();
        foreach (var m in history)
            messages.Add(new { role = m.Role, content = m.Content });
        messages.Add(new { role = "user", content = userMessage });

        var requestBody = new
        {
            model      = "claude-sonnet-4-20250514",
            max_tokens = 1024,
            system     = systemPrompt,
            messages
        };

        var client = _http.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var json     = JsonSerializer.Serialize(requestBody);
        var content  = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content, ct);
        var body     = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return new ChatMessage("assistant", $"⚠️ API error {response.StatusCode}: {body}");

        using var doc   = JsonDocument.Parse(body);
        var rawText = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "";

        // Try to extract a JSON action block from the response
        var (prose, action) = ParseResponse(rawText);

        return new ChatMessage("assistant", prose, action);
    }

    /// <summary>
    /// Actually execute a confirmed attendance action against the DB.
    /// </summary>
    public async Task ExecuteAttendanceAsync(AssistantAction action, CancellationToken ct = default)
    {
        if (action.AttendanceChanges == null) return;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        foreach (var change in action.AttendanceChanges)
        {
            if (!DateOnly.TryParse(change.Date, out var date)) continue;

            // Remove any existing record for this student+date
            var existing = await db.AttendanceEvents
                .Where(a => a.StudentId == change.StudentId && a.Date == date)
                .ToListAsync(ct);
            db.AttendanceEvents.RemoveRange(existing);

            if (change.Status == "absent")
            {
                db.AttendanceEvents.Add(new AttendanceEvent
                {
                    StudentId = change.StudentId,
                    Date      = date,
                    Type      = AttendanceType.Absent
                });
            }
            else if (change.Status == "late")
            {
                db.AttendanceEvents.Add(new AttendanceEvent
                {
                    StudentId = change.StudentId,
                    Date      = date,
                    Type      = AttendanceType.Late
                });
            }
            // "present" = just delete existing record (sparse storage)
        }

        await db.SaveChangesAsync(ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private record SchoolContext(
        string SchoolName,
        string TodayStr,
        string? ActiveYearName,
        string? CurrentTermName,
        List<StudentInfo> TodayStudents,
        List<AttendanceInfo> TodayAttendance,
        SchoolAiConfig? AiConfig,
        TeacherAiConfig? TeacherConfig
    );

    private record StudentInfo(int Id, string FullName, string? ClassName, string? Grade);
    private record AttendanceInfo(int StudentId, string FullName, string Status);

    private async Task<SchoolContext> BuildSchoolContextAsync(SchoolDbContext db, CancellationToken ct)
    {
        var today    = DateOnly.FromDateTime(DateTime.Today);
        var todayStr = DateTime.Today.ToString("dddd, MMMM d, yyyy");

        // School name from config
        var schoolName = (await db.SchoolConfigs.FirstOrDefaultAsync(c => c.Key == "SchoolName", ct))?.Value
                         ?? "KinderKollege";

        // Active school year + current term
        var activeYear = await db.SchoolYears
            .Include(y => y.TermInstances)
            .FirstOrDefaultAsync(y => y.IsActive, ct);

        var currentTerm = activeYear?.TermInstances
            .FirstOrDefault(t => today >= t.StartDate && today <= t.EndDate);

        // Students enrolled in current term
        List<StudentInfo> todayStudents = new();
        if (currentTerm != null)
        {
            todayStudents = await db.Enrollments
                .Where(e => e.TermInstanceId == currentTerm.Id)
                .Include(e => e.Student)
                .Include(e => e.Grade)
                .Include(e => e.ClassGroupInstance)
                .OrderBy(e => e.Student!.LastName)
                .Select(e => new StudentInfo(
                    e.StudentId,
                    e.Student!.FirstName + " " + e.Student.LastName,
                    e.ClassGroupInstance!.DisplayName,
                    e.Grade!.Name))
                .ToListAsync(ct);
        }

        // Today's attendance
        var studentIds = todayStudents.Select(s => s.Id).ToList();
        var attendance = await db.AttendanceEvents
            .Where(a => studentIds.Contains(a.StudentId) && a.Date == today)
            .ToListAsync(ct);

        var todayAttendance = attendance.Select(a => new AttendanceInfo(
            a.StudentId,
            todayStudents.FirstOrDefault(s => s.Id == a.StudentId)?.FullName ?? "Unknown",
            a.Type == AttendanceType.Absent ? "absent" : "late"
        )).ToList();

        var aiConfig      = await db.SchoolAiConfigs.FirstOrDefaultAsync(ct);
        var teacherConfig  = (TeacherAiConfig?)null; // future: resolve from logged-in teacher

        return new SchoolContext(
            schoolName,
            todayStr,
            activeYear?.Name,
            currentTerm?.Name,
            todayStudents,
            todayAttendance,
            aiConfig,
            teacherConfig
        );
    }

    // ── Route to the right prompt based on mode ───────────────────────────────

    private string BuildSystemPrompt(SchoolContext ctx)
        => _context.Current.Mode == AssistantMode.CommentWriter
            ? BuildCommentWriterPrompt(ctx)
            : BuildGeneralPrompt(ctx);

    // ── MODE 1: General casual bot ────────────────────────────────────────────

    private string BuildGeneralPrompt(SchoolContext ctx)
    {
        var sb = new StringBuilder();
        var ai = ctx.AiConfig;
        var tc = ctx.TeacherConfig;

        // Personality: teacher override > school override > built-in default
        var personality = !string.IsNullOrWhiteSpace(tc?.GeneralAssistantPrompt)
            ? tc!.GeneralAssistantPrompt
            : !string.IsNullOrWhiteSpace(ai?.GeneralAssistantPrompt)
                ? ai!.GeneralAssistantPrompt
                : $"You are a friendly, warm, and occasionally funny school assistant for {ctx.SchoolName}. " +
                  "You enjoy casual banter with teachers — you can joke around, be playful, and keep things light. " +
                  "You're also genuinely helpful when they need real answers. Keep replies concise.";

        sb.AppendLine(personality);
        sb.AppendLine($"Today is {ctx.TodayStr}. School year: {ctx.ActiveYearName ?? "none"}. Term: {ctx.CurrentTermName ?? "none"}.");
        sb.AppendLine();
        AppendSharedGuidance(sb, ai, tc);

        sb.AppendLine("== STUDENTS ENROLLED TODAY ==");
        if (ctx.TodayStudents.Count == 0)
            sb.AppendLine("  (No students enrolled in the current term)");
        else
            foreach (var s in ctx.TodayStudents)
                sb.AppendLine($"  ID:{s.Id}  {s.FullName}  [{s.ClassName} / {s.Grade}]");

        sb.AppendLine();
        sb.AppendLine("== TODAY'S ATTENDANCE SO FAR ==");
        if (ctx.TodayAttendance.Count == 0)
            sb.AppendLine("  (none recorded yet — everyone is assumed present)");
        else
            foreach (var a in ctx.TodayAttendance)
                sb.AppendLine($"  {a.FullName}: {a.Status}");

        sb.AppendLine();
        sb.AppendLine("""
            == RESPONDING WITH ACTIONS ==
            When the teacher wants to UPDATE attendance records, end your response
            with a JSON block wrapped in <action>...</action> tags:

            <action>
            {
              "type": "mark_attendance",
              "summary": "Mark David Hassan absent and Mary Smith late for today",
              "attendanceChanges": [
                { "studentId": 3, "studentName": "David Hassan", "date": "2026-03-04", "status": "absent" },
                { "studentId": 5, "studentName": "Mary Smith",   "date": "2026-03-04", "status": "late" }
              ]
            }
            </action>

            Rules:
            - Match student names from the enrolled list. Use the exact ID.
            - If a name is ambiguous, ask for clarification.
            - Use today's date unless the teacher specifies otherwise.
            - For non-update queries, reply in prose only — no <action> block.
            - Always confirm what you're about to do BEFORE the action block.
            """);

        return sb.ToString();
    }

    // ── MODE 2: Focused comment-writing assistant ─────────────────────────────

    private string BuildCommentWriterPrompt(SchoolContext ctx)
    {
        var sb = new StringBuilder();
        var ai  = ctx.AiConfig;
        var tc  = ctx.TeacherConfig;
        var ctx2 = _context.Current;

        // Personality: teacher override > school override > built-in default
        var personality = !string.IsNullOrWhiteSpace(tc?.CommentWriterPrompt)
            ? tc!.CommentWriterPrompt
            : !string.IsNullOrWhiteSpace(ai?.CommentWriterPrompt)
                ? ai!.CommentWriterPrompt
                : $"You are a professional Ontario elementary school report card comment writer for {ctx.SchoolName}. " +
                  "Your job is to help teachers write or refine student report card comments. " +
                  "Comments must be clear, specific, professional, and strengths-based. " +
                  "Avoid vague generalities. Use the student's name and correct pronouns. " +
                  "Do not use the word 'overall'. Keep comments under 100 words unless asked otherwise. " +
                  "Respond ONLY with the comment text — no preamble, no explanation.";

        sb.AppendLine(personality);
        sb.AppendLine();

        // Grade-specific context
        if (!string.IsNullOrWhiteSpace(ctx2.GradeLabel))
            sb.AppendLine($"The student is in: {ctx2.GradeLabel}. Calibrate language and expectations to this grade level.");
        if (!string.IsNullOrWhiteSpace(ctx2.Subject))
            sb.AppendLine($"Subject area: {ctx2.Subject}.");
        if (!string.IsNullOrWhiteSpace(ctx2.StudentName))
            sb.AppendLine($"Student name: {ctx2.StudentName}.");
        if (!string.IsNullOrWhiteSpace(ctx2.Term))
            sb.AppendLine($"Term: {ctx2.Term}.");
        if (!string.IsNullOrWhiteSpace(ctx2.CurrentText))
        {
            sb.AppendLine();
            sb.AppendLine("== EXISTING COMMENT TO REFINE ==");
            sb.AppendLine(ctx2.CurrentText);
            sb.AppendLine("(The teacher may ask you to rewrite, shorten, improve tone, or start fresh.)");
        }

        // Layer in shared guidance (grading philosophy, terminology etc.)
        sb.AppendLine();
        AppendSharedGuidance(sb, ai, tc);

        return sb.ToString();
    }

    // ── Shared guidance appended to both prompts ──────────────────────────────

    private static void AppendSharedGuidance(StringBuilder sb, SchoolAiConfig? ai, TeacherAiConfig? tc)
    {
        if (ai == null && tc == null) return;

        sb.AppendLine("== SCHOOL GUIDANCE ==");
        if (!string.IsNullOrWhiteSpace(ai?.GradingPhilosophy))  sb.AppendLine($"  Grading philosophy: {ai!.GradingPhilosophy}");
        if (!string.IsNullOrWhiteSpace(ai?.ToneGuidance))       sb.AppendLine($"  Tone: {ai!.ToneGuidance}");
        if (!string.IsNullOrWhiteSpace(ai?.TerminologyNotes))   sb.AppendLine($"  Terminology: {ai!.TerminologyNotes}");
        if (!string.IsNullOrWhiteSpace(ai?.SpellingGuidance))   sb.AppendLine($"  Spelling: {ai!.SpellingGuidance}");
        if (!string.IsNullOrWhiteSpace(ai?.GrammarGuidance))    sb.AppendLine($"  Grammar: {ai!.GrammarGuidance}");
        if (!string.IsNullOrWhiteSpace(ai?.RubricGuidance))     sb.AppendLine($"  Rubric: {ai!.RubricGuidance}");
        if (!string.IsNullOrWhiteSpace(ai?.AdditionalInstructions)) sb.AppendLine($"  Additional: {ai!.AdditionalInstructions}");

        if (tc != null)
        {
            sb.AppendLine("== TEACHER PREFERENCES ==");
            if (!string.IsNullOrWhiteSpace(tc.PreferredTone))          sb.AppendLine($"  Tone: {tc.PreferredTone}");
            if (!string.IsNullOrWhiteSpace(tc.FocusAreas))             sb.AppendLine($"  Focus: {tc.FocusAreas}");
            if (!string.IsNullOrWhiteSpace(tc.AdditionalInstructions)) sb.AppendLine($"  Additional: {tc.AdditionalInstructions}");
        }
        sb.AppendLine();
    }

    private static (string prose, AssistantAction? action) ParseResponse(string raw)
    {
        const string open  = "<action>";
        const string close = "</action>";

        var start = raw.IndexOf(open,  StringComparison.OrdinalIgnoreCase);
        var end   = raw.IndexOf(close, StringComparison.OrdinalIgnoreCase);

        if (start < 0 || end < 0)
            return (raw.Trim(), null);

        var prose     = raw[..start].Trim();
        var jsonBlock = raw[(start + open.Length)..end].Trim();

        AssistantAction? action = null;
        try
        {
            action = JsonSerializer.Deserialize<AssistantAction>(jsonBlock,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { /* malformed JSON — ignore action, show prose */ }

        return (prose, action);
    }
}
