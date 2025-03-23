using Microsoft.EntityFrameworkCore;
using Kerko.Core.Models;

namespace Kerko.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<City> Cities { get; set; }
    public DbSet<MaritalStatus> MaritalStatuses { get; set; }
    public DbSet<Nationality> Nationalities { get; set; }
    public DbSet<PatronazhInfo> PatronazhInfos { get; set; }
    public DbSet<Person> People { get; set; }
    public DbSet<PreviousSurname> PreviousSurnames { get; set; }
    public DbSet<Relationship> Relationships { get; set; }
    public DbSet<Salary> Salaries { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Person>()
            .HasOne<City>()
            .WithMany()
            .HasForeignKey(p => p.CityId);

        modelBuilder.Entity<Person>()
            .HasOne<MaritalStatus>()
            .WithMany()
            .HasForeignKey(p => p.MaritalStatusId);

        modelBuilder.Entity<Person>()
            .HasOne<Nationality>()
            .WithMany()
            .HasForeignKey(p => p.NationalityId);

        modelBuilder.Entity<Person>()
            .HasOne<Relationship>()
            .WithMany()
            .HasForeignKey(p => p.RelationshipId);

        modelBuilder.Entity<PatronazhInfo>()
            .HasOne<Person>()
            .WithMany()
            .HasForeignKey(p => p.PersonId);

        modelBuilder.Entity<PreviousSurname>()
            .HasOne<Person>()
            .WithMany()
            .HasForeignKey(p => p.PersonId);

        modelBuilder.Entity<Salary>()
            .HasOne<Person>()
            .WithMany()
            .HasForeignKey(p => p.PersonId);

        modelBuilder.Entity<Vehicle>()
            .HasOne<Person>()
            .WithMany()
            .HasForeignKey(p => p.PersonId);
    }
} 