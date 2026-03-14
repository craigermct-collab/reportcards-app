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
            // Read entire file into memory once — PDFtoImage/PDFium can close/invalidate
            // a FileStream internally, so we use a MemoryStream that we control.
            var pdfBytes = File.ReadAllBytes(path);
            var result = new List<string>();

            // Get page count from a fresh stream
            var pageCount = Conversion.GetPageCount(new MemoryStream(pdfBytes));

            for (int i = 0; i < pageCount; i++)
            {
                // Fresh MemoryStream per page — avoids any position/disposal issues
                using var pageStream = new MemoryStream(pdfBytes);
                using var bitmap = Conversion.ToImage(pageStream, page: i, options: new(Dpi: dpi));
                using var ms = new MemoryStream();
                bitmap.Encode(ms, SKEncodedImageFormat.Png, 90);
                result.Add($"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}");
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
