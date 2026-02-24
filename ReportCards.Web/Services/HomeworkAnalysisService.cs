using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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
    private readonly HttpClient _http;

    public HomeworkAnalysisService(IConfiguration config, ILogger<HomeworkAnalysisService> logger)
    {
        _config = config;
        _logger = logger;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
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
            var endpoint = (_config["AZURE_OPENAI_ENDPOINT"] ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT not configured.")).TrimEnd('/');
            var key = _config["AZURE_OPENAI_KEY"] ?? throw new InvalidOperationException("AZURE_OPENAI_KEY not configured.");
            var deployment = _config["AZURE_OPENAI_DEPLOYMENT"] ?? "gpt-4o";
            var apiVersion = _config["AZURE_OPENAI_API_VERSION"] ?? "2025-01-01-preview";

            // Build exact URL
            var url = $"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";

            var checks = new List<string>();
            if (request.CheckSpelling) checks.Add("spelling errors (score out of 10, list specific errors)");
            if (request.CheckGrammar) checks.Add("grammar issues (score out of 10, list specific issues)");
            if (request.CheckRubric) checks.Add($"rubric evaluation for {request.GradeName} / {request.ClassGroupName} (score out of 10)");
            if (request.CheckAiGenerated) checks.Add("AI-generated content likelihood (Low/Medium/High with reasoning)");

            var jsonSchema = @"{
  ""spelling"": { ""score"": <int 0-10 or null>, ""summary"": ""<string or null>"" },
  ""grammar"": { ""score"": <int 0-10 or null>, ""summary"": ""<string or null>"" },
  ""rubric"": { ""score"": <int 0-10 or null>, ""summary"": ""<string or null>"" },
  ""aiDetection"": { ""likelihood"": ""<Low|Medium|High or null>"", ""summary"": ""<string or null>"" },
  ""overallSummary"": ""<2-3 sentence encouraging but honest summary>""
}";

            var systemPrompt =
                $"You are an expert educator reviewing student homework for a {request.GradeName} student " +
                $"at KinderKollege Private Primary School ({request.ClassGroupName} class group).\n\n" +
                "Analyze the provided homework image(s) and respond in valid JSON only.\n" +
                "No markdown fences, no preamble, just raw JSON.\n\n" +
                "Use this exact structure:\n" + jsonSchema + "\n\n" +
                "Only populate requested sections. Set unrequested sections to null values.\n" +
                "Be encouraging but accurate. Use language appropriate for the student's grade level.";

            // Build messages array with images
            var userContentParts = new List<object>
            {
                new { type = "text", text = $"Please analyze this homework submission. Checks requested: {string.Join(", ", checks)}." }
            };

            foreach (var dataUrl in request.ImageDataUrls)
            {
                var base64 = dataUrl.Contains(",") ? dataUrl.Split(',')[1] : dataUrl;
                var mimeType = dataUrl.Contains("data:") ? dataUrl.Split(';')[0].Replace("data:", "") : "image/jpeg";
                userContentParts.Add(new
                {
                    type = "image_url",
                    image_url = new { url = $"data:{mimeType};base64,{base64}" }
                });
            }

            var body = new
            {
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userContentParts }
                },
                max_tokens = 2000,
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(body);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Add("api-key", key);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"HTTP {(int)response.StatusCode} ({response.StatusCode}): {responseBody}");

            var responseDoc = JsonDocument.Parse(responseBody);
            var content = responseDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            // Strip markdown fences if model adds them
            content = content.Trim();
            if (content.StartsWith("```")) content = string.Join("\n", content.Split('\n').Skip(1));
            if (content.EndsWith("```")) content = content[..content.LastIndexOf("```")];
            content = content.Trim();

            var doc = JsonDocument.Parse(content);
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
                result.OverallSummary = overall.ValueKind != JsonValueKind.Null ? overall.GetString() : null;

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Homework analysis failed");
            result.Success = false;
            result.ErrorMessage = ex.InnerException != null
                ? $"{ex.Message} → {ex.InnerException.Message}"
                : ex.Message;
        }

        return result;
    }

    private static int? TryGetInt(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number
            ? v.GetInt32() : null;

    private static string? TryGetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() : null;
}
