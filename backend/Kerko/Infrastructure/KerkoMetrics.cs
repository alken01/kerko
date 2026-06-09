using System.Diagnostics.Metrics;

namespace Kerko.Infrastructure;

/// <summary>
/// Process-wide counters for failure paths that would otherwise be invisible.
/// Readable without extra infrastructure via `dotnet-counters monitor --counters Kerko`.
/// </summary>
public static class KerkoMetrics
{
    public static readonly Meter Meter = new("Kerko");

    public static readonly Counter<long> AnalyticsLogsDropped =
        Meter.CreateCounter<long>("kerko.analytics.logs_dropped", description: "Request logs dropped because the analytics channel was full");

    public static readonly Counter<long> AnalyticsWriteFailures =
        Meter.CreateCounter<long>("kerko.analytics.write_failures", description: "Analytics batches that failed to persist");

    public static readonly Counter<long> GeolocationFailures =
        Meter.CreateCounter<long>("kerko.geolocation.failures", description: "Failed ip-api.com batch lookups");
}
