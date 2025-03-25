using Microsoft.EntityFrameworkCore;
using Kerko.Infrastructure;
using Kerko.Services;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel with HTTPS
if (!builder.Environment.IsDevelopment()) // Only use custom certificate in production
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(80); // Standard HTTP port
        serverOptions.ListenAnyIP(443, listenOptions => // Standard HTTPS port
        {
            listenOptions.UseHttps(options =>
            {
                // Load certificate and private key
                var certPath = Path.Combine(builder.Environment.ContentRootPath, "certificates", "certificate.crt");
                var keyPath = Path.Combine(builder.Environment.ContentRootPath, "certificates", "private.key");
                
                // Read certificate and private key content
                var certContent = File.ReadAllText(certPath);
                var keyContent = File.ReadAllText(keyPath);
                
                // Combine certificate and private key into a PFX
                using var cert = X509Certificate2.CreateFromPemFile(certPath, keyPath);
                
                // Create a copy with exportable private key
                var certWithPrivateKey = new X509Certificate2(cert.Export(X509ContentType.Pfx));
                
                options.ServerCertificate = certWithPrivateKey;
                options.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
            });
        });
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
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();