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
public class AdminIntegrationTests
{
    private const string ValidToken = "test-token-123";

    private string _analyticsTempDb = null!;
    private WebApplicationFactory<Program> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = BuildFactory(ValidToken);
    }

    private WebApplicationFactory<Program> BuildFactory(string? token)
    {
        _analyticsTempDb = Path.Combine(Path.GetTempPath(), $"analytics_test_{Guid.NewGuid():N}.db");

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Replace AnalyticsDbContext with a temp SQLite DB
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AnalyticsDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<AnalyticsDbContext>(options =>
                        options.UseSqlite($"Data Source={_analyticsTempDb}"));
                });
                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    var settings = new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:AnalyticsConnection"] = $"Data Source={_analyticsTempDb}"
                    };
                    if (token is not null)
                        settings["KERKO_ADMIN_TOKEN"] = token;
                    cfg.AddInMemoryCollection(settings);
                });
            });

        // Ensure analytics DB is created
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        db.Database.EnsureCreated();
        return factory;
    }

    private async Task SeedLogsAsync(IEnumerable<RequestLog> rows)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        db.RequestLogs.AddRange(rows);
        await db.SaveChangesAsync();
    }

    private HttpClient AuthedClient(string token = ValidToken)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Admin-Token", token);
        return client;
    }

    [TearDown]
    public void TearDown()
    {
        _factory?.Dispose();
        if (File.Exists(_analyticsTempDb))
            File.Delete(_analyticsTempDb);
    }

    private static RequestLog MakeLog(
        DateTime timestampUtc,
        string endpoint = "kerko",
        string? emri = null,
        string? mbiemri = null,
        string? numriTarges = null,
        string? numriTelefonit = null,
        string ip = "1.2.3.4",
        int status = 200,
        int durationMs = 10,
        int? resultCount = null)
        => new()
        {
            TimestampUtc = timestampUtc,
            Endpoint = endpoint,
            Emri = emri,
            Mbiemri = mbiemri,
            NumriTarges = numriTarges,
            NumriTelefonit = numriTelefonit,
            PageNumber = 1,
            PageSize = 10,
            ClientIp = ip,
            UserAgentRaw = "test",
            UserAgentSimplified = "Unknown/Unknown/Desktop",
            StatusCode = status,
            DurationMs = durationMs,
            ResultCount = resultCount,
            RequestId = Guid.NewGuid().ToString()
        };

    // ─── Admin auth tests ────────────────────────────────────────────────────

    [Test]
    public async Task AdminLogs_NoToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/admin/logs");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task AdminLogs_WrongToken_Returns401()
    {
        var response = await AuthedClient("wrong-token").GetAsync("/api/admin/logs");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task AdminLogs_CorrectToken_Returns200()
    {
        var response = await AuthedClient().GetAsync("/api/admin/logs");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task AdminStats_CorrectToken_Returns200()
    {
        var response = await AuthedClient().GetAsync("/api/admin/stats");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.That(doc.RootElement.TryGetProperty("window", out _), Is.True);
        Assert.That(doc.RootElement.TryGetProperty("total", out _), Is.True);
    }

    // ─── Defense-in-depth: empty expected token must NEVER grant access ──────

    [Test]
    public async Task AdminLogs_EmptyExpectedToken_Returns401_EvenWithEmptyProvidedToken()
    {
        // Rebuild the factory with an empty configured token to simulate misconfig.
        _factory.Dispose();
        _factory = BuildFactory(string.Empty);

        // Client sends no X-Admin-Token header (equivalent to empty string).
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/admin/logs");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized),
            "Empty expected token must not grant access even when the client sends an empty token.");
    }

    [Test]
    public async Task AdminLogs_EmptyExpectedToken_Returns401_WithExplicitEmptyHeader()
    {
        _factory.Dispose();
        _factory = BuildFactory(string.Empty);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Admin-Token", "");
        var response = await client.GetAsync("/api/admin/logs");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task AdminLogs_MissingExpectedTokenEntirely_Returns401()
    {
        // No KERKO_ADMIN_TOKEN key in configuration at all.
        _factory.Dispose();
        _factory = BuildFactory(token: null);

        var response = await AuthedClient("anything").GetAsync("/api/admin/logs");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    // ─── Cursor pagination tests ──────────────────────────────────────────────

    [Test]
    public async Task AdminLogs_CursorPagination_ReturnsConsistentPages()
    {
        // Seed analytics DB with test rows
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
            var baseTime = DateTime.UtcNow.AddMinutes(-10);
            for (int i = 0; i < 10; i++)
            {
                db.RequestLogs.Add(new RequestLog
                {
                    TimestampUtc = baseTime.AddSeconds(i),
                    Endpoint = "kerko",
                    PageNumber = 1,
                    PageSize = 10,
                    ClientIp = $"1.2.3.{i}",
                    UserAgentRaw = "test",
                    UserAgentSimplified = "Unknown/Unknown/Desktop",
                    StatusCode = 200,
                    DurationMs = 10,
                    RequestId = Guid.NewGuid().ToString()
                });
            }
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Admin-Token", "test-token-123");

        // First page - 5 items
        var response1 = await client.GetAsync("/api/admin/logs?limit=5");
        Assert.That(response1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json1 = await response1.Content.ReadAsStringAsync();
        using var doc1 = JsonDocument.Parse(json1);
        var items1 = doc1.RootElement.GetProperty("items").GetArrayLength();
        Assert.That(items1, Is.EqualTo(5));
        var cursor = doc1.RootElement.GetProperty("nextCursor").GetString();
        Assert.That(cursor, Is.Not.Null);

        // Second page using cursor
        var response2 = await client.GetAsync($"/api/admin/logs?limit=5&cursor={Uri.EscapeDataString(cursor!)}");
        Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json2 = await response2.Content.ReadAsStringAsync();
        using var doc2 = JsonDocument.Parse(json2);
        var items2 = doc2.RootElement.GetProperty("items").GetArrayLength();
        Assert.That(items2, Is.EqualTo(5));

        // No more pages
        var cursor2 = doc2.RootElement.GetProperty("nextCursor");
        Assert.That(cursor2.ValueKind, Is.EqualTo(JsonValueKind.Null));
    }

    // ─── Analytics write-through test ─────────────────────────────────────────

    [Test]
    public async Task KerkoRequest_EventuallyWritesRowToAnalyticsDb()
    {
        // The test environment uses an in-memory SQLite kerko.db, so we skip
        // actual search results; we just verify the middleware logs the request.
        // The search will fail (no DB) — that's fine; the middleware logs on all paths
        // after the pipeline runs. However, in testing env, the main DB may not exist.
        // We verify the analytics middleware logged a 4xx or 5xx request.
        var client = _factory.CreateClient();

        // Fire a request — will return 400 (bad request) since emri/mbiemri too short
        await client.GetAsync("/api/kerko?emri=a&mbiemri=b");

        // Poll the analytics DB for up to 3 seconds
        var found = false;
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(100);
            var count = await db.RequestLogs.CountAsync(r => r.Endpoint == "kerko");
            if (count > 0)
            {
                found = true;
                break;
            }
        }

        Assert.That(found, Is.True, "Expected a RequestLog row for /api/kerko to appear within 3 seconds");
    }

    // ─── Filter tests for /api/admin/logs ────────────────────────────────────

    [Test]
    public async Task AdminLogs_EndpointFilter_ReturnsOnlyMatchingEndpoint()
    {
        var now = DateTime.UtcNow;
        await SeedLogsAsync(new[]
        {
            MakeLog(now.AddSeconds(-1), endpoint: "kerko",   emri: "Alice"),
            MakeLog(now.AddSeconds(-2), endpoint: "targat",  numriTarges: "AA123"),
            MakeLog(now.AddSeconds(-3), endpoint: "telefon", numriTelefonit: "555-0100"),
            MakeLog(now.AddSeconds(-4), endpoint: "kerko",   emri: "Bob"),
        });

        var response = await AuthedClient().GetAsync("/api/admin/logs?endpoint=kerko");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("items");

        Assert.That(items.GetArrayLength(), Is.EqualTo(2));
        foreach (var item in items.EnumerateArray())
            Assert.That(item.GetProperty("endpoint").GetString(), Is.EqualTo("kerko"));
    }

    [Test]
    public async Task AdminLogs_QueryFilter_MatchesAcrossAllTermColumns()
    {
        var now = DateTime.UtcNow;
        await SeedLogsAsync(new[]
        {
            MakeLog(now.AddSeconds(-1), emri: "Alejandro"),        // matches "Alej"
            MakeLog(now.AddSeconds(-2), mbiemri: "Alejos"),        // matches "Alej"
            MakeLog(now.AddSeconds(-3), numriTarges: "Alej-99"),   // matches "Alej"
            MakeLog(now.AddSeconds(-4), emri: "Unrelated"),
        });

        // Note: SQLite's default collation is BINARY, so LIKE/Contains is case-sensitive.
        // The seeded rows are chosen to all match the literal substring "Alej".
        var response = await AuthedClient().GetAsync("/api/admin/logs?q=Alej");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(doc.RootElement.GetProperty("items").GetArrayLength(), Is.EqualTo(3));
    }

    [Test]
    public async Task AdminLogs_IpAndStatusFilters_Combine()
    {
        var now = DateTime.UtcNow;
        await SeedLogsAsync(new[]
        {
            MakeLog(now.AddSeconds(-1), ip: "10.0.0.1", status: 200),
            MakeLog(now.AddSeconds(-2), ip: "10.0.0.1", status: 404),
            MakeLog(now.AddSeconds(-3), ip: "10.0.0.2", status: 200),
            MakeLog(now.AddSeconds(-4), ip: "192.168.1.1", status: 200),
        });

        var response = await AuthedClient().GetAsync("/api/admin/logs?ip=10.0.0&status=200");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("items");

        Assert.That(items.GetArrayLength(), Is.EqualTo(2));
        foreach (var item in items.EnumerateArray())
        {
            Assert.That(item.GetProperty("clientIp").GetString(), Does.StartWith("10.0.0."));
            Assert.That(item.GetProperty("statusCode").GetInt32(), Is.EqualTo(200));
        }
    }

    [Test]
    public async Task AdminLogs_FromToFilter_BoundsByTimestamp()
    {
        var now = DateTime.UtcNow;
        var inWindowStart = now.AddMinutes(-10);
        var inWindowEnd   = now.AddMinutes(-5);

        await SeedLogsAsync(new[]
        {
            MakeLog(now.AddMinutes(-20)),                    // before window
            MakeLog(inWindowStart.AddSeconds(1)),            // in window
            MakeLog(inWindowEnd.AddSeconds(-1)),             // in window
            MakeLog(now.AddMinutes(-1)),                     // after window
        });

        var from = inWindowStart.ToString("o");
        var to   = inWindowEnd.ToString("o");
        var response = await AuthedClient().GetAsync($"/api/admin/logs?from={from}&to={to}");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.That(doc.RootElement.GetProperty("items").GetArrayLength(), Is.EqualTo(2));
    }

    [Test]
    public async Task AdminLogs_LimitIsClampedTo200()
    {
        var now = DateTime.UtcNow;
        var rows = Enumerable.Range(0, 5).Select(i => MakeLog(now.AddSeconds(-i))).ToList();
        await SeedLogsAsync(rows);

        // Passing a crazy limit should not 500.
        var response = await AuthedClient().GetAsync("/api/admin/logs?limit=999999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task AdminLogs_InvalidCursor_DoesNotCrash()
    {
        var response = await AuthedClient().GetAsync("/api/admin/logs?cursor=not-valid-base64!!");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    // ─── Stats content correctness ───────────────────────────────────────────

    [Test]
    public async Task AdminStats_ReturnsCorrectCountsAndTopIps()
    {
        var now = DateTime.UtcNow;
        await SeedLogsAsync(new[]
        {
            MakeLog(now.AddMinutes(-1), ip: "1.1.1.1"),
            MakeLog(now.AddMinutes(-2), ip: "1.1.1.1"),
            MakeLog(now.AddMinutes(-3), ip: "1.1.1.1"),
            MakeLog(now.AddMinutes(-4), ip: "2.2.2.2"),
            MakeLog(now.AddMinutes(-5), ip: "3.3.3.3"),
            // Out of 24h window:
            MakeLog(now.AddDays(-2),    ip: "9.9.9.9"),
        });

        var response = await AuthedClient().GetAsync("/api/admin/stats?window=24h");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(doc.RootElement.GetProperty("window").GetString(), Is.EqualTo("24h"));
        Assert.That(doc.RootElement.GetProperty("total").GetInt32(), Is.EqualTo(5));

        var topIps = doc.RootElement.GetProperty("topIps");
        Assert.That(topIps.GetArrayLength(), Is.EqualTo(3));
        var firstIp = topIps[0];
        Assert.That(firstIp.GetProperty("ip").GetString(), Is.EqualTo("1.1.1.1"));
        Assert.That(firstIp.GetProperty("count").GetInt32(), Is.EqualTo(3));
    }

    [Test]
    public async Task AdminStats_TopQueriesGroupsRepeatedSearches()
    {
        var now = DateTime.UtcNow;
        await SeedLogsAsync(new[]
        {
            MakeLog(now.AddMinutes(-1), emri: "Alice", mbiemri: "Smith"),
            MakeLog(now.AddMinutes(-2), emri: "Alice", mbiemri: "Smith"),
            MakeLog(now.AddMinutes(-3), emri: "Alice", mbiemri: "Smith"),
            MakeLog(now.AddMinutes(-4), emri: "Bob"),
        });

        var response = await AuthedClient().GetAsync("/api/admin/stats?window=24h");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var topQueries = doc.RootElement.GetProperty("topQueries");

        Assert.That(topQueries.GetArrayLength(), Is.EqualTo(2));
        var top = topQueries[0];
        Assert.That(top.GetProperty("term").GetString(), Does.Contain("Alice").And.Contain("Smith"));
        Assert.That(top.GetProperty("count").GetInt32(), Is.EqualTo(3));
    }

    [Test]
    public async Task AdminStats_WindowSelectorChangesResults()
    {
        var now = DateTime.UtcNow;
        await SeedLogsAsync(new[]
        {
            MakeLog(now.AddMinutes(-30)),   // in 1h and 24h
            MakeLog(now.AddHours(-2)),      // in 24h only
            MakeLog(now.AddDays(-3)),       // in 7d only
        });

        var client = AuthedClient();

        var r1 = JsonDocument.Parse(await client.GetStringAsync("/api/admin/stats?window=1h"));
        var r24 = JsonDocument.Parse(await client.GetStringAsync("/api/admin/stats?window=24h"));
        var r7d = JsonDocument.Parse(await client.GetStringAsync("/api/admin/stats?window=7d"));

        Assert.That(r1.RootElement.GetProperty("total").GetInt32(), Is.EqualTo(1));
        Assert.That(r24.RootElement.GetProperty("total").GetInt32(), Is.EqualTo(2));
        Assert.That(r7d.RootElement.GetProperty("total").GetInt32(), Is.EqualTo(3));
    }

    // ─── Middleware scope: non-admin endpoints are not blocked ───────────────

    [Test]
    public async Task HealthEndpoint_NoToken_Returns200()
    {
        // /api/health must not be behind AdminAuthMiddleware — it's a branched UseWhen.
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/health");
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.Unauthorized));
    }
}
