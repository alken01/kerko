namespace Kerko.Analytics;

/// <summary>
/// Persistent cache of resolved IP locations so a restart doesn't re-query
/// ip-api.com for every known IP. Only successful lookups are stored.
/// </summary>
public class IpLocationCache
{
    public string Ip { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime CachedAtUtc { get; set; }
}
