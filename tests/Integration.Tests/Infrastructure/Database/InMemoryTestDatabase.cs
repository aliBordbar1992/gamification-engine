using GamificationEngine.Infrastructure.Storage.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GamificationEngine.Integration.Tests.Infrastructure.Database;

/// <summary>
/// In-memory test database implementation for fast, isolated testing
/// </summary>
public class InMemoryTestDatabase : TestDatabaseBase
{
    private readonly string _databaseName;

    public InMemoryTestDatabase(IServiceProvider serviceProvider, string? databaseName = null)
        : base(serviceProvider)
    {
        _databaseName = databaseName ?? $"test-db-{Guid.NewGuid()}";
    }

    public override async Task InitializeAsync()
    {
        _context = CreateDbContext();
        await EnsureCreatedAsync();
    }

    public override async Task EnsureCreatedAsync()
    {
        if (_context != null)
        {
            await _context.Database.EnsureCreatedAsync();
        }
    }

    protected override GamificationEngineDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<GamificationEngineDbContext>();
        ConfigureDatabaseOptions(optionsBuilder);
        return new GamificationEngineDbContext(optionsBuilder.Options);
    }

    protected override void ConfigureDatabaseOptions(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(_databaseName);

        // Configure in-memory specific options
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
    }

    protected override async Task PerformProviderCleanupAsync()
    {
        // In-memory database cleanup is handled by the base CleanupAsync method
        await Task.CompletedTask;
    }

    public override async Task CleanupAsync()
    {
        if (_context != null)
        {
            // For in-memory, we can just clear the data without removing the database
            await base.CleanupAsync();
        }
    }
}