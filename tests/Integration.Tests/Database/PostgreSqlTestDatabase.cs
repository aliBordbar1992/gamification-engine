using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GamificationEngine.Infrastructure.Storage.EntityFramework;

namespace GamificationEngine.Integration.Tests.Database;

/// <summary>
/// PostgreSQL test database implementation for integration testing with real database
/// </summary>
public class PostgreSqlTestDatabase : TestDatabaseBase
{
    private readonly string _connectionString;
    private readonly string _databaseName;

    public PostgreSqlTestDatabase(IServiceProvider serviceProvider, string? connectionString = null)
        : base(serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        _connectionString = connectionString ?? configuration.GetConnectionString("TestPostgreSql")
            ?? throw new InvalidOperationException("TestPostgreSql connection string not found in configuration");

        // Extract database name from connection string for cleanup
        _databaseName = ExtractDatabaseName(_connectionString);
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
            // For PostgreSQL, we ensure the database exists and apply migrations
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
        optionsBuilder.UseNpgsql(_connectionString, npgsqlOptions =>
        {
            // Configure PostgreSQL specific options
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        });

        // Enable detailed logging for debugging
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
    }

    protected override async Task PerformProviderCleanupAsync()
    {
        if (_context != null)
        {
            // For PostgreSQL, we can truncate tables for faster cleanup
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Events\" RESTART IDENTITY CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"UserStates\" RESTART IDENTITY CASCADE");
        }
    }

    public override async Task CleanupAsync()
    {
        if (_context != null)
        {
            // Use PostgreSQL-specific cleanup for better performance
            await PerformProviderCleanupAsync();
        }
    }

    /// <summary>
    /// Extracts the database name from a PostgreSQL connection string
    /// </summary>
    private static string ExtractDatabaseName(string connectionString)
    {
        try
        {
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
            return builder.Database ?? "postgres";
        }
        catch
        {
            // Fallback to a default name if parsing fails
            return "test_database";
        }
    }

    /// <summary>
    /// Gets the database name for this test instance
    /// </summary>
    public string DatabaseName => _databaseName;
}