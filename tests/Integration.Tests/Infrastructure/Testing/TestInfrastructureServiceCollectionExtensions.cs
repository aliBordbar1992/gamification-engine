using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Data;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using GamificationEngine.Integration.Tests.Infrastructure.Monitoring;
using GamificationEngine.Integration.Tests.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Extension methods for configuring test infrastructure services
/// </summary>
public static class TestInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds test infrastructure services to the service collection
    /// </summary>
    public static IServiceCollection AddTestInfrastructure(this IServiceCollection services)
    {
        // TestDatabaseFactory is static, no registration needed

        // Add test lifecycle management
        services.AddScoped<ITestLifecycleManager, TestLifecycleManager>();

        // Add test data management
        services.AddScoped<ITestDataManager, TestDataManager>();

        // Add test timing utilities
        services.AddScoped<ITestTimingUtilities, TestTimingUtilities>();

        // Add test assertion utilities (static class, no registration needed)

        // Add test HTTP client factory (static class, no registration needed)

        return services;
    }

    /// <summary>
    /// Adds test infrastructure services with custom configuration
    /// </summary>
    public static IServiceCollection AddTestInfrastructure(
        this IServiceCollection services,
        Action<TestInfrastructureOptions> configureOptions)
    {
        // Add default services
        services.AddTestInfrastructure();

        // Configure options
        var options = new TestInfrastructureOptions();
        configureOptions(options);

        // Register options
        services.Configure<TestInfrastructureOptions>(opt =>
        {
            opt.DefaultTimeout = options.DefaultTimeout;
            opt.DefaultPollInterval = options.DefaultPollInterval;
            opt.EnableDetailedLogging = options.EnableDetailedLogging;
            opt.EnablePerformanceMonitoring = options.EnablePerformanceMonitoring;
        });

        // Add performance monitoring if enabled
        if (options.EnablePerformanceMonitoring)
        {
            services.AddScoped<ITestPerformanceMonitor, TestPerformanceMonitor>();
        }

        // Add metrics collection if enabled
        if (options.EnableDetailedLogging)
        {
            services.AddScoped<ITestMetricsCollector, TestMetricsCollector>();
        }

        return services;
    }

    /// <summary>
    /// Adds test database services to the service collection
    /// </summary>
    public static IServiceCollection AddTestDatabase(
        this IServiceCollection services,
        string databaseType = "InMemory",
        string? connectionString = null)
    {
        // Add database configuration
        services.Configure<TestDatabaseOptions>(options =>
        {
            options.DatabaseType = databaseType;
            options.ConnectionString = connectionString;
        });

        // Register DbContext based on database type
        if (databaseType.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            // Register PostgreSQL DbContext
            services.AddDbContext<GamificationEngine.Infrastructure.Storage.EntityFramework.GamificationEngineDbContext>(options =>
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string is required for PostgreSQL database.");
                }

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                });

                // Enable detailed logging for tests
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
        }
        else
        {
            // Register InMemory DbContext
            services.AddDbContext<GamificationEngine.Infrastructure.Storage.EntityFramework.GamificationEngineDbContext>(options =>
            {
                options.UseInMemoryDatabase($"test-db-{Guid.NewGuid()}");
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
        }

        return services;
    }

    /// <summary>
    /// Adds test logging configuration
    /// </summary>
    public static IServiceCollection AddTestLogging(
        this IServiceCollection services,
        LogLevel minimumLevel = LogLevel.Information)
    {
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(minimumLevel);
            builder.AddConsole();
            builder.AddDebug();
        });

        return services;
    }
}