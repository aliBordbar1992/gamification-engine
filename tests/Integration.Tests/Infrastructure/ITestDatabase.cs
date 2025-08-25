using GamificationEngine.Infrastructure.Storage.EntityFramework;

namespace GamificationEngine.Integration.Tests.Infrastructure;

/// <summary>
/// Interface for managing test database operations across different database providers
/// </summary>
public interface ITestDatabase
{
    /// <summary>
    /// Gets the database context for the current test
    /// </summary>
    GamificationEngineDbContext Context { get; }

    /// <summary>
    /// Initializes the database with test data
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Cleans up the database after a test
    /// </summary>
    Task CleanupAsync();

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    Task SeedAsync();

    /// <summary>
    /// Ensures the database is created and ready for testing
    /// </summary>
    Task EnsureCreatedAsync();

    /// <summary>
    /// Gets a scoped service provider for dependency injection in tests
    /// </summary>
    IServiceProvider GetServiceProvider();
}