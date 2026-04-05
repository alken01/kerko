using Microsoft.EntityFrameworkCore;
using Kerko.Infrastructure;
using Kerko.Services;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging.Console;
using Microsoft.AspNetCore.HttpOverrides;
using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = "customFormatter";
});
builder.Logging.AddConsoleFormatter<Kerko.CustomConsoleFormatter, ConsoleFormatterOptions>();

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<ISearchService, SearchService>();

// Response caching (required for VaryByQueryKeys)
builder.Services.AddResponseCaching();

// Health checks with DB ping
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Swagger UI in development (PRD 5.11)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseHttpsRedirection();
app.UseCors();

// Use rate limiter only in non-testing environments
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/api/health");

// Create database and tables
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Ensure the database directory exists
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            var dbPath = connectionString.Replace("Data Source=", "").Trim();
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        var context = services.GetRequiredService<ApplicationDbContext>();
        // Apply migrations (this will create the database if it doesn't exist)
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        // Don't throw here, let the application start even if database initialization fails
    }
}

app.Run();

// Make the implicit Program class accessible to tests
public partial class Program { }
