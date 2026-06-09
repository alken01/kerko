using Kerko.Models;
using Microsoft.EntityFrameworkCore;

namespace Kerko.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Person> Person { get; set; }
    public DbSet<Rrogat> Rrogat { get; set; }
    public DbSet<Targat> Targat { get; set; }
    public DbSet<Patronazhist> Patronazhist { get; set; }

    // The main DB is opened Mode=ReadOnly in production; fail fast with a clear
    // message instead of surfacing a cryptic SQLite error at write time. Tests
    // seed via in-memory connections, which stay writable.
    private bool IsReadOnlyConnection =>
        Database.GetConnectionString()?.Contains("Mode=ReadOnly", StringComparison.OrdinalIgnoreCase) == true;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ThrowIfReadOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ThrowIfReadOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ThrowIfReadOnly()
    {
        if (IsReadOnlyConnection)
        {
            throw new InvalidOperationException("ApplicationDbContext is read-only.");
        }
    }

    // SQL expression that folds Albanian diacritics: Ç/ç → c, Ë/ë → e, and lowercases ASCII.
    // SQLite's LOWER() only touches ASCII, so we replace both cases of the diacritics explicitly.
    private static string NormalizeExpr(string column)
    {
        return $"REPLACE(REPLACE(REPLACE(REPLACE(LOWER(\"{column}\"), 'Ç', 'c'), 'ç', 'c'), 'Ë', 'e'), 'ë', 'e')";
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Person — computed diacritic-folded columns + composite index for prefix search
        modelBuilder.Entity<Person>(e =>
        {
            e.Property(p => p.EmerNormalized)
                .HasComputedColumnSql(NormalizeExpr("Emer"), stored: false);
            e.Property(p => p.MbiemerNormalized)
                .HasComputedColumnSql(NormalizeExpr("Mbiemer"), stored: false);
            e.HasIndex(p => new { p.MbiemerNormalized, p.EmerNormalized });
            e.HasIndex(p => p.EmerNormalized);
        });

        // Rrogat
        modelBuilder.Entity<Rrogat>(e =>
        {
            e.Property(r => r.EmriNormalized)
                .HasComputedColumnSql(NormalizeExpr("Emri"), stored: false);
            e.Property(r => r.MbiemriNormalized)
                .HasComputedColumnSql(NormalizeExpr("Mbiemri"), stored: false);
            e.HasIndex(r => new { r.MbiemriNormalized, r.EmriNormalized });
            e.HasIndex(r => r.EmriNormalized);
        });

        // Targat — keep NumriTarges index for the separate plate search
        modelBuilder.Entity<Targat>(e =>
        {
            e.Property(t => t.EmriNormalized)
                .HasComputedColumnSql(NormalizeExpr("Emri"), stored: false);
            e.Property(t => t.MbiemriNormalized)
                .HasComputedColumnSql(NormalizeExpr("Mbiemri"), stored: false);
            e.HasIndex(t => new { t.MbiemriNormalized, t.EmriNormalized });
            e.HasIndex(t => t.EmriNormalized);
            e.HasIndex(t => t.NumriTarges);
        });

        // Patronazhist — keep Tel index for the separate phone search
        modelBuilder.Entity<Patronazhist>(e =>
        {
            e.Property(p => p.EmriNormalized)
                .HasComputedColumnSql(NormalizeExpr("Emri"), stored: false);
            e.Property(p => p.MbiemriNormalized)
                .HasComputedColumnSql(NormalizeExpr("Mbiemri"), stored: false);
            e.HasIndex(p => new { p.MbiemriNormalized, p.EmriNormalized });
            e.HasIndex(p => p.EmriNormalized);
            e.HasIndex(p => p.Tel);
        });
    }
}
