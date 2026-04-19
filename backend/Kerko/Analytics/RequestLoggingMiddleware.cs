using System.Diagnostics;
using System.Threading.Channels;
using Kerko.Http;

namespace Kerko.Analytics;

public class RequestLoggingMiddleware(RequestDelegate next, Channel<RequestLog> channel, ILogger<RequestLoggingMiddleware> logger)
{
    private static readonly HashSet<string> _trackedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/kerko",
        "/api/targat",
        "/api/telefon"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (!_trackedPaths.Contains(path))
        {
            await next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        var timestamp = DateTime.UtcNow;

        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();
            EnqueueLog(context, path, timestamp, sw.ElapsedMilliseconds);
        }
    }

    private void EnqueueLog(HttpContext context, string path, DateTime timestamp, long durationMs)
    {
        try
        {
            var query = context.Request.Query;
            var endpoint = path.TrimStart('/').Split('/').LastOrDefault() ?? string.Empty;
            var userAgentRaw = context.Request.Headers.UserAgent.ToString();

            var log = new RequestLog
            {
                TimestampUtc = timestamp,
                Endpoint = endpoint,
                Emri = query["emri"].FirstOrDefault(),
                Mbiemri = query["mbiemri"].FirstOrDefault(),
                NumriTarges = query["numriTarges"].FirstOrDefault(),
                NumriTelefonit = query["numriTelefonit"].FirstOrDefault(),
                PageNumber = int.TryParse(query["pageNumber"].FirstOrDefault(), out var pn) ? pn : 1,
                PageSize = int.TryParse(query["pageSize"].FirstOrDefault(), out var ps) ? ps : 10,
                ClientIp = ClientInfo.GetClientIpAddress(context.Request),
                UserAgentRaw = userAgentRaw,
                UserAgentSimplified = ClientInfo.SimplifyUserAgent(userAgentRaw),
                StatusCode = context.Response.StatusCode,
                DurationMs = (int)durationMs,
                ResultCount = context.Items.TryGetValue("Kerko.ResultCount", out var rc) ? rc as int? : null,
                RequestId = context.TraceIdentifier
            };

            if (!channel.Writer.TryWrite(log))
            {
                logger.LogDebug("Analytics channel full, dropping log for {Path}", path);
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to enqueue analytics log for {Path}", path);
        }
    }
}
