using Kerko.Models;
using Microsoft.EntityFrameworkCore;

namespace Kerko.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Person> Person { get; set; }
    public DbSet<Rrogat> Rrogat { get; set; }
    public DbSet<Targat> Targat { get; set; }
    public DbSet<Patronazhist> Patronazhist { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure indexes for better search performance
        modelBuilder.Entity<Person>()
            .HasIndex(p => p.Mbiemer);
        modelBuilder.Entity<Person>()
            .HasIndex(p => p.Emer);

        modelBuilder.Entity<Rrogat>()
            .HasIndex(r => r.Mbiemri);
        modelBuilder.Entity<Rrogat>()
            .HasIndex(r => r.Emri);

        modelBuilder.Entity<Targat>()
            .HasIndex(t => t.Mbiemri);
        modelBuilder.Entity<Targat>()
            .HasIndex(t => t.Emri);

        modelBuilder.Entity<Patronazhist>()
            .HasIndex(p => p.Mbiemri);
        modelBuilder.Entity<Patronazhist>()
            .HasIndex(p => p.Emri);
    }
} 