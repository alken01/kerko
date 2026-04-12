using Microsoft.EntityFrameworkCore;

namespace Kerko.Analytics;

/// <summary>
/// One-shot background service that backfills Location for existing logs on startup.
/// </summary>
public class LocationBackfillService(
    IServiceProvider serviceProvider,
    IpGeolocationService geoService,
    ILogger<LocationBackfillService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let the app finish starting before backfilling
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

            var ips = await db.RequestLogs
                .Where(r => r.Location == null)
                .Select(r => r.ClientIp)
                .Distinct()
                .ToListAsync(stoppingToken);

            if (ips.Count == 0)
            {
                logger.LogInformation("Location backfill: nothing to do");
                return;
            }

            logger.LogInformation("Location backfill: resolving {Count} unique IP(s)", ips.Count);

            var resolved = await geoService.ResolveBatchAsync(ips);

            var updated = 0;
            foreach (var (ip, location) in resolved)
            {
                if (location == null) continue;
                updated += await db.Database.ExecuteSqlAsync(
                    $"UPDATE RequestLogs SET Location = {location} WHERE ClientIp = {ip} AND Location IS NULL",
                    stoppingToken);
            }

            logger.LogInformation("Location backfill: updated {Updated} log(s)", updated);
        }
        catch (OperationCanceledException)
        {
            // Shutdown requested
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Location backfill failed");
        }
    }
}
