using System.Collections.Concurrent;
using System.Net.Http.Json;
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
            .Where(ip => !IsPrivateIp(NormalizeIp(ip)) && !_cache.ContainsKey(ip))
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

    /// <summary>
    /// Strips the ::ffff: prefix from IPv4-mapped IPv6 addresses (e.g. ::ffff:3.71.121.233 -> 3.71.121.233).
    /// </summary>
    private static string NormalizeIp(string ip)
    {
        if (ip.StartsWith(MappedV4Prefix, StringComparison.OrdinalIgnoreCase))
            return ip[MappedV4Prefix.Length..];
        return ip;
    }

    private static bool IsPrivateIp(string ip)
    {
        if (string.IsNullOrEmpty(ip)) return true;
        return ip.StartsWith("10.") ||
               ip.StartsWith("172.16.") || ip.StartsWith("172.17.") ||
               ip.StartsWith("172.18.") || ip.StartsWith("172.19.") ||
               ip.StartsWith("172.20.") || ip.StartsWith("172.21.") ||
               ip.StartsWith("172.22.") || ip.StartsWith("172.23.") ||
               ip.StartsWith("172.24.") || ip.StartsWith("172.25.") ||
               ip.StartsWith("172.26.") || ip.StartsWith("172.27.") ||
               ip.StartsWith("172.28.") || ip.StartsWith("172.29.") ||
               ip.StartsWith("172.30.") || ip.StartsWith("172.31.") ||
               ip.StartsWith("192.168.") ||
               ip.StartsWith("127.") ||
               ip == "::1";
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
