using System.IO.Compression;
using System.Threading.Channels;
using System.Threading.RateLimiting;
using Kerko.Admin;
using Kerko.Analytics;
using Kerko.Http;
using Kerko.Infrastructure;
using Kerko.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = "customFormatter";
});
builder.Logging.AddConsoleFormatter<Kerko.CustomConsoleFormatter, ConsoleFormatterOptions>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    var adminToken = builder.Configuration["KERKO_ADMIN_TOKEN"];
    if (string.IsNullOrEmpty(adminToken))
    {
        throw new InvalidOperationException("KERKO_ADMIN_TOKEN environment variable must be set.");
    }
}

if (!builder.Environment.IsEnvironment("Testing"))
{
    var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
    var permitLimit = rateLimitConfig.GetValue<int>("PermitLimit", 20);
    var windowMinutes = rateLimitConfig.GetValue<int>("WindowMinutes", 1);

    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ClientInfo.GetClientIpAddress(context.Request),
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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new Kerko.Infrastructure.UtcDateTimeJsonConverter());
    });

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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AnalyticsConnection")));

builder.Services.AddSingleton(_ => Channel.CreateBounded<RequestLog>(new BoundedChannelOptions(10_000)
{
    FullMode = BoundedChannelFullMode.DropWrite,
    SingleReader = true,
    SingleWriter = false
}));

builder.Services.AddHostedService<RequestLogWriter>();

builder.Services.AddHttpClient("IpGeo", c =>
{
    c.BaseAddress = new Uri("http://ip-api.com/");
    c.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddSingleton<IpGeolocationService>();

builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.AddResponseCaching();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// EnsureCreated won't alter existing tables, so add Location column manually
using (var scope = app.Services.CreateScope())
{
    var analyticsDb = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    analyticsDb.Database.EnsureCreated();

    try
    { analyticsDb.Database.ExecuteSqlRaw("ALTER TABLE RequestLogs ADD COLUMN Location TEXT"); }
    catch (Microsoft.Data.Sqlite.SqliteException) { /* column already exists */ }
}

app.UseResponseCompression();
app.UseResponseCaching();
app.UseHttpsRedirection();
app.UseCors();
app.UseRouting();

app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api/admin"), branch =>
{
    branch.UseMiddleware<AdminAuthMiddleware>();
});

app.UseMiddleware<RequestLoggingMiddleware>();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();

public partial class Program { }
