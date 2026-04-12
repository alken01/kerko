using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json.Serialization;

namespace Kerko.Analytics;

public class IpGeolocationService(IHttpClientFactory httpClientFactory, ILogger<IpGeolocationService> logger)
{
    private readonly ConcurrentDictionary<string, string?> _cache = new();

    private const string MappedV4Prefix = "::ffff:";

    public async Task ResolveLocationsAsync(List<RequestLog> logs)
    {
        var uncachedIps = logs
            .Select(l => l.ClientIp)
            .Where(ip => !IsPrivateIp(ip) && !_cache.ContainsKey(ip))
            .Distinct()
            .ToList();

        if (uncachedIps.Count > 0)
            await FetchBatchAsync(uncachedIps);

        foreach (var log in logs)
        {
            if (_cache.TryGetValue(log.ClientIp, out var location))
                log.Location = location;
        }
    }

    /// <summary>
    /// Resolves a list of IPs to locations. Returns a dictionary of original IP -> location.
    /// Batches into groups of 100 (ip-api.com limit) with delays to respect rate limits.
    /// </summary>
    public async Task<Dictionary<string, string?>> ResolveBatchAsync(List<string> ips)
    {
        var toResolve = ips
            .Where(ip => !IsPrivateIp(ip) && !_cache.ContainsKey(ip))
            .Distinct()
            .ToList();

        var chunks = toResolve.Chunk(100).ToList();
        for (var i = 0; i < chunks.Count; i++)
        {
            await FetchBatchAsync(chunks[i].ToList());
            if (i < chunks.Count - 1)
                await Task.Delay(1500);
        }

        var result = new Dictionary<string, string?>();
        foreach (var ip in ips)
        {
            if (_cache.TryGetValue(ip, out var location))
                result[ip] = location;
        }
        return result;
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
                logger.LogDebug("ip-api.com batch returned {Status}", response.StatusCode);
                foreach (var ip in originalIps) _cache.TryAdd(ip, null);
                return;
            }

            var results = await response.Content.ReadFromJsonAsync<List<IpApiResult>>();
            if (results == null)
            {
                foreach (var ip in originalIps) _cache.TryAdd(ip, null);
                return;
            }

            // Build normalized IP -> location map from results
            var locationByNormalized = new Dictionary<string, string?>();
            foreach (var r in results)
            {
                if (r.Query == null) continue;

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
            foreach (var (original, normalized) in normalizedMap)
            {
                var loc = locationByNormalized.GetValueOrDefault(normalized);
                _cache.TryAdd(original, loc);
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to fetch IP geolocation for {Count} IPs", originalIps.Count);
            foreach (var ip in originalIps) _cache.TryAdd(ip, null);
        }
    }

    private static string NormalizeIp(string ip)
    {
        if (ip.StartsWith(MappedV4Prefix, StringComparison.OrdinalIgnoreCase))
            return ip[MappedV4Prefix.Length..];
        return ip;
    }

    private static bool IsPrivateIp(string ip)
    {
        if (string.IsNullOrEmpty(ip)) return true;
        if (!IPAddress.TryParse(ip, out var addr)) return true;
        if (IPAddress.IsLoopback(addr)) return true;

        if (addr.IsIPv4MappedToIPv6) addr = addr.MapToIPv4();

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
