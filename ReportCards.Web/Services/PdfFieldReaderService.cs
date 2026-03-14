using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Advanced;

namespace ReportCards.Web.Services;

/// <summary>
/// Reads AcroForm field names from a PDF template file.
/// Uses two strategies:
///   1. Walk the AcroForm field tree via PdfSharp
///   2. Walk each page's /Annots array via the low-level PDF dictionary API
///      to catch fields that are present as widget annotations but not in /Fields
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

            // ── Strategy 1: AcroForm field tree ──────────────────────────
            var form = doc.AcroForm;
            if (form != null)
                CollectFromFieldTree(form.Fields, seen, results);

            // ── Strategy 2: page widget annotation scan ───────────────────
            // Catches fields present as /Widget annotations but missing from /Fields array
            foreach (var page in doc.Pages)
            {
                try
                {
                    // Access the raw /Annots array from the page dictionary
                    if (!page.Elements.ContainsKey("/Annots")) continue;

                    var annotsObj = page.Elements.GetObject("/Annots");
                    PdfArray? annotArray = annotsObj as PdfArray;

                    // Could be an indirect reference to an array
                    if (annotArray == null && annotsObj is PdfReference annotRef)
                        annotArray = annotRef.Value as PdfArray;

                    if (annotArray == null) continue;

                    foreach (var element in annotArray.Elements)
                    {
                        try
                        {
                            // Each element may be a direct dict or an indirect reference
                            PdfDictionary? annot = element as PdfDictionary;
                            if (annot == null && element is PdfReference elemRef)
                                annot = elemRef.Value as PdfDictionary;
                            if (annot == null) continue;

                            // Only process Widget annotations
                            var subtype = annot.Elements.GetName("/Subtype");
                            if (subtype != "/Widget") continue;

                            // Get field name from /T (may be on widget or parent)
                            var name = GetFieldName(annot);
                            if (string.IsNullOrWhiteSpace(name)) continue;
                            if (!seen.Add(name)) continue;

                            // Determine field type from /FT (may be on widget or parent)
                            var ft = annot.Elements.GetName("/FT");
                            if (string.IsNullOrEmpty(ft))
                            {
                                var parent = GetParent(annot);
                                if (parent != null)
                                    ft = parent.Elements.GetName("/FT");
                            }

                            var fieldType = ft switch
                            {
                                "/Tx"  => "Text",
                                "/Btn" => "Checkbox",
                                "/Ch"  => "ComboBox",
                                _      => "Other"
                            };

                            results.Add(new PdfFieldInfo(name, fieldType));
                        }
                        catch { /* skip malformed annotation */ }
                    }
                }
                catch { /* skip malformed page */ }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read PDF fields from {File}", fileName);
        }

        return results.OrderBy(f => f.Name).ToList();
    }

    // ── AcroForm tree traversal ───────────────────────────────────────────

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

    // ── Helpers ───────────────────────────────────────────────────────────

    private static string? GetFieldName(PdfDictionary annot)
    {
        // Try /T directly on the widget
        var t = annot.Elements.GetString("/T");
        if (!string.IsNullOrWhiteSpace(t)) return t;

        // Fall back to parent's /T
        var parent = GetParent(annot);
        if (parent != null)
        {
            var pt = parent.Elements.GetString("/T");
            if (!string.IsNullOrWhiteSpace(pt)) return pt;
        }

        return null;
    }

    private static PdfDictionary? GetParent(PdfDictionary annot)
    {
        try
        {
            var parentEl = annot.Elements["/Parent"];
            if (parentEl == null) return null;
            if (parentEl is PdfDictionary pd) return pd;
            if (parentEl is PdfReference pr) return pr.Value as PdfDictionary;
        }
        catch { }
        return null;
    }
}

public record PdfFieldInfo(string Name, string FieldType);
