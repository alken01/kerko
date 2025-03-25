using Microsoft.EntityFrameworkCore;
using Kerko.Infrastructure;
using Kerko.Services;
using Kerko.Models;
using System.Security.Cryptography.X509Certificates;
using Kerko.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel with HTTPS
if (!builder.Environment.IsDevelopment()) // Only use custom certificate in production
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        var kestrelConfig = builder.Configuration.GetSection("Kestrel").Get<KestrelConfig>();

        // Configure HTTP endpoint
        if (kestrelConfig?.Endpoints?.Http != null)
        {
            var httpPort = kestrelConfig.Endpoints.Http.Url.Split(':')[2];
            serverOptions.ListenAnyIP(int.Parse(httpPort));
        }

        // Configure HTTPS endpoint
        if (kestrelConfig?.Endpoints?.Https != null)
        {
            var httpsPort = kestrelConfig.Endpoints.Https.Url.Split(':')[2];
            serverOptions.ListenAnyIP(int.Parse(httpsPort), listenOptions =>
            {
                listenOptions.UseHttps(options =>
                {
                    var certConfig = kestrelConfig.Endpoints.Https.Certificate;
                    var certPath = Path.Combine(builder.Environment.ContentRootPath, certConfig.Path);
                    var keyPath = Path.Combine(builder.Environment.ContentRootPath, certConfig.KeyPath);

                    if (!File.Exists(certPath) || !File.Exists(keyPath))
                    {
                        throw new FileNotFoundException("SSL certificate or private key not found");
                    }

                    var certWithPrivateKey = X509Certificate2.CreateFromPemFile(certPath, keyPath);
                    options.ServerCertificate = certWithPrivateKey;
                    options.SslProtocols = System.Security.Authentication.SslProtocols.Tls13;
                });
            });
        }
    });
}

// Add rate limiting
// Add rate limiting service
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 20,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
        if (origins == null || origins.Length == 0)
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .WithMethods("GET");
        }
        else
        {
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .WithMethods("GET");
        }
    });
});

// Configure SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<ISearchService, SearchService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter(); // Add rate limiting middleware
app.UseAuthorization();
app.UseApiKeyAuth();
app.MapControllers();

// Create database and tables
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Ensure database exists
        context.Database.EnsureCreated();
        // Apply migrations
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