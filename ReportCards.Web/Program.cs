using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using MudBlazor.Services;
using ReportCards.Web.Data;
using ReportCards.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// Add Razor Pages for login/logout handling
builder.Services.AddRazorPages();

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