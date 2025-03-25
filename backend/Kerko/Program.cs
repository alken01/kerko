using Microsoft.EntityFrameworkCore;
using Kerko.Infrastructure;
using Kerko.Services;
using Kerko.Models;
using System.Security.Cryptography.X509Certificates;

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
            serverOptions.ListenAnyIP(8080); // HTTP port
        }

        // Configure HTTPS endpoint
        if (kestrelConfig?.Endpoints?.Https != null)
        {
            serverOptions.ListenAnyIP(8443, listenOptions => // HTTPS port
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

                    using var cert = X509Certificate2.CreateFromPemFile(certPath, keyPath);
                    var certWithPrivateKey = new X509Certificate2(cert.Export(X509ContentType.Pfx));
                    
                    options.ServerCertificate = certWithPrivateKey;
                    options.SslProtocols = System.Security.Authentication.SslProtocols.Tls13; // Only allow TLS 1.3
                });
            });
        }
    });
}

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
        policy.WithOrigins(origins ?? Array.Empty<string>())
            .AllowAnyHeader()
            .AllowAnyMethod();
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
app.UseCors(); // Add CORS middleware
app.UseAuthorization();
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