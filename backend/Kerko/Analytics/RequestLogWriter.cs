using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;

namespace Kerko.Analytics;

public class RequestLogWriter(
    Channel<RequestLog> channel,
    IServiceProvider serviceProvider,
    ILogger<RequestLogWriter> logger) : IHostedService, IDisposable
{
    private Task? _backgroundTask;
    private readonly CancellationTokenSource _cts = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _backgroundTask = Task.Run(() => DrainLoopAsync(_cts.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try { channel.Writer.Complete(); } catch (InvalidOperationException) { /* already completed */ }
        _cts.Cancel();

        if (_backgroundTask != null)
        {
            try
            {
                await _backgroundTask;
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }
        }

        // Drain remaining items
        await FlushRemainingAsync();
    }

    private async Task DrainLoopAsync(CancellationToken cancellationToken)
    {
        var batch = new List<RequestLog>(50);
        DateTime? firstItemTime = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Try to read with a timeout
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                if (firstItemTime.HasValue)
                {
                    var elapsed = DateTime.UtcNow - firstItemTime.Value;
                    var remaining = TimeSpan.FromSeconds(1) - elapsed;
                    if (remaining > TimeSpan.Zero)
                    {
                        timeoutCts.CancelAfter(remaining);
                    }
                    else
                    {
                        // 1s already elapsed, flush immediately
                        await FlushBatchAsync(batch);
                        batch.Clear();
                        firstItemTime = null;
                        continue;
                    }
                }

                try
                {
                    var item = await channel.Reader.ReadAsync(timeoutCts.Token);
                    if (firstItemTime == null)
                        firstItemTime = DateTime.UtcNow;

                    batch.Add(item);

                    if (batch.Count >= 50)
                    {
                        await FlushBatchAsync(batch);
                        batch.Clear();
                        firstItemTime = null;
                    }
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // 1s timeout hit — flush
                    if (batch.Count > 0)
                    {
                        await FlushBatchAsync(batch);
                        batch.Clear();
                        firstItemTime = null;
                    }
                }
            }
            catch (ChannelClosedException)
            {
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        if (batch.Count > 0)
        {
            await FlushBatchAsync(batch);
        }
    }

    private async Task FlushRemainingAsync()
    {
        var batch = new List<RequestLog>(50);
        while (channel.Reader.TryRead(out var item))
        {
            batch.Add(item);
            if (batch.Count >= 50)
            {
                await FlushBatchAsync(batch);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await FlushBatchAsync(batch);
        }
    }

    private async Task FlushBatchAsync(List<RequestLog> batch)
    {
        if (batch.Count == 0) return;

        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
            db.RequestLogs.AddRange(batch);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write analytics batch of {Count} items", batch.Count);
        }
    }

    public void Dispose()
    {
        _cts.Dispose();
    }
}
