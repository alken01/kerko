using System.Text;
using Kerko.Analytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kerko.Admin;

[ApiController]
[Route("api/admin")]
[ResponseCache(NoStore = true)]
public class AdminController(AnalyticsDbContext db, IpGeolocationService geoService) : ControllerBase
{
    [HttpGet("logs")]
    public async Task<IActionResult> Logs(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? endpoint,
        [FromQuery] string? q,
        [FromQuery] string? ip,
        [FromQuery] int? status,
        [FromQuery] int limit = 50,
        [FromQuery] string? cursor = null)
    {
        limit = Math.Clamp(limit, 1, 200);

        var query = db.RequestLogs.AsNoTracking().AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(from) && DateTime.TryParse(from, null, System.Globalization.DateTimeStyles.RoundtripKind, out var fromDt))
            query = query.Where(r => r.TimestampUtc >= fromDt);

        if (!string.IsNullOrEmpty(to) && DateTime.TryParse(to, null, System.Globalization.DateTimeStyles.RoundtripKind, out var toDt))
            query = query.Where(r => r.TimestampUtc <= toDt);

        if (!string.IsNullOrEmpty(endpoint))
            query = query.Where(r => r.Endpoint == endpoint);

        if (!string.IsNullOrEmpty(q))
            query = query.Where(r =>
                (r.Emri != null && r.Emri.Contains(q)) ||
                (r.Mbiemri != null && r.Mbiemri.Contains(q)) ||
                (r.NumriTarges != null && r.NumriTarges.Contains(q)) ||
                (r.NumriTelefonit != null && r.NumriTelefonit.Contains(q)));

        if (!string.IsNullOrEmpty(ip))
            query = query.Where(r => r.ClientIp.Contains(ip));

        if (status.HasValue)
            query = query.Where(r => r.StatusCode == status.Value);

        // Cursor-based pagination (reverse-chronological)
        if (!string.IsNullOrEmpty(cursor))
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                var parts = decoded.Split(':');
                if (parts.Length == 2 && long.TryParse(parts[0], out var cursorTicks) && long.TryParse(parts[1], out var cursorId))
                {
                    var cursorTime = new DateTime(cursorTicks, DateTimeKind.Utc);
                    query = query.Where(r => r.TimestampUtc < cursorTime ||
                                            (r.TimestampUtc == cursorTime && r.Id < cursorId));
                }
            }
            catch
            {
                // Invalid cursor, ignore
            }
        }

        var items = await query
            .OrderByDescending(r => r.TimestampUtc)
            .ThenByDescending(r => r.Id)
            .Take(limit + 1)
            .ToListAsync();

        string? nextCursor = null;
        if (items.Count > limit)
        {
            items.RemoveAt(items.Count - 1);
            var last = items[^1];
            var cursorStr = $"{last.TimestampUtc.Ticks}:{last.Id}";
            nextCursor = Convert.ToBase64String(Encoding.UTF8.GetBytes(cursorStr));
        }

        return Ok(new { items, nextCursor });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> Stats([FromQuery] string window = "24h")
    {
        var now = DateTime.UtcNow;
        var windowStart = window switch
        {
            "1h" => now.AddHours(-1),
            "7d" => now.AddDays(-7),
            "30d" => now.AddDays(-30),
            _ => now.AddHours(-24) // "24h" default
        };

        var todayStart = now.Date;
        var last7dStart = now.AddDays(-7);
        var earliest = new[] { windowStart, todayStart, last7dStart }.Min();

        var counts = await db.RequestLogs
            .Where(r => r.TimestampUtc >= earliest)
            .GroupBy(r => 1)
            .Select(g => new
            {
                total = g.Sum(r => r.TimestampUtc >= windowStart ? 1 : 0),
                totalToday = g.Sum(r => r.TimestampUtc >= todayStart ? 1 : 0),
                totalLast7d = g.Sum(r => r.TimestampUtc >= last7dStart ? 1 : 0)
            })
            .FirstOrDefaultAsync();

        var total = counts?.total ?? 0;
        var totalToday = counts?.totalToday ?? 0;
        var totalLast7d = counts?.totalLast7d ?? 0;

        var topIpsRaw = await db.RequestLogs
            .Where(r => r.TimestampUtc >= windowStart)
            .GroupBy(r => r.ClientIp)
            .Select(g => new { ip = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToListAsync();

        // Resolve locations for top IPs
        var ipList = topIpsRaw.Select(x => x.ip).ToList();
        var locations = await geoService.ResolveBatchAsync(ipList);
        var topIps = topIpsRaw.Select(x => new
        {
            x.ip,
            x.count,
            location = locations.GetValueOrDefault(x.ip)
        }).ToList();

        // Coalesce query terms into a normalized string for grouping
        var topQueriesRaw = await db.RequestLogs
            .Where(r => r.TimestampUtc >= windowStart)
            .Where(r => r.Emri != null || r.Mbiemri != null || r.NumriTarges != null || r.NumriTelefonit != null)
            .Select(r => new
            {
                r.Emri,
                r.Mbiemri,
                r.NumriTarges,
                r.NumriTelefonit
            })
            .Take(10_000)
            .ToListAsync();

        var topQueries = topQueriesRaw
            .Select(r =>
            {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(r.Emri)) parts.Add($"emri={r.Emri}");
                if (!string.IsNullOrEmpty(r.Mbiemri)) parts.Add($"mbiemri={r.Mbiemri}");
                if (!string.IsNullOrEmpty(r.NumriTarges)) parts.Add($"numriTarges={r.NumriTarges}");
                if (!string.IsNullOrEmpty(r.NumriTelefonit)) parts.Add($"numriTelefonit={r.NumriTelefonit}");
                return string.Join(" ", parts);
            })
            .Where(s => !string.IsNullOrEmpty(s))
            .GroupBy(s => s.ToLowerInvariant())
            .Select(g => new { term = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToList();

        return Ok(new
        {
            window,
            total,
            totalToday,
            totalLast7d,
            topIps,
            topQueries
        });
    }

}
