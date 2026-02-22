using Microsoft.EntityFrameworkCore;
using ReportCards.Web.Data;
using ReportCards.Web.Components;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

var app = builder.Build();

// ===============================
// Seed minimal data (dev-friendly)
// ===============================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();

    // Ensure DB exists / migrations are applied (optional in dev; safe)
    await db.Database.MigrateAsync();

    // Seed a default teacher + class if DB is empty
    if (!await db.Teachers.AnyAsync())
    {
        var teacher = new Teacher
        {
            DisplayName = "Craig (Teacher)",
            Email = "teacher1@example.com"
        };

        db.Teachers.Add(teacher);
        await db.SaveChangesAsync();

        db.Classes.Add(new Class
        {
            Name = "Grade 3",
            SchoolYear = "2025-2026",
            TeacherId = teacher.Id
        });

        await db.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/health/db", async (IConfiguration config) =>
{
    var cs = config.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(cs))
        return Results.Problem("Missing ConnectionStrings:DefaultConnection in App Service");

    await using var conn = new Microsoft.Data.SqlClient.SqlConnection(cs);
    await conn.OpenAsync();

    await using var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT 1", conn);
    var result = await cmd.ExecuteScalarAsync();

    return Results.Ok(new { db = "ok", result });
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
