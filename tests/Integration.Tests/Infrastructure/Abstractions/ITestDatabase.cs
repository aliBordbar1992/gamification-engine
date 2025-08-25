using Microsoft.EntityFrameworkCore;
using GamificationEngine.Infrastructure.Storage.EntityFramework;

namespace GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

/// <summary>
/// Interface for test database implementations providing database management capabilities for integration tests
/// </summary>
public interface ITestDatabase : IAsyncDisposable
{
    /// <summary>
    /// Gets the database context for the test database
    /// </summary>
    GamificationEngineDbContext Context { get; }

    /// <summary>
    /// Initializes the test database
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Ensures the database is created and ready for use
    /// </summary>
    Task EnsureCreatedAsync();

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    Task SeedAsync();

    /// <summary>
    /// Cleans up all data from the database
    /// </summary>
    Task CleanupAsync();

    /// <summary>
    /// Gets the service provider for the test database scope
    /// </summary>
    IServiceProvider GetServiceProvider();
}