using System.IO.Compression;
using System.Threading.Channels;
using System.Threading.RateLimiting;
using Kerko.Analytics;
using Kerko.Http;
using Kerko.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

namespace Kerko.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKerkoMvc(this IServiceCollection services)
    {
        services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>())
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter()));

        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        services.Configure<BrotliCompressionProviderOptions>(options =>
            options.Level = CompressionLevel.Fastest);

        services.AddResponseCaching();

        return services;
    }

    public static IServiceCollection AddKerkoCors(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var envOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS");
                var origins = !string.IsNullOrEmpty(envOrigins)
                    ? envOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    : configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();

                if (origins == null || origins.Length == 0)
                {
                    if (environment.IsDevelopment())
                    {
                        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    }
                    else
                    {
                        throw new InvalidOperationException("CORS:AllowedOrigins must be configured for production");
                    }
                }
                else
                {
                    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
                }
            });
        });
        return services;
    }

    public static IServiceCollection AddKerkoData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("AnalyticsConnection")));

        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

        return services;
    }

    public static IServiceCollection AddKerkoAnalytics(this IServiceCollection services, IConfiguration configuration)
    {
        var capacity = configuration.GetValue<int>("Analytics:ChannelCapacity", 10_000);
        services.AddSingleton(_ => Channel.CreateBounded<RequestLog>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        }));

        services.AddHostedService<RequestLogWriter>();

        services.AddHttpClient("IpGeo", c =>
        {
            c.BaseAddress = new Uri("http://ip-api.com/");
            c.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddSingleton<IpGeolocationService>();

        return services;
    }

    public static IServiceCollection AddKerkoSearch(this IServiceCollection services)
    {
        services.AddScoped<ISearchService, SearchService>();
        return services;
    }

    public static IServiceCollection AddKerkoRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitConfig = configuration.GetSection("RateLimiting");
        var permitLimit = rateLimitConfig.GetValue<int>("PermitLimit", 20);
        var windowMinutes = rateLimitConfig.GetValue<int>("WindowMinutes", 1);

        services.AddRateLimiter(options =>
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

        return services;
    }
}
