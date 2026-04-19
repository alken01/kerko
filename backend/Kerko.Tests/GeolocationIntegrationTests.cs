using System.Net;
using System.Text.Json;
using Kerko.Analytics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Kerko.Tests;

[TestFixture]
public class GeolocationIntegrationTests
{
    private const string ValidToken = "test-token-geo";

    private string _analyticsTempDb = null!;
    private WebApplicationFactory<Program> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _analyticsTempDb = Path.Combine(Path.GetTempPath(), $"analytics_test_{Guid.NewGuid():N}.db");

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AnalyticsDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AnalyticsDbContext>(options =>
                        options.UseSqlite($"Data Source={_analyticsTempDb}"));

                });
                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:AnalyticsConnection"] = $"Data Source={_analyticsTempDb}",
                        ["KERKO_ADMIN_TOKEN"] = ValidToken
                    });
                });
            });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        db.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        _factory?.Dispose();
        if (File.Exists(_analyticsTempDb))
        {
            File.Delete(_analyticsTempDb);
        }
    }

    private HttpClient AuthedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Admin-Token", ValidToken);
        return client;
    }

    private async Task SeedLogsAsync(IEnumerable<RequestLog> rows)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        db.RequestLogs.AddRange(rows);
        await db.SaveChangesAsync();
    }

    private static RequestLog MakeLog(string ip, string endpoint = "kerko")
    {
        return new()
        {
            TimestampUtc = DateTime.UtcNow.AddMinutes(-1),
            Endpoint = endpoint,
            PageNumber = 1,
            PageSize = 10,
            ClientIp = ip,
            UserAgentRaw = "test",
            UserAgentSimplified = "Unknown/Unknown/Desktop",
            StatusCode = 200,
            DurationMs = 10,
            RequestId = Guid.NewGuid().ToString()
        };
    }

    // ─── IpGeolocationService direct tests ───────────────────────────────────

    [Test]
    public async Task GeoService_ResolvesIpv4MappedAddresses()
    {
        var service = _factory.Services.GetRequiredService<IpGeolocationService>();

        var logs = new List<RequestLog>
        {
            MakeLog("::ffff:3.71.121.233"),
            MakeLog("::ffff:3.71.123.63"),
            MakeLog("::ffff:3.127.74.190"),
            MakeLog("::ffff:3.127.74.204"),
            MakeLog("::ffff:3.127.74.214"),
        };

        await service.ResolveLocationsAsync(logs);

        foreach (var log in logs)
        {
            Assert.That(log.Location, Is.Not.Null.And.Not.Empty,
                $"Expected location for {log.ClientIp} but got null");
        }

        // These are AWS eu-central-1 IPs — should resolve to Germany/Frankfurt area
        TestContext.Out.WriteLine("Resolved locations:");
        foreach (var log in logs)
        {
            TestContext.Out.WriteLine($"  {log.ClientIp} -> {log.Location}");
        }
    }

    [Test]
    public async Task GeoService_SkipsPrivateIps()
    {
        var service = _factory.Services.GetRequiredService<IpGeolocationService>();

        var logs = new List<RequestLog>
        {
            MakeLog("192.168.1.1"),
            MakeLog("10.0.0.1"),
            MakeLog("127.0.0.1"),
            MakeLog("::1"),
        };

        await service.ResolveLocationsAsync(logs);

        foreach (var log in logs)
        {
            Assert.That(log.Location, Is.Null,
                $"Private IP {log.ClientIp} should not have a location");
        }
    }

    [Test]
    public async Task GeoService_CachesPreviousLookups()
    {
        var service = _factory.Services.GetRequiredService<IpGeolocationService>();

        var logs1 = new List<RequestLog> { MakeLog("::ffff:3.71.121.233") };
        await service.ResolveLocationsAsync(logs1);
        var firstLocation = logs1[0].Location;

        // Second call with same IP should return same result from cache
        var logs2 = new List<RequestLog> { MakeLog("::ffff:3.71.121.233") };
        await service.ResolveLocationsAsync(logs2);

        Assert.That(logs2[0].Location, Is.EqualTo(firstLocation));
    }

    // ─── Logs endpoint returns location field ────────────────────────────────

    [Test]
    public async Task AdminLogs_ReturnsLocationField()
    {
        // Seed a log with a pre-set location
        var log = MakeLog("::ffff:3.71.121.233");
        log.Location = "Frankfurt am Main, Germany";
        await SeedLogsAsync(new[] { log });

        var response = await AuthedClient().GetAsync("/api/admin/logs");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("items");
        Assert.That(items.GetArrayLength(), Is.EqualTo(1));

        var location = items[0].GetProperty("location").GetString();
        Assert.That(location, Is.EqualTo("Frankfurt am Main, Germany"));
    }
}
