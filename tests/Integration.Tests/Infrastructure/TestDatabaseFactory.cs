using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GamificationEngine.Integration.Tests.Infrastructure;

/// <summary>
/// Factory for creating test database instances based on configuration or explicit requests
/// </summary>
public static class TestDatabaseFactory
{
    /// <summary>
    /// Creates a test database based on the configuration settings
    /// </summary>
    public static ITestDatabase CreateFromConfiguration(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var databaseProvider = configuration.GetValue<string>("TestSettings:DatabaseProvider") ?? "InMemory";

        return Create(databaseProvider, serviceProvider);
    }

    /// <summary>
    /// Creates a test database of the specified type
    /// </summary>
    public static ITestDatabase Create(string databaseType, IServiceProvider serviceProvider)
    {
        return databaseType.ToLowerInvariant() switch
        {
            "inmemory" or "memory" => new InMemoryTestDatabase(serviceProvider),
            "postgresql" or "postgres" => new PostgreSqlTestDatabase(serviceProvider),
            _ => throw new ArgumentException($"Unsupported database type: {databaseType}. Supported types: InMemory, PostgreSQL")
        };
    }

    /// <summary>
    /// Creates an in-memory test database
    /// </summary>
    public static ITestDatabase CreateInMemory(IServiceProvider serviceProvider, string? databaseName = null)
    {
        return new InMemoryTestDatabase(serviceProvider, databaseName);
    }

    /// <summary>
    /// Creates a PostgreSQL test database
    /// </summary>
    public static ITestDatabase CreatePostgreSql(IServiceProvider serviceProvider, string? connectionString = null)
    {
        return new PostgreSqlTestDatabase(serviceProvider, connectionString);
    }

    /// <summary>
    /// Gets the available database types
    /// </summary>
    public static IEnumerable<string> GetAvailableTypes()
    {
        return new[] { "InMemory", "PostgreSQL" };
    }
}