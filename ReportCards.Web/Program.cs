using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using ReportCards.Web.Data;
using ReportCards.Web.Components;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

    // Seed default admin user (your email)
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

// Google OAuth callback — maps Google identity to AppUser role
app.MapGet("/signin-google", async (HttpContext ctx, SchoolDbContext db) =>
{
    var result = await ctx.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    if (result?.Principal == null) return Results.Redirect("/login");

    var email = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    if (email == null) return Results.Redirect("/access-denied");

    var appUser = await db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
    if (appUser == null) return Results.Redirect("/access-denied");

    return Results.Redirect("/");
});

app.MapGet("/login", () => Results.Redirect("/auth/google-login"));

app.MapGet("/auth/google-login", async (HttpContext ctx) =>
{
    await ctx.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
        new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = "/auth/google-callback"
        });
});

app.MapGet("/auth/google-callback", async (HttpContext ctx, SchoolDbContext db) =>
{
    var result = await ctx.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    if (result?.Principal == null) return Results.Redirect("/access-denied");

    var email = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    if (email == null) return Results.Redirect("/access-denied");

    var appUser = await db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
    if (appUser == null) return Results.Redirect("/access-denied");

    // Re-sign in with role claim
    var claims = new List<System.Security.Claims.Claim>
    {
        new(System.Security.Claims.ClaimTypes.Email, email),
        new(System.Security.Claims.ClaimTypes.Role, appUser.Role),
        new(System.Security.Claims.ClaimTypes.Name, result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email)
    };

    var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new System.Security.Claims.ClaimsPrincipal(identity);

    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Results.Redirect("/");
});

app.MapGet("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();