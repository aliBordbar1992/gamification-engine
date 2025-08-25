using GamificationEngine.Infrastructure.Storage.EntityFramework;
using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

namespace GamificationEngine.Integration.Tests.Tests;

/// <summary>
/// Mock test database for unit testing
/// </summary>
public class MockTestDatabase : ITestDatabase
{
    public GamificationEngineDbContext Context => throw new NotImplementedException("Mock database context not implemented for unit tests");

    public Task InitializeAsync() => Task.CompletedTask;
    public Task EnsureCreatedAsync() => Task.CompletedTask;
    public Task SeedAsync() => Task.CompletedTask;
    public Task CleanupAsync() => Task.CompletedTask;
    public IServiceProvider GetServiceProvider() => throw new NotImplementedException();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}