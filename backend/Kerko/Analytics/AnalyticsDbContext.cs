using Microsoft.EntityFrameworkCore;

namespace Kerko.Analytics;

public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<RequestLog> RequestLogs { get; set; }
    public DbSet<IpLocationCache> IpLocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RequestLog>(e =>
        {
            e.HasIndex(r => new { r.TimestampUtc, r.Id })
                .IsDescending(true, true);
            e.HasIndex(r => r.ClientIp);
            e.HasIndex(r => r.Endpoint);
        });

        modelBuilder.Entity<IpLocationCache>(e =>
        {
            e.ToTable("IpLocationCache");
            e.HasKey(c => c.Ip);
        });
    }
}
