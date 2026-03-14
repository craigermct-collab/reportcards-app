using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;

namespace ReportCards.Web.Services;

/// <summary>
/// Reads AcroForm field names from a PDF template file.
/// Uses PdfSharp's field tree walk, then supplements with any well-known
/// fields that PdfSharp misses due to non-standard AcroForm structure.
/// </summary>
public class PdfFieldReaderService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PdfFieldReaderService> _logger;
    private const string TemplatesFolder = "ReportCardTemplates";

    public PdfFieldReaderService(IWebHostEnvironment env, ILogger<PdfFieldReaderService> logger)
    {
        _env    = env;
        _logger = logger;
    }

    public List<PdfFieldInfo> GetFields(string fileName)
    {
        var path = Path.Combine(_env.ContentRootPath, TemplatesFolder, fileName);
        if (!File.Exists(path))
        {
            _logger.LogWarning("PDF template not found: {Path}", path);
            return new();
        }

        var seen    = new HashSet<string>(StringComparer.Ordinal);
        var results = new List<PdfFieldInfo>();

        try
        {
            using var doc = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            var form = doc.AcroForm;
            if (form != null)
                CollectFromFieldTree(form.Fields, seen, results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read PDF fields from {File}", fileName);
        }

        // Supplement with known fields that PdfSharp misses from this PDF's
        // non-standard AcroForm structure (verified via pypdf extraction).
        // These are the Ontario Elementary Report Card header fields that live
        // as widget annotations but are not linked into the /Fields array.
        var knownSupplements = GetKnownSupplementFields(fileName);
        foreach (var (name, fieldType) in knownSupplements)
        {
            if (seen.Add(name))
                results.Add(new PdfFieldInfo(name, fieldType));
        }

        return results.OrderBy(f => f.Name).ToList();
    }

    // ── AcroForm field tree traversal ─────────────────────────────────────

    private static void CollectFromFieldTree(
        PdfAcroField.PdfAcroFieldCollection fields,
        HashSet<string> seen,
        List<PdfFieldInfo> results)
    {
        for (int i = 0; i < fields.Count; i++)
        {
            try
            {
                var field = fields[i];
                if (field == null) continue;

                var name = field.Name;
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (!seen.Add(name)) continue;

                var fieldType = field switch
                {
                    PdfTextField        => "Text",
                    PdfCheckBoxField    => "Checkbox",
                    PdfRadioButtonField => "Radio",
                    PdfComboBoxField    => "ComboBox",
                    _                  => "Other"
                };

                results.Add(new PdfFieldInfo(name, fieldType));

                if (field.Fields is { Count: > 0 })
                    CollectFromFieldTree(field.Fields, seen, results);
            }
            catch { }
        }
    }

    // ── Known supplement fields per template filename ─────────────────────
    // Fields verified present in the PDF via pypdf but not returned by PdfSharp.
    // Keyed by filename so we can extend for other templates without risk.

    private static IEnumerable<(string Name, string FieldType)> GetKnownSupplementFields(string fileName)
    {
        // Ontario Elementary Report Card (RC1) — verified 2025-03 via pypdf
        if (fileName.Equals("elementary-report-card.pdf", StringComparison.OrdinalIgnoreCase))
        {
            // Page 1 header fields — present as widgets, absent from /Fields array
            yield return ("Student",        "Text");
            yield return ("OEN",            "Text");
            yield return ("Grade",          "Text");
            yield return ("GradeInSeptember","Text");
            yield return ("Teacher",        "Text");
            yield return ("Board",          "Text");
            yield return ("School",         "Text");
            yield return ("Address",        "Text");
            yield return ("Principal",      "Text");
            yield return ("Telephone",      "Text");
            yield return ("DaysAbsent",     "Text");
            yield return ("TotalDaysAbsent","Text");
            yield return ("TimesLate",      "Text");
            yield return ("TotalTimesLate", "Text");
        }
    }
}

public record PdfFieldInfo(string Name, string FieldType);
