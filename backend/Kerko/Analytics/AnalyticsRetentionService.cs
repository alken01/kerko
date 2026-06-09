using Microsoft.EntityFrameworkCore;

namespace Kerko.Analytics;

/// <summary>
/// Prunes old rows from the analytics DB once a day so it doesn't grow unbounded
/// on the server's small disk. RequestLogs older than Analytics:RetentionDays
/// (default 90, 0 disables) and expired IpLocationCache entries are deleted.
/// </summary>
public class AnalyticsRetentionService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<AnalyticsRetentionService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retentionDays = configuration.GetValue<int>("Analytics:RetentionDays", 90);
        if (retentionDays <= 0)
        {
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        do
        {
            await PruneAsync(retentionDays, stoppingToken);
        }
        while (await WaitForNextTickSafeAsync(timer, stoppingToken));
    }

    private async Task PruneAsync(int retentionDays, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

            var logCutoff = DateTime.UtcNow.AddDays(-retentionDays);
            var removedLogs = await db.RequestLogs
                .Where(r => r.TimestampUtc < logCutoff)
                .ExecuteDeleteAsync(cancellationToken);

            var geoCutoff = DateTime.UtcNow - IpGeolocationService.CacheTtl;
            var removedGeo = await db.IpLocations
                .Where(c => c.CachedAtUtc < geoCutoff)
                .ExecuteDeleteAsync(cancellationToken);

            if (removedLogs > 0 || removedGeo > 0)
            {
                logger.LogInformation(
                    "Analytics retention pruned {RemovedLogs} request logs and {RemovedGeo} IP cache rows",
                    removedLogs, removedGeo);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Analytics retention prune failed");
        }
    }

    private static async Task<bool> WaitForNextTickSafeAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
