using Microsoft.EntityFrameworkCore;
using Kerko.Infrastructure;
using Kerko.Services;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging.Console;
using Microsoft.AspNetCore.HttpOverrides;
using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using System.Threading.Channels;
using Kerko.Analytics;
using Kerko.Admin;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = "customFormatter";
});
builder.Logging.AddConsoleFormatter<Kerko.CustomConsoleFormatter, ConsoleFormatterOptions>();

// Fail-loud: KERKO_ADMIN_TOKEN must be set in non-testing environments
if (!builder.Environment.IsEnvironment("Testing"))
{
    var adminToken = builder.Configuration["KERKO_ADMIN_TOKEN"];
    if (string.IsNullOrEmpty(adminToken))
    {
        throw new InvalidOperationException("KERKO_ADMIN_TOKEN environment variable must be set.");
    }
}

// Add rate limiting (skip in testing environment)
if (!builder.Environment.IsEnvironment("Testing"))
{
    var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
    var permitLimit = rateLimitConfig.GetValue<int>("PermitLimit", 20);
    var windowMinutes = rateLimitConfig.GetValue<int>("WindowMinutes", 1);

    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? context.Request.Headers.Host.ToString(),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = permitLimit,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(windowMinutes)
                }));

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
}

// Add services to the container.
builder.Services.AddControllers();

// Response compression (PRD 2.3)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Add CORS with env var override (PRD 3.3)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // Check env var first, then fall back to config
        var envOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS");
        var origins = !string.IsNullOrEmpty(envOrigins)
            ? envOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();

        if (origins == null || origins.Length == 0)
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
            else
            {
                throw new InvalidOperationException("CORS:AllowedOrigins must be configured for production");
            }
        }
        else
        {
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Configure forwarded headers so RemoteIpAddress reflects the real client IP behind reverse proxy/Docker
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure SQLite (main kerko.db — ReadOnly)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure analytics SQLite (analytics.db — read-write)
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AnalyticsConnection")));

// Register the bounded channel for analytics logs (singleton)
builder.Services.AddSingleton(_ => Channel.CreateBounded<RequestLog>(new BoundedChannelOptions(10_000)
{
    FullMode = BoundedChannelFullMode.DropWrite,
    SingleReader = true,
    SingleWriter = false
}));

// Register analytics writer hosted service
builder.Services.AddHostedService<RequestLogWriter>();

// Register IP geolocation service
builder.Services.AddHttpClient("IpGeo", c =>
{
    c.BaseAddress = new Uri("http://ip-api.com/");
    c.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddSingleton<IpGeolocationService>();

// Register services
builder.Services.AddScoped<ISearchService, SearchService>();

// Response caching (required for VaryByQueryKeys)
builder.Services.AddResponseCaching();

// Health checks with DB ping
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Auto-create analytics.db if it doesn't exist (empty DB, no data to migrate)
using (var scope = app.Services.CreateScope())
{
    var analyticsDb = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    analyticsDb.Database.EnsureCreated();

    // Add Location column for existing DBs (EnsureCreated won't alter existing tables)
    try { analyticsDb.Database.ExecuteSqlRaw("ALTER TABLE RequestLogs ADD COLUMN Location TEXT"); }
    catch (Microsoft.Data.Sqlite.SqliteException) { /* column already exists */ }
}

app.UseForwardedHeaders();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseHttpsRedirection();
app.UseCors();
app.UseRouting();

// Admin auth middleware — only runs on /api/admin paths
app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/admin"), branch =>
{
    branch.UseMiddleware<AdminAuthMiddleware>();
});

// Request logging middleware — logs /api/kerko, /api/targat, /api/telefon
app.UseMiddleware<RequestLoggingMiddleware>();

// Use rate limiter only in non-testing environments
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();

// Make the implicit Program class accessible to tests
public partial class Program { }
