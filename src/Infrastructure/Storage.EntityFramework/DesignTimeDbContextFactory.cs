using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GamificationEngine.Infrastructure.Storage.EntityFramework;

/// <summary>
/// Design-time factory for EF Core migrations
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GamificationEngineDbContext>
{
    public GamificationEngineDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<GamificationEngineDbContext>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback for design-time
            connectionString = "Host=localhost;Database=gamification_engine;Username=postgres;Password=postgres";
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new GamificationEngineDbContext(optionsBuilder.Options);
    }
}