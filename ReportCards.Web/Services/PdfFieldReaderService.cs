using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using ReportCards.Web.Data;

namespace ReportCards.Web.Services;

/// <summary>
/// Reads AcroForm field names directly from a PDF template file.
/// Used to populate the mapping UI without manual entry.
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

    /// <summary>
    /// Returns all AcroForm field names from the given PDF template file,
    /// sorted alphabetically. Returns empty list if file not found or has no fields.
    /// </summary>
    public List<PdfFieldInfo> GetFields(string fileName)
    {
        var path = Path.Combine(_env.ContentRootPath, TemplatesFolder, fileName);
        if (!File.Exists(path))
        {
            _logger.LogWarning("PDF template not found: {Path}", path);
            return new();
        }

        var results = new List<PdfFieldInfo>();
        try
        {
            using var doc  = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            var form = doc.AcroForm;
            if (form == null) return results;

            CollectFields(form.Fields, results, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read PDF fields from {File}", fileName);
        }

        return results.OrderBy(f => f.Name).ToList();
    }

    private static void CollectFields(PdfAcroField.PdfAcroFieldCollection fields,
                                      List<PdfFieldInfo> results, string prefix)
    {
        for (int i = 0; i < fields.Count; i++)
        {
            try
            {
                var field = fields[i];
                if (field == null) continue;

                var name = field.Name;
                if (string.IsNullOrWhiteSpace(name)) continue;

                var fieldType = field switch
                {
                    PdfTextField      => "Text",
                    PdfCheckBoxField  => "Checkbox",
                    PdfRadioButtonField => "Radio",
                    PdfComboBoxField  => "ComboBox",
                    _                 => "Other"
                };

                results.Add(new PdfFieldInfo(name, fieldType));

                // Recurse into child fields if any
                if (field.Fields is { Count: > 0 })
                    CollectFields(field.Fields, results, name + ".");
            }
            catch { /* skip malformed fields */ }
        }
    }
}

public record PdfFieldInfo(string Name, string FieldType);
