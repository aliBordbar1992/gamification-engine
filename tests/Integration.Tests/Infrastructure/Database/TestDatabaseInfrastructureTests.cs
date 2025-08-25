using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GamificationEngine.Infrastructure.Storage.EntityFramework;
using Shouldly;

namespace GamificationEngine.Integration.Tests.Database;

/// <summary>
/// Tests for the test database infrastructure to ensure it works correctly
/// </summary>
public class TestDatabaseInfrastructureTests : IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<ITestDatabase> _testDatabases = new();

    public TestDatabaseInfrastructureTests()
    {
        var services = new ServiceCollection();

        // Add configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestSettings:DatabaseProvider"] = "InMemory",
                ["ConnectionStrings:TestPostgreSql"] = "Host=localhost;Port=5432;Database=gamification_test;Username=test_user;Password=test_password"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task InMemoryTestDatabase_ShouldInitializeAndCreateContext()
    {
        // Arrange
        var database = TestDatabaseFactory.CreateInMemory(_serviceProvider);
        _testDatabases.Add(database);

        // Act
        await database.InitializeAsync();
        var context = database.Context;

        // Assert
        context.ShouldNotBeNull();
        // Note: IsInMemory() extension method is not available in this version
        await database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task InMemoryTestDatabase_ShouldCleanupDataCorrectly()
    {
        // Arrange
        var database = TestDatabaseFactory.CreateInMemory(_serviceProvider);
        _testDatabases.Add(database);
        await database.InitializeAsync();

        // Seed some test data
        await TestDatabaseUtilities.SeedCommonTestDataAsync(database.Context);

        // Verify data was seeded
        var (eventCount, userStateCount) = await TestDatabaseUtilities.GetEntityCountsAsync(database.Context);
        eventCount.ShouldBeGreaterThan(0);
        userStateCount.ShouldBeGreaterThan(0);

        // Act
        await database.CleanupAsync();

        // Assert
        var (cleanEventCount, cleanUserStateCount) = await TestDatabaseUtilities.GetEntityCountsAsync(database.Context);
        cleanEventCount.ShouldBe(0);
        cleanUserStateCount.ShouldBe(0);
    }

    [Fact]
    public async Task TestDatabaseFactory_ShouldCreateCorrectDatabaseType()
    {
        // Arrange & Act
        var inMemoryDb = TestDatabaseFactory.Create("InMemory", _serviceProvider);
        var postgreSqlDb = TestDatabaseFactory.Create("PostgreSQL", _serviceProvider);

        _testDatabases.Add(inMemoryDb);
        _testDatabases.Add(postgreSqlDb);

        // Assert
        inMemoryDb.ShouldBeOfType<InMemoryTestDatabase>();
        postgreSqlDb.ShouldBeOfType<PostgreSqlTestDatabase>();
    }

    [Fact]
    public async Task TestDatabaseFactory_ShouldCreateFromConfiguration()
    {
        // Arrange & Act
        var database = TestDatabaseFactory.CreateFromConfiguration(_serviceProvider);
        _testDatabases.Add(database);

        // Assert
        database.ShouldBeOfType<InMemoryTestDatabase>(); // Based on our test config
    }

    [Fact]
    public async Task TestDatabaseFactory_ShouldThrowForUnsupportedType()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentException>(() => TestDatabaseFactory.Create("Unsupported", _serviceProvider));
    }

    [Fact]
    public async Task TestDatabaseUtilities_ShouldSeedAndCleanupData()
    {
        // Arrange
        var database = TestDatabaseFactory.CreateInMemory(_serviceProvider);
        _testDatabases.Add(database);
        await database.InitializeAsync();

        // Act - Seed data
        await TestDatabaseUtilities.SeedCommonTestDataAsync(database.Context);

        // Assert - Verify data was seeded
        var (eventCount, userStateCount) = await TestDatabaseUtilities.GetEntityCountsAsync(database.Context);
        eventCount.ShouldBeGreaterThan(0);
        userStateCount.ShouldBeGreaterThan(0);

        // Act - Cleanup data
        await TestDatabaseUtilities.CleanupAllTestDataAsync(database.Context);

        // Assert - Verify data was cleaned up
        var (cleanEventCount, cleanUserStateCount) = await TestDatabaseUtilities.GetEntityCountsAsync(database.Context);
        cleanEventCount.ShouldBe(0);
        cleanUserStateCount.ShouldBe(0);
    }

    [Fact]
    public async Task TestDatabaseUtilities_ShouldResetDatabaseToKnownState()
    {
        // Arrange
        var database = TestDatabaseFactory.CreateInMemory(_serviceProvider);
        _testDatabases.Add(database);
        await database.InitializeAsync();

        // Act
        await TestDatabaseUtilities.ResetDatabaseAsync(database.Context);

        // Assert
        var (eventCount, userStateCount) = await TestDatabaseUtilities.GetEntityCountsAsync(database.Context);
        eventCount.ShouldBeGreaterThan(0);
        userStateCount.ShouldBeGreaterThan(0);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var database in _testDatabases)
        {
            if (database is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
    }
}