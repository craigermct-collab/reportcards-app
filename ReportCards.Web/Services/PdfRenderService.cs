using PDFtoImage;
using SkiaSharp;

namespace ReportCards.Web.Services;

/// <summary>
/// Renders PDF pages to base64-encoded PNG images for the mapping UI overlay.
/// Uses PDFtoImage (PDFium wrapper) for accurate rendering.
/// </summary>
public class PdfRenderService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PdfRenderService> _logger;
    private const string TemplatesFolder = "ReportCardTemplates";

    // Cache rendered pages so we don't re-render on every Blazor render cycle
    private readonly Dictionary<string, List<string>> _cache = new();
    private readonly Dictionary<string, string> _lastError = new();

    public string? GetLastError(string fileName) =>
        _lastError.TryGetValue(fileName, out var err) ? err : null;

    public PdfRenderService(IWebHostEnvironment env, ILogger<PdfRenderService> logger)
    {
        _env    = env;
        _logger = logger;
    }

    /// <summary>Returns base64 PNG data URLs for each page of the PDF, at the given DPI.</summary>
    public List<string> GetPageImages(string fileName, int dpi = 120)
    {
        var cacheKey = $"{fileName}@{dpi}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        var path = Path.Combine(_env.ContentRootPath, TemplatesFolder, fileName);
        if (!File.Exists(path))
        {
            _logger.LogWarning("PDF not found for rendering: {Path}", path);
            return new();
        }

        try
        {
            var result = new List<string>();
            using var fs = File.OpenRead(path);
            var pageCount = Conversion.GetPageCount(fs);
            fs.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < pageCount; i++)
            {
                fs.Seek(0, SeekOrigin.Begin);
                // PDFtoImage 4.x: RenderOptions struct with positional Dpi
                using var bitmap = Conversion.ToImage(fs, page: i, options: new(Dpi: dpi));
                using var ms = new MemoryStream();
                bitmap.Encode(ms, SKEncodedImageFormat.Png, 90);
                var b64 = Convert.ToBase64String(ms.ToArray());
                result.Add($"data:image/png;base64,{b64}");
            }

            _cache[cacheKey] = result;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render PDF pages for {File}", fileName);
            // Store the error message so the UI can surface it
            _lastError[fileName] = ex.Message + (ex.InnerException != null ? " — " + ex.InnerException.Message : "");
            return new();
        }
    }

    /// <summary>Clear the cache when a template file changes.</summary>
    public void InvalidateCache(string fileName) =>
        _cache.Remove(fileName);
}
