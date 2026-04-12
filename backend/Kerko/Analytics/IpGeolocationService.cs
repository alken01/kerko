using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Kerko.Analytics;

public class IpGeolocationService(IHttpClientFactory httpClientFactory, ILogger<IpGeolocationService> logger)
{
    private readonly ConcurrentDictionary<string, string?> _cache = new();

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

    private async Task FetchBatchAsync(List<string> ips)
    {
        try
        {
            var client = httpClientFactory.CreateClient("IpGeo");
            var response = await client.PostAsJsonAsync("batch?fields=status,query,country,city", ips);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogDebug("ip-api.com batch returned {Status}", response.StatusCode);
                foreach (var ip in ips) _cache.TryAdd(ip, null);
                return;
            }

            var results = await response.Content.ReadFromJsonAsync<List<IpApiResult>>();
            if (results == null)
            {
                foreach (var ip in ips) _cache.TryAdd(ip, null);
                return;
            }

            foreach (var r in results)
            {
                if (r.Query == null) continue;

                if (r.Status == "success")
                {
                    var loc = !string.IsNullOrEmpty(r.City)
                        ? $"{r.City}, {r.Country}"
                        : r.Country;
                    _cache.TryAdd(r.Query, loc);
                }
                else
                {
                    _cache.TryAdd(r.Query, null);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to fetch IP geolocation for {Count} IPs", ips.Count);
            foreach (var ip in ips) _cache.TryAdd(ip, null);
        }
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
