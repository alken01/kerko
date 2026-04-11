using Microsoft.EntityFrameworkCore;

namespace Kerko.Analytics;

public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<RequestLog> RequestLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RequestLog>(e =>
        {
            e.HasIndex(r => new { r.TimestampUtc, r.Id })
                .IsDescending(true, true);
            e.HasIndex(r => r.ClientIp);
        });
    }
}
