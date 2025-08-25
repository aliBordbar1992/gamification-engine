using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GamificationEngine.Infrastructure.Storage.EntityFramework;

namespace GamificationEngine.Integration.Tests.Database;

/// <summary>
/// Base abstract class for test database implementations providing common functionality
/// </summary>
public abstract class TestDatabaseBase : ITestDatabase
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IServiceScope _serviceScope;
    protected GamificationEngineDbContext? _context;

    protected TestDatabaseBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _serviceScope = serviceProvider.CreateScope();
    }

    public GamificationEngineDbContext Context
    {
        get
        {
            if (_context == null)
            {
                throw new InvalidOperationException("Database context not initialized. Call InitializeAsync() first.");
            }
            return _context;
        }
    }

    public abstract Task InitializeAsync();

    public virtual async Task CleanupAsync()
    {
        if (_context != null)
        {
            // Clear all data from the database
            _context.Events.RemoveRange(_context.Events);
            _context.UserStates.RemoveRange(_context.UserStates);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task SeedAsync()
    {
        // Default implementation - override in derived classes for specific seeding logic
        await Task.CompletedTask;
    }

    public abstract Task EnsureCreatedAsync();

    public IServiceProvider GetServiceProvider()
    {
        return _serviceScope.ServiceProvider;
    }

    /// <summary>
    /// Creates a new DbContext instance with the configured options
    /// </summary>
    protected abstract GamificationEngineDbContext CreateDbContext();

    /// <summary>
    /// Configures the database options for the specific provider
    /// </summary>
    protected abstract void ConfigureDatabaseOptions(DbContextOptionsBuilder optionsBuilder);

    /// <summary>
    /// Performs provider-specific cleanup
    /// </summary>
    protected abstract Task PerformProviderCleanupAsync();

    /// <summary>
    /// Disposes of the test database resources
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        if (_context != null)
        {
            await CleanupAsync();
            await _context.DisposeAsync();
        }

        _serviceScope?.Dispose();
        await ValueTask.CompletedTask;
    }
}