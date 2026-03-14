using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using ReportCards.Web.Data;

namespace ReportCards.Web.Services;

/// <summary>
/// Reads AcroForm field names directly from a PDF template file.
/// Uses two strategies:
///   1. Walk the AcroForm field tree (catches most fields)
///   2. Walk every page's widget annotations (catches fields the tree misses)
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

            // ── Strategy 1: walk the AcroForm field tree ──────────────────
            var form = doc.AcroForm;
            if (form != null)
                CollectFromFieldTree(form.Fields, seen, results);

            // ── Strategy 2: walk every page's widget annotations ──────────
            // This catches fields that are present as widgets but not properly
            // linked into the AcroForm /Fields array (common in scanned/converted PDFs)
            foreach (var page in doc.Pages)
            {
                try
                {
                    var annots = page.Elements["/Annots"];
                    if (annots == null) continue;

                    PdfArray? annotArray = annots as PdfArray;
                    if (annotArray == null && annots is PdfReference annotRef)
                        annotArray = annotRef.Value as PdfArray;
                    if (annotArray == null) continue;

                    foreach (var item in annotArray.Elements)
                    {
                        try
                        {
                            PdfDictionary? annot = item as PdfDictionary;
                            if (annot == null && item is PdfReference r)
                                annot = r.Value as PdfDictionary;
                            if (annot == null) continue;

                            // Only Widget annotations
                            var subtypeEl = annot.Elements["/Subtype"];
                            var subtype = subtypeEl?.ToString() ?? "";
                            if (subtype != "/Widget") continue;

                            // Get field name — check /T directly, or follow /Parent
                            var name = GetFieldName(annot, doc);
                            if (string.IsNullOrWhiteSpace(name)) continue;
                            if (!seen.Add(name)) continue;

                            var ftEl = annot.Elements["/FT"]?.ToString() ?? "";
                            // Also check parent for /FT
                            if (string.IsNullOrEmpty(ftEl))
                            {
                                var parent = GetParentDict(annot, doc);
                                if (parent != null)
                                    ftEl = parent.Elements["/FT"]?.ToString() ?? "";
                            }

                            var fieldType = ftEl switch
                            {
                                "/Tx"  => "Text",
                                "/Btn" => IsRadioOrCheckbox(annot, doc),
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

    // ── AcroForm tree walker ──────────────────────────────────────────────

    private static void CollectFromFieldTree(PdfAcroField.PdfAcroFieldCollection fields,
                                             HashSet<string> seen, List<PdfFieldInfo> results)
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

    // ── Helpers for widget annotation traversal ───────────────────────────

    private static string? GetFieldName(PdfDictionary annot, PdfDocument doc)
    {
        // /T on the widget itself
        var t = annot.Elements["/T"];
        if (t != null)
        {
            var name = t.ToString()?.Trim('(', ')');
            if (!string.IsNullOrWhiteSpace(name)) return name;
        }

        // Follow /Parent chain to find /T
        var parent = GetParentDict(annot, doc);
        if (parent != null)
        {
            var pt = parent.Elements["/T"];
            if (pt != null)
            {
                var name = pt.ToString()?.Trim('(', ')');
                if (!string.IsNullOrWhiteSpace(name)) return name;
            }
        }

        return null;
    }

    private static PdfDictionary? GetParentDict(PdfDictionary annot, PdfDocument doc)
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

    private static string IsRadioOrCheckbox(PdfDictionary annot, PdfDocument doc)
    {
        try
        {
            // Check /Ff (field flags) bit 16 = radio button
            var ff = annot.Elements["/Ff"];
            if (ff == null)
            {
                var parent = GetParentDict(annot, doc);
                ff = parent?.Elements["/Ff"];
            }
            if (ff != null && int.TryParse(ff.ToString(), out int flags))
                return (flags & (1 << 15)) != 0 ? "Radio" : "Checkbox";
        }
        catch { }
        return "Checkbox";
    }
}

public record PdfFieldInfo(string Name, string FieldType);
