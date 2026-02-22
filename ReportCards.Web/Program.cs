using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using ReportCards.Web.Data;
using ReportCards.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

    if (!await db.Teachers.AnyAsync())
    {
        var teacher = new Teacher { DisplayName = "Craig (Teacher)", Email = "teacher1@example.com" };
        db.Teachers.Add(teacher);
        await db.SaveChangesAsync();

        db.Classes.Add(new Class { Name = "Grade 3", SchoolYear = "2025-2026", TeacherId = teacher.Id });
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

app.MapGet("/", (HttpContext ctx) =>
{
    if (ctx.User.Identity?.IsAuthenticated != true)
        return Results.Redirect("/login");
    return Results.Redirect("/home");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();