using Microsoft.EntityFrameworkCore;
using ReportCards.Web.Data;

namespace ReportCards.Web.Services;

/// <summary>
/// Loads school configuration from the DB once per Blazor circuit.
/// Inject as scoped — call EnsureLoadedAsync() before accessing properties.
/// </summary>
public class SchoolConfigService
{
    private readonly SchoolDbContext _db;
    private Dictionary<string, string> _values = new();
    private bool _loaded;

    public SchoolConfigService(SchoolDbContext db) => _db = db;

    public async Task EnsureLoadedAsync()
    {
        if (_loaded) return;
        var rows = await _db.SchoolConfigs.ToListAsync();
        _values = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Value))
            .ToDictionary(r => r.Key, r => r.Value!);
        _loaded = true;
    }

    public string Get(string key, string fallback = "") =>
        _values.TryGetValue(key, out var v) ? v : fallback;

    // ── Typed convenience properties ──────────────────────────────

    public string SchoolName    => Get(SchoolConfigKeys.SchoolName,    "KinderKollege");
    public string LogoUrl       => Get(SchoolConfigKeys.LogoUrl,       "https://www.kinderkollege.ca/images/logo_4.png");
    public string PrimaryColor  => Get(SchoolConfigKeys.PrimaryColor,  "#1B4F72");
    public string SecondaryColor => Get(SchoolConfigKeys.SecondaryColor, "#E67E22");
    public string NavDarkColor  => Get(SchoolConfigKeys.NavDarkColor,  "#1B4F72");
    public string ContactEmail  => Get(SchoolConfigKeys.ContactEmail,  "");
    public string ContactPhone  => Get(SchoolConfigKeys.ContactPhone,  "");
    public string Address       => Get(SchoolConfigKeys.Address,       "");
}
