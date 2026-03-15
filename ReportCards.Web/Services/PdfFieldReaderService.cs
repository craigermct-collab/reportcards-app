using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using ReportCards.Web.Data;

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

    public List<PdfFieldInfo> GetFields(string fileName, ReportCardTemplateType? templateType = null)
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
        // Ontario Elementary Report Card (RC1) — all 128 fields verified via pypdf
        // grouped by their actual page in the PDF.
        // PdfSharp's AcroForm field tree only returns ~56; the rest are widget
        // annotations not linked into the /Fields array.
        // Kindergarten Communication of Learning — keyed on template type, filename-independent
        if (templateType == ReportCardTemplateType.KindergartenCommunicationOfLearning
            || fileName.StartsWith("Kindergarten", StringComparison.OrdinalIgnoreCase)) // fallback
        {
            // All fields verified via full widget annotation scan (pypdf)
            // Text fields
            yield return ("Student",            "Text");
            yield return ("OEN",                "Text");
            yield return ("Teacher",            "Text");
            yield return ("School",             "Text");
            yield return ("Board",              "Text");
            yield return ("Address",            "Text");
            yield return ("Telephone",          "Text");
            yield return ("Principal",          "Text");
            yield return ("Date",               "Text");
            yield return ("DaysAbsent",         "Text");
            yield return ("TotalDaysAbsent",    "Text");
            yield return ("TimesLate",          "Text");
            yield return ("TotalTimesLate",     "Text");
            yield return ("BelAndConNotes",     "Text");
            yield return ("SelfRegAndWellNotes","Text");
            yield return ("LitAndMathNotes",    "Text");
            yield return ("ProbAndInnNotes",    "Text");
            // Checkboxes
            yield return ("SeptemberYear2",     "Checkbox");
            yield return ("SeptemberGd1",       "Checkbox");
            yield return ("ReportPgYr1",        "Checkbox");
            yield return ("ReportPgYr2",        "Checkbox");
            yield return ("BelAndConESL",       "Checkbox");
            yield return ("BelAndConIEP",       "Checkbox");
            yield return ("SelfRegAndWelESL",   "Checkbox");
            yield return ("SelfRegAndWelIEP",   "Checkbox");
            yield return ("LitAndMathESL",      "Checkbox");
            yield return ("LitAndMathIEP",      "Checkbox");
            yield return ("ProbAndInnESL",      "Checkbox");
            yield return ("ProbAndInnIEP",      "Checkbox");
            yield break;
        }

        if (!fileName.Equals("elementary-report-card.pdf", StringComparison.OrdinalIgnoreCase))
            yield break;

        // ── Page 1: Header ──────────────────────────────────────────────
        yield return ("Student",             "Text");
        yield return ("OEN",                 "Text");
        yield return ("Grade",               "Text");
        yield return ("GradeInSeptember",    "Text");
        yield return ("Teacher",             "Text");
        yield return ("Board",               "Text");
        yield return ("School",              "Text");
        yield return ("Address",             "Text");
        yield return ("Principal",           "Text");
        yield return ("Telephone",           "Text");
        yield return ("DaysAbsent",          "Text");
        yield return ("TotalDaysAbsent",     "Text");
        yield return ("TimesLate",           "Text");
        yield return ("TotalTimesLate",      "Text");

        // ── Page 1: Learning Skills ─────────────────────────────────────
        yield return ("Term1Responsibiity",  "Text");  // typo in PDF
        yield return ("Term2Responsibiity",  "Text");
        yield return ("Term1Organization",   "Text");
        yield return ("Term2Organization",   "Text");
        yield return ("Term1IndependentWork","Text");
        yield return ("Term2IndependentWork","Text");
        yield return ("Term1Collaboration",  "Text");
        yield return ("Term2Collaboration",  "Text");
        yield return ("Term1Initiative",     "Text");
        yield return ("Term2Initiative",     "Text");
        yield return ("Term1SelfRegulation", "Text");
        yield return ("Term2SelfRegulation", "Text");

        // ── Page 2: Language ────────────────────────────────────────────
        yield return ("LanguageNA",          "Checkbox");
        yield return ("LanguageESLELD",      "Checkbox");
        yield return ("LanguageIEP",         "Checkbox");
        yield return ("LanguageTerm1",       "Text");
        yield return ("LanguageTerm2",       "Text");
        yield return ("LanguageNotes",       "Text");

        // ── Page 2: French ──────────────────────────────────────────────
        yield return ("FrenchNA",            "Checkbox");
        yield return ("LIsteningESLELD",     "Checkbox");  // typo in PDF
        yield return ("ListeningIEP",        "Checkbox");
        yield return ("SpeakingESLELD",      "Checkbox");
        yield return ("SpeakingIEP",         "Checkbox");
        yield return ("ReadingESLELD",       "Checkbox");
        yield return ("ReadingIEP",          "Checkbox");
        yield return ("WritingESLELD",       "Checkbox");
        yield return ("WritingIEP",          "Checkbox");
        yield return ("FrenchCore",          "Checkbox");
        yield return ("FrenchImmersion",     "Checkbox");
        yield return ("FrenchExtended",      "Checkbox");
        yield return ("FrenchListeningTerm1","Text");
        yield return ("FrenchListeningTerm2","Text");
        yield return ("FrenchSpeakingTerm1", "Text");
        yield return ("FrenchSpeakingTerm2", "Text");
        yield return ("FrenchReadingTerm1",  "Text");
        yield return ("FrenchReadingTerm2",  "Text");
        yield return ("FrenchWritingTerm1",  "Text");
        yield return ("FrenchWritingTerm2",  "Text");
        yield return ("FrenchNotes",         "Text");

        // ── Page 2: Native Language ─────────────────────────────────────
        yield return ("NativeLanguageESLELD","Checkbox");
        yield return ("NativeLanguageIEP",   "Checkbox");
        yield return ("NativeLanguageNA",    "Checkbox");
        yield return ("NativeLanguageTerm1", "Text");
        yield return ("NativeLanguageTerm2", "Text");
        yield return ("NativeLanguageNotes", "Text");

        // ── Page 2: Mathematics ─────────────────────────────────────────
        yield return ("MathematicsESLELD",   "Checkbox");
        yield return ("MathematicsIEP",      "Checkbox");
        yield return ("MathematicsFrench",   "Checkbox");
        yield return ("MathematicsTerm1",    "Text");
        yield return ("MathematicsTerm2",    "Text");
        yield return ("MathematicsNotes",    "Text");

        // ── Page 2: Science & Technology ───────────────────────────────
        yield return ("ScienceAndTechESLELD","Checkbox");
        yield return ("ScienceAndTechIEP",   "Checkbox");
        yield return ("ScienceAndTechFrench","Checkbox");
        yield return ("ScienceAndTechTerm1", "Text");
        yield return ("ScienceAndTechTerm2", "Text");
        yield return ("ScienceAndTechNotes", "Text");

        // ── Page 3: Social Studies ──────────────────────────────────────
        yield return ("SocialStudiesESLELD", "Checkbox");
        yield return ("SocialStudiesIEP",    "Checkbox");
        yield return ("SocialStudiesFrench", "Checkbox");
        yield return ("SocialStudiesTerm1",  "Text");
        yield return ("SocialStudiesTerm2",  "Text");
        yield return ("SocialStudiesNotes",  "Text");

        // ── Page 3: Health & Physical Education ─────────────────────────
        yield return ("HealthyESLELD",            "Checkbox");
        yield return ("HealthyIEP",               "Checkbox");
        yield return ("HealthyFrench",            "Checkbox");
        yield return ("MovementESLELD",           "Checkbox");
        yield return ("MovementIEP",              "Checkbox");
        yield return ("MovementFrench",           "Checkbox");
        yield return ("HealthHealthyLivingTerm1", "Text");
        yield return ("HealthHealthyLivingTerm2", "Text");
        yield return ("HealthActiveLivingTerm1",  "Text");
        yield return ("HealthActiveLivingTerm2",  "Text");
        yield return ("HealthMovementTerm1",      "Text");
        yield return ("HealthMovementTerm2",      "Text");
        yield return ("HealthPhysEdNotes",        "Text");

        // ── Page 3: The Arts ────────────────────────────────────────────
        yield return ("DanceESLELD",   "Checkbox");
        yield return ("DanceIEP",      "Checkbox");
        yield return ("DanceFrench",   "Checkbox");
        yield return ("DanceNA",       "Checkbox");
        yield return ("DanceTerm1",    "Text");
        yield return ("DanceTerm2",    "Text");
        yield return ("DramaESLELD",   "Checkbox");
        yield return ("DramaIEP",      "Checkbox");
        yield return ("DramaFrench",   "Checkbox");
        yield return ("DramaNA",       "Checkbox");
        yield return ("DramaTerm1",    "Text");
        yield return ("DramaTerm2",    "Text");
        yield return ("MusicESLELD",   "Checkbox");
        yield return ("MusicIEP",      "Checkbox");
        yield return ("MusicFrench",   "Checkbox");
        yield return ("MusicNA",       "Checkbox");
        yield return ("MusicTerm1",    "Text");
        yield return ("MusicTerm2",    "Text");
        yield return ("VisualArtsESLELD",  "Checkbox");
        yield return ("VisualArtsIEP",     "Checkbox");
        yield return ("VisualArtsFrench",  "Checkbox");
        yield return ("VisualArtsNA",      "Checkbox");
        yield return ("VisualArtsTerm1",   "Text");
        yield return ("VisualArtsTerm2",   "Text");
        yield return ("CustomESLELD",  "Checkbox");
        yield return ("CustomIEP",     "Checkbox");
        yield return ("CustomFrench",  "Checkbox");
        yield return ("CustomNA",      "Checkbox");
        yield return ("CustomTerm1",   "Text");
        yield return ("CustomTerm2",   "Text");
        yield return ("CustomNotes",   "Text");
        yield return ("TheArtsNotes",  "Text");

        // ── Page 3: ERS ─────────────────────────────────────────────────
        yield return ("ERS",         "Checkbox");
        yield return ("BenchmarkYes","Checkbox");
        yield return ("BenchmarkNo", "Checkbox");
        yield return ("ERSday",      "Text");
        yield return ("ERSmonth",    "Text");
        yield return ("ERSyear",     "Text");
    }
}

public record PdfFieldInfo(string Name, string FieldType);
