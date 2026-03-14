using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using PdfSharp.Fonts;
using Microsoft.AspNetCore.Authentication.Google;
using MudBlazor.Services;
using ReportCards.Web.Data;
using ReportCards.Web.Components;
using ReportCards.Web.Services;

// Register PdfSharp font resolver for Linux/Azure (no system fonts available)
GlobalFontSettings.FontResolver = new NullFontResolver();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 20 * 1024 * 1024; // 20MB
    });

builder.Services.AddMudServices();

// Add Razor Pages for login/logout handling
builder.Services.AddRazorPages();

builder.Services.AddDbContextFactory<SchoolDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.CallbackPath = "/signin-google";
});

builder.Services.AddScoped<HomeworkAnalysisService>();
builder.Services.AddScoped<AssistantContextService>();
builder.Services.AddScoped<AssistantService>();
builder.Services.AddScoped<CommentTemplateService>();
builder.Services.AddScoped<ReportCardGeneratorService>();
builder.Services.AddScoped<PdfFieldReaderService>();
builder.Services.AddSingleton<PdfRenderService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<SchoolConfigService>();
builder.Services.AddScoped<CurriculumStampService>();
builder.Services.AddHttpClient();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed minimal data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
    await db.Database.MigrateAsync();

    // Seed immutable reference data (ClassGroupTypes, Grades, GradingScales)
    await DbSeeder.SeedReferenceDataAsync(db);

    // Seed built-in report card PDF templates
    if (!await db.ReportCardTemplates.AnyAsync())
    {
        db.ReportCardTemplates.AddRange(
            new ReportCards.Web.Data.ReportCardTemplate
            {
                Name         = "Elementary Progress Report",
                FileName     = "progress-report.pdf",
                TemplateType = ReportCards.Web.Data.ReportCardTemplateType.ElementaryProgressReport,
                CreatedAt    = DateTimeOffset.UtcNow,
            },
            new ReportCards.Web.Data.ReportCardTemplate
            {
                Name         = "Elementary Report Card",
                FileName     = "elementary-report-card.pdf",
                TemplateType = ReportCards.Web.Data.ReportCardTemplateType.ElementaryReportCard,
                CreatedAt    = DateTimeOffset.UtcNow,
            }
        );
        await db.SaveChangesAsync();
    }

        if (!await db.Teachers.AnyAsync())
    {
        db.Teachers.Add(new Teacher { DisplayName = "Craig (Teacher)", Email = "teacher1@example.com" });
        await db.SaveChangesAsync();
    }

    if (!await db.AppUsers.AnyAsync())
    {
        db.AppUsers.Add(new AppUser
        {
            Email = "craigermct@gmail.com",
            Role = "Admin"
        });
        await db.SaveChangesAsync();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Map Razor Pages BEFORE antiforgery so login/logout work correctly
app.MapRazorPages();

app.UseAntiforgery();

// Diagnostic: dump all PDF field names + test name-based access
app.MapGet("/debug/pdf-fields/{filename}", (string filename, IWebHostEnvironment env) =>
{
    var path = Path.Combine(env.ContentRootPath, "ReportCardTemplates", filename);
    if (!File.Exists(path)) return Results.NotFound($"File not found: {path}");
    using var doc = PdfSharp.Pdf.IO.PdfReader.Open(path, PdfSharp.Pdf.IO.PdfDocumentOpenMode.ReadOnly);
    var form = doc.AcroForm;
    if (form == null) return Results.Ok(new { error = "No AcroForm" });

    // Test name-based access for known text fields
    var testNames = new[] { "Student", "OEN", "Grade", "Teacher", "School", "Board", "Address", "Telephone", "Days Absent", "Times Late" };
    var nameTests = testNames.Select(name => {
        try {
            var f = form.Fields[name];
            return new { name, found = f != null, type = f?.GetType().Name ?? "(null)", error = (string?)null };
        } catch (Exception ex) {
            return new { name, found = false, type = "(error)", error = ex.Message };
        }
    }).ToList();

    // Raw index scan
    var byIndex = Enumerable.Range(0, form.Fields.Count)
        .Select(i => {
            try {
                var f = form.Fields[i];
                return new { index = i, name = f?.Name ?? "(null)", type = f?.GetType().Name ?? "(null)", error = (string?)null };
            } catch (Exception ex) {
                return new { index = i, name = "(error)", type = "(error)", error = ex.Message };
            }
        }).ToList();

    // Walk raw AcroForm /Fields array to find ALL fields including nested ones
    var rawFields = new List<object>();
    void WalkFields(PdfSharp.Pdf.PdfArray? arr, string prefix) {
        if (arr == null) return;
        for (int i = 0; i < arr.Elements.Count; i++) {
            try {
                var item = arr.Elements[i];
                var dict = (item as PdfSharp.Pdf.PdfDictionary)
                    ?? (item is PdfSharp.Pdf.PdfObject obj ? obj as PdfSharp.Pdf.PdfDictionary : null);
                if (dict == null) continue;
                var nameEl = dict.Elements["/T"];
                var name = nameEl?.ToString()?.Trim('(', ')') ?? "";
                var fullName = string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";
                var ftEl = dict.Elements["/FT"];
                var ft = ftEl?.ToString() ?? "";
                rawFields.Add(new { fullName, ft });
                // Recurse into kids
                var kids = dict.Elements["/Kids"] as PdfSharp.Pdf.PdfArray;
                WalkFields(kids, fullName);
            } catch (Exception ex) {
                rawFields.Add(new { fullName = $"(error at {i})", ft = ex.Message });
            }
        }
    }
    var topFields = form.Elements["/Fields"] as PdfSharp.Pdf.PdfArray;
    WalkFields(topFields, "");

    return Results.Ok(new { nameTests, rawFields });
});

app.MapGet("/health/db", async (IConfiguration config) =>
{
    var cs = config.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(cs)) return Results.Problem("Missing ConnectionStrings:DefaultConnection in App Service");
    await using var conn = new Microsoft.Data.SqlClient.SqlConnection(cs);
    await conn.OpenAsync();
    await using var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT 1", conn);
    var result = await cmd.ExecuteScalarAsync();
    return Results.Ok(new { db = "ok", result });
});


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

/// <summary>
/// Minimal font resolver for PdfSharp on Linux/Azure.
/// We only fill existing AcroForm fields so no new font rendering is needed.
/// </summary>
public class NullFontResolver : IFontResolver
{
    public byte[] GetFont(string faceName)
        => Array.Empty<byte>();

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        => new FontResolverInfo(familyName);
}