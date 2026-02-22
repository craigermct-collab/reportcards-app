using ReportCards.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

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
