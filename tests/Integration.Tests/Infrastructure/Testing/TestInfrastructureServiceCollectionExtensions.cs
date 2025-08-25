using GamificationEngine.Integration.Tests.Database;
using GamificationEngine.Integration.Tests.Infrastructure.Logging;
using GamificationEngine.Integration.Tests.Testing.BusinessLogic;
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
        // TestDatabaseFactory is static, no registration needed

        // Add database configuration
        services.Configure<TestDatabaseOptions>(options =>
        {
            options.DatabaseType = databaseType;
            options.ConnectionString = connectionString;
        });

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

/// <summary>
/// Configuration options for test infrastructure
/// </summary>
public class TestInfrastructureOptions
{
    /// <summary>
    /// Default timeout for test operations
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Default polling interval for wait operations
    /// </summary>
    public TimeSpan DefaultPollInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Whether to enable detailed logging
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable performance monitoring
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = false;
}

/// <summary>
/// Configuration options for test database
/// </summary>
public class TestDatabaseOptions
{
    /// <summary>
    /// Type of database to use for testing
    /// </summary>
    public string DatabaseType { get; set; } = "InMemory";

    /// <summary>
    /// Connection string for the test database
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Whether to enable database logging
    /// </summary>
    public bool EnableLogging { get; set; } = false;

    /// <summary>
    /// Whether to enable sensitive data logging
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;
}