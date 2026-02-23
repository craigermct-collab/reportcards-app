using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace ReportCards.Web.Services;

public class HomeworkAnalysisRequest
{
    public List<string> ImageDataUrls { get; set; } = new();
    public string GradeName { get; set; } = "";
    public string ClassGroupName { get; set; } = "";
    public bool CheckSpelling { get; set; }
    public bool CheckGrammar { get; set; }
    public bool CheckRubric { get; set; }
    public bool CheckAiGenerated { get; set; }
}

public class HomeworkAnalysisResult
{
    public string? SpellingSummary { get; set; }
    public int? SpellingScore { get; set; }
    public string? GrammarSummary { get; set; }
    public int? GrammarScore { get; set; }
    public string? RubricSummary { get; set; }
    public int? RubricScore { get; set; }
    public string? AiDetectionSummary { get; set; }
    public string? AiLikelihood { get; set; }
    public string? OverallSummary { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset AnalyzedAt { get; set; } = DateTimeOffset.UtcNow;
    public string GradeName { get; set; } = "";
    public string ClassGroupName { get; set; } = "";
}

public class HomeworkAnalysisService
{
    private readonly IConfiguration _config;
    private readonly ILogger<HomeworkAnalysisService> _logger;

    public HomeworkAnalysisService(IConfiguration config, ILogger<HomeworkAnalysisService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<HomeworkAnalysisResult> AnalyzeAsync(HomeworkAnalysisRequest request)
    {
        var result = new HomeworkAnalysisResult
        {
            GradeName = request.GradeName,
            ClassGroupName = request.ClassGroupName
        };

        try
        {
            var endpoint = _config["AZURE_OPENAI_ENDPOINT"]
                ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT not configured.");
            var key = _config["AZURE_OPENAI_KEY"]
                ?? throw new InvalidOperationException("AZURE_OPENAI_KEY not configured.");
            var deployment = _config["AZURE_OPENAI_DEPLOYMENT"] ?? "gpt-4o";

            var client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(key));
            var chatClient = client.GetChatClient(deployment);

            var checks = new List<string>();
            if (request.CheckSpelling) checks.Add("spelling errors (score out of 10, list specific errors)");
            if (request.CheckGrammar) checks.Add("grammar issues (score out of 10, list specific issues)");
            if (request.CheckRubric) checks.Add($"rubric evaluation for {request.GradeName} / {request.ClassGroupName} (score out of 10)");
            if (request.CheckAiGenerated) checks.Add("AI-generated content likelihood (Low/Medium/High with reasoning)");

            var systemPrompt = $"""
                You are an expert educator reviewing student homework for a {request.GradeName} student
                at KinderKollege Private Primary School ({request.ClassGroupName} class group).

                Analyze the provided homework image(s) and respond in valid JSON only.
                No markdown fences, no preamble, just raw JSON.

                Use this exact structure:
                {{
                  "spelling": {{ "score": <int 0-10 or null>, "summary": "<string or null>" }},
                  "grammar": {{ "score": <int 0-10 or null>, "summary": "<string or null>" }},
                  "rubric": {{ "score": <int 0-10 or null>, "summary": "<string or null>" }},
                  "aiDetection": {{ "likelihood": "<Low|Medium|High or null>", "summary": "<string or null>" }},
                  "overallSummary": "<2-3 sentence encouraging but honest summary>"
                }}

                Only populate requested sections. Set unrequested sections to null values.
                Be encouraging but accurate. Use language appropriate for the student's grade level.
                """;

            var userContent = new List<ChatMessageContentPart>();
            userContent.Add(ChatMessageContentPart.CreateTextPart(
                $"Please analyze this homework submission. Checks requested: {string.Join(", ", checks)}."));

            foreach (var dataUrl in request.ImageDataUrls)
            {
                var base64 = dataUrl.Contains(",") ? dataUrl.Split(',')[1] : dataUrl;
                var bytes = Convert.FromBase64String(base64);
                var imageData = BinaryData.FromBytes(bytes);
                userContent.Add(ChatMessageContentPart.CreateImagePart(imageData, "image/jpeg"));
            }

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userContent)
            };

            var response = await chatClient.CompleteChatAsync(messages);
            var json = response.Value.Content[0].Text;

            // Strip markdown fences if model adds them anyway
            json = json.Trim();
            if (json.StartsWith("```")) json = string.Join("\n", json.Split('\n').Skip(1));
            if (json.EndsWith("```")) json = json[..json.LastIndexOf("```")];
            json = json.Trim();

            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (request.CheckSpelling && root.TryGetProperty("spelling", out var spelling))
            {
                result.SpellingScore = TryGetInt(spelling, "score");
                result.SpellingSummary = TryGetString(spelling, "summary");
            }
            if (request.CheckGrammar && root.TryGetProperty("grammar", out var grammar))
            {
                result.GrammarScore = TryGetInt(grammar, "score");
                result.GrammarSummary = TryGetString(grammar, "summary");
            }
            if (request.CheckRubric && root.TryGetProperty("rubric", out var rubric))
            {
                result.RubricScore = TryGetInt(rubric, "score");
                result.RubricSummary = TryGetString(rubric, "summary");
            }
            if (request.CheckAiGenerated && root.TryGetProperty("aiDetection", out var ai))
            {
                result.AiLikelihood = TryGetString(ai, "likelihood");
                result.AiDetectionSummary = TryGetString(ai, "summary");
            }
            if (root.TryGetProperty("overallSummary", out var overall))
                result.OverallSummary = overall.ValueKind != System.Text.Json.JsonValueKind.Null ? overall.GetString() : null;

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Homework analysis failed");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private static int? TryGetInt(System.Text.Json.JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.Number
            ? v.GetInt32() : null;

    private static string? TryGetString(System.Text.Json.JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.String
            ? v.GetString() : null;
}
