namespace ReportCards.Web.Services;

public enum AssistantMode
{
    /// <summary>Casual, friendly general-purpose assistant shown in the floating chat bubble.</summary>
    General,
    /// <summary>Focused, professional comment-writing assistant summoned from grade entry.</summary>
    CommentWriter,
}

/// <summary>
/// Describes where the user is in the app and what they're working on.
/// Any page/component can set this to give the assistant relevant context.
/// </summary>
public class AssistantContext
{
    public AssistantMode Mode { get; set; } = AssistantMode.General;
    public string Page { get; set; } = "dashboard";
    public string? PageLabel { get; set; }
    public string? StudentName { get; set; }
    public string? StudentFirstName { get; set; }
    public string? GradeLabel { get; set; }
    public string? Subject { get; set; }
    public string? Term { get; set; }
    public string? CurrentText { get; set; }
    public Dictionary<string, string> Extra { get; set; } = new();
    public string? SuggestedPrompt { get; set; }
}

/// <summary>
/// Scoped service — pages push context here, chat drawer reads it.
/// </summary>
public class AssistantContextService
{
    private AssistantContext _current = new();

    public AssistantContext Current => _current;

    public event Action? OnContextChanged;

    public void SetContext(AssistantContext context)
    {
        _current = context;
        OnContextChanged?.Invoke();
    }

    public void ClearContext()
    {
        _current = new();
        OnContextChanged?.Invoke();
    }

    public string BuildContextPrompt()
    {
        if (_current.Page == "dashboard" && _current.PageLabel == null)
            return "";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("== CURRENT PAGE CONTEXT ==");
        sb.AppendLine($"Page: {_current.PageLabel ?? _current.Page}");

        if (!string.IsNullOrWhiteSpace(_current.StudentName))
            sb.AppendLine($"Student: {_current.StudentName}");
        if (!string.IsNullOrWhiteSpace(_current.Subject))
            sb.AppendLine($"Subject: {_current.Subject}");
        if (!string.IsNullOrWhiteSpace(_current.Term))
            sb.AppendLine($"Term: {_current.Term}");
        if (!string.IsNullOrWhiteSpace(_current.CurrentText))
            sb.AppendLine($"Current text: \"{_current.CurrentText}\"");

        foreach (var kv in _current.Extra)
            sb.AppendLine($"{kv.Key}: {kv.Value}");

        sb.AppendLine();
        sb.AppendLine("Tailor your response to this context. If the teacher is editing a comment, " +
                      "offer to help write or improve it. If they're on a student profile, " +
                      "focus on that student.");

        return sb.ToString();
    }
}
