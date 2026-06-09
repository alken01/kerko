using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using Kerko.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kerko.Analytics;

public class IpGeolocationService(
    IHttpClientFactory httpClientFactory,
    IServiceProvider serviceProvider,
    ILogger<IpGeolocationService> logger)
{
    private readonly ConcurrentDictionary<string, string?> _cache = new();

    private const string MappedV4Prefix = "::ffff:";

    // Successful lookups persist across restarts; re-resolve after this long.
    public static readonly TimeSpan CacheTtl = TimeSpan.FromDays(30);

    public async Task ResolveLocationsAsync(List<RequestLog> logs)
    {
        var uncachedIps = UncachedPublicIps(logs.Select(l => l.ClientIp));
        if (uncachedIps.Count > 0)
        {
            await HydrateFromDbAsync(uncachedIps);
            var stillUncached = UncachedPublicIps(uncachedIps);
            if (stillUncached.Count > 0)
            {
                await FetchBatchAsync(stillUncached);
            }
        }

        foreach (var log in logs)
        {
            if (_cache.TryGetValue(log.ClientIp, out var location))
            {
                log.Location = location;
            }
        }
    }

    /// <summary>
    /// Resolves a list of IPs to locations. Returns a dictionary of original IP -> location.
    /// Batches into groups of 100 (ip-api.com limit) with delays to respect rate limits.
    /// </summary>
    public async Task<Dictionary<string, string?>> ResolveBatchAsync(List<string> ips)
    {
        var uncached = UncachedPublicIps(ips);
        if (uncached.Count > 0)
        {
            await HydrateFromDbAsync(uncached);
        }

        var toResolve = UncachedPublicIps(ips);

        var chunks = toResolve.Chunk(100).ToList();
        for (var i = 0; i < chunks.Count; i++)
        {
            await FetchBatchAsync(chunks[i].ToList());
            if (i < chunks.Count - 1)
            {
                await Task.Delay(1500);
            }
        }

        var result = new Dictionary<string, string?>();
        foreach (var ip in ips)
        {
            if (_cache.TryGetValue(ip, out var location))
            {
                result[ip] = location;
            }
        }
        return result;
    }

    private List<string> UncachedPublicIps(IEnumerable<string> ips)
    {
        return ips
            .Where(ip => !IsPrivateIp(ip) && !_cache.ContainsKey(ip))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Loads previously resolved locations from the analytics DB into the in-memory
    /// cache so a restart doesn't re-query ip-api.com for every known IP.
    /// DB problems are non-fatal — resolution falls through to the HTTP lookup.
    /// </summary>
    private async Task HydrateFromDbAsync(List<string> ips)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
            var cutoff = DateTime.UtcNow - CacheTtl;
            var rows = await db.IpLocations
                .AsNoTracking()
                .Where(c => ips.Contains(c.Ip) && c.CachedAtUtc >= cutoff)
                .ToListAsync();
            foreach (var row in rows)
            {
                _cache.TryAdd(row.Ip, row.Location);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read IP location cache from analytics DB");
        }
    }

    private async Task PersistToDbAsync(Dictionary<string, string?> resolved)
    {
        var successful = resolved.Where(kv => kv.Value != null).ToList();
        if (successful.Count == 0)
        {
            return;
        }

        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
            var keys = successful.Select(kv => kv.Key).ToList();
            var existing = await db.IpLocations.Where(c => keys.Contains(c.Ip)).ToDictionaryAsync(c => c.Ip);
            var now = DateTime.UtcNow;
            foreach (var (ip, location) in successful)
            {
                if (existing.TryGetValue(ip, out var row))
                {
                    row.Location = location;
                    row.CachedAtUtc = now;
                }
                else
                {
                    db.IpLocations.Add(new IpLocationCache { Ip = ip, Location = location, CachedAtUtc = now });
                }
            }
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist IP location cache to analytics DB");
        }
    }

    private async Task FetchBatchAsync(List<string> originalIps)
    {
        // Map original IP -> normalized IP for the API call
        var normalizedMap = originalIps.ToDictionary(ip => ip, NormalizeIp);
        var lookupIps = normalizedMap.Values.Distinct().ToList();

        try
        {
            var client = httpClientFactory.CreateClient("IpGeo");
            var response = await client.PostAsJsonAsync("batch?fields=status,query,country,city", lookupIps);

            if (!response.IsSuccessStatusCode)
            {
                KerkoMetrics.GeolocationFailures.Add(1);
                logger.LogWarning("ip-api.com batch returned {Status}", response.StatusCode);
                foreach (var ip in originalIps)
                {
                    _cache.TryAdd(ip, null);
                }

                return;
            }

            var results = await response.Content.ReadFromJsonAsync<List<IpApiResult>>();
            if (results == null)
            {
                foreach (var ip in originalIps)
                {
                    _cache.TryAdd(ip, null);
                }

                return;
            }

            // Build normalized IP -> location map from results
            var locationByNormalized = new Dictionary<string, string?>();
            foreach (var r in results)
            {
                if (r.Query == null)
                {
                    continue;
                }

                if (r.Status == "success")
                {
                    var loc = !string.IsNullOrEmpty(r.City)
                        ? $"{r.City}, {r.Country}"
                        : r.Country;
                    locationByNormalized[r.Query] = loc;
                }
                else
                {
                    locationByNormalized[r.Query] = null;
                }
            }

            // Cache using the original IP keys
            var resolved = new Dictionary<string, string?>();
            foreach (var (original, normalized) in normalizedMap)
            {
                var loc = locationByNormalized.GetValueOrDefault(normalized);
                _cache.TryAdd(original, loc);
                resolved[original] = loc;
            }

            await PersistToDbAsync(resolved);
        }
        catch (Exception ex)
        {
            KerkoMetrics.GeolocationFailures.Add(1);
            logger.LogWarning(ex, "Failed to fetch IP geolocation for {Count} IPs", originalIps.Count);
            foreach (var ip in originalIps)
            {
                _cache.TryAdd(ip, null);
            }
        }
    }

    private static string NormalizeIp(string ip)
    {
        if (ip.StartsWith(MappedV4Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return ip[MappedV4Prefix.Length..];
        }

        return ip;
    }

    private static bool IsPrivateIp(string ip)
    {
        if (string.IsNullOrEmpty(ip))
        {
            return true;
        }

        if (!IPAddress.TryParse(ip, out var addr))
        {
            return true;
        }

        if (IPAddress.IsLoopback(addr))
        {
            return true;
        }

        if (addr.IsIPv4MappedToIPv6)
        {
            addr = addr.MapToIPv4();
        }

        if (addr.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = addr.GetAddressBytes();
            return bytes[0] == 10 ||
                   (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) ||
                   (bytes[0] == 192 && bytes[1] == 168);
        }

        return addr.IsIPv6LinkLocal || addr.IsIPv6SiteLocal;
    }

    private class IpApiResult
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("query")]
        public string? Query { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }
    }
}
