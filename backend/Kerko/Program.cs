using Kerko.Admin;
using Kerko.Analytics;
using Kerko.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.FormatterName = "customFormatter");
builder.Logging.AddConsoleFormatter<Kerko.CustomConsoleFormatter, ConsoleFormatterOptions>();

var isTesting = builder.Environment.IsEnvironment("Testing");

if (!isTesting)
{
    var adminToken = builder.Configuration["KERKO_ADMIN_TOKEN"];
    if (string.IsNullOrEmpty(adminToken))
    {
        throw new InvalidOperationException("KERKO_ADMIN_TOKEN environment variable must be set.");
    }

    builder.Services.AddKerkoRateLimiting(builder.Configuration);
}

builder.Services
    .AddKerkoMvc()
    .AddKerkoCors(builder.Configuration, builder.Environment)
    .AddKerkoData(builder.Configuration)
    .AddKerkoAnalytics(builder.Configuration)
    .AddKerkoSearch();

var app = builder.Build();

// EnsureCreated won't alter existing tables, so detect and add the Location
// column on legacy DBs that pre-date it.
using (var scope = app.Services.CreateScope())
{
    var analyticsDb = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    analyticsDb.Database.EnsureCreated();

    var hasLocation = analyticsDb.Database
        .SqlQueryRaw<string>("SELECT name AS Value FROM pragma_table_info('RequestLogs') WHERE name = 'Location'")
        .AsEnumerable()
        .Any();
    if (!hasLocation)
    {
        analyticsDb.Database.ExecuteSqlRaw("ALTER TABLE RequestLogs ADD COLUMN Location TEXT");
    }
}

app.UseResponseCompression();
app.UseResponseCaching();
app.UseHttpsRedirection();
app.UseCors();
app.UseRouting();

app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/admin"),
    branch => branch.UseMiddleware<AdminAuthMiddleware>());

app.UseMiddleware<RequestLoggingMiddleware>();

if (!isTesting)
{
    app.UseRateLimiter();
}

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();

public partial class Program { }
