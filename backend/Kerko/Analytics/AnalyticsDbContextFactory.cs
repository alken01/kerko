using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kerko.Analytics;

/// <summary>
/// Design-time factory used by EF Core CLI tools (dotnet ef migrations add, etc.)
/// when the application's DI container cannot be used.
/// </summary>
public class AnalyticsDbContextFactory : IDesignTimeDbContextFactory<AnalyticsDbContext>
{
    public AnalyticsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AnalyticsDbContext>();
        optionsBuilder.UseSqlite("Data Source=/Users/alken/Code/kerko/backend/data/analytics.db");
        return new AnalyticsDbContext(optionsBuilder.Options);
    }
}
