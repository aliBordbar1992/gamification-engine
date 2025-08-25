using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GamificationEngine.Integration.Tests.Infrastructure.Testing;
using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

namespace GamificationEngine.Integration.Tests.Infrastructure.Configuration;

/// <summary>
/// Configuration extensions for performance testing infrastructure
/// </summary>
public static class PerformanceTestingConfiguration
{
    /// <summary>
    /// Adds performance testing services to the service collection
    /// </summary>
    public static IServiceCollection AddPerformanceTesting(
        this IServiceCollection services,
        TestSettings testSettings)
    {
        if (testSettings.EnablePerformanceTesting)
        {
            services.AddSingleton<IPerformanceTestHarness, PerformanceTestHarness>();
            services.AddSingleton<ITestExecutionMonitor, TestExecutionMonitor>();
        }

        return services;
    }

    /// <summary>
    /// Adds performance testing services with custom configuration
    /// </summary>
    public static IServiceCollection AddPerformanceTesting(
        this IServiceCollection services,
        Action<PerformanceTestingOptions> configureOptions)
    {
        var options = new PerformanceTestingOptions();
        configureOptions(options);

        services.AddSingleton(options);
        services.AddSingleton<IPerformanceTestHarness, PerformanceTestHarness>();
        services.AddSingleton<ITestExecutionMonitor, TestExecutionMonitor>();

        return services;
    }

    /// <summary>
    /// Adds performance testing services with default configuration
    /// </summary>
    public static IServiceCollection AddPerformanceTesting(this IServiceCollection services)
    {
        var options = new PerformanceTestingOptions();
        services.AddSingleton(options);
        services.AddSingleton<IPerformanceTestHarness, PerformanceTestHarness>();
        services.AddSingleton<ITestExecutionMonitor, TestExecutionMonitor>();

        return services;
    }
}

/// <summary>
/// Options for configuring performance testing
/// </summary>
public class PerformanceTestingOptions
{
    /// <summary>
    /// Whether to enable performance testing infrastructure
    /// </summary>
    public bool EnablePerformanceTesting { get; set; } = true;

    /// <summary>
    /// Whether to enable load testing capabilities
    /// </summary>
    public bool EnableLoadTesting { get; set; } = true;

    /// <summary>
    /// Whether to enable stress testing capabilities
    /// </summary>
    public bool EnableStressTesting { get; set; } = true;

    /// <summary>
    /// Whether to enable baseline testing capabilities
    /// </summary>
    public bool EnableBaselineTesting { get; set; } = true;

    /// <summary>
    /// Whether to enable test execution monitoring
    /// </summary>
    public bool EnableTestExecutionMonitoring { get; set; } = true;

    /// <summary>
    /// Default concurrency for load tests
    /// </summary>
    public int DefaultLoadTestConcurrency { get; set; } = 10;

    /// <summary>
    /// Default duration for load tests
    /// </summary>
    public TimeSpan DefaultLoadTestDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Default number of iterations for baseline tests
    /// </summary>
    public int DefaultBaselineTestIterations { get; set; } = 100;

    /// <summary>
    /// Whether to enable real-time performance reporting
    /// </summary>
    public bool EnableRealTimeReporting { get; set; } = false;

    /// <summary>
    /// Whether to enable performance metrics export
    /// </summary>
    public bool EnableMetricsExport { get; set; } = true;

    /// <summary>
    /// Performance thresholds for alerts
    /// </summary>
    public PerformanceThresholds Thresholds { get; set; } = new();
}

/// <summary>
/// Performance thresholds for testing
/// </summary>
public class PerformanceThresholds
{
    /// <summary>
    /// Maximum acceptable response time for API endpoints (in milliseconds)
    /// </summary>
    public int MaxResponseTimeMs { get; set; } = 1000;

    /// <summary>
    /// Maximum acceptable test execution time (in milliseconds)
    /// </summary>
    public int MaxTestExecutionTimeMs { get; set; } = 30000;

    /// <summary>
    /// Minimum acceptable success rate for tests (0.0 to 1.0)
    /// </summary>
    public double MinSuccessRate { get; set; } = 0.95;

    /// <summary>
    /// Maximum acceptable memory usage (in MB)
    /// </summary>
    public int MaxMemoryUsageMb { get; set; } = 512;

    /// <summary>
    /// Maximum acceptable CPU usage percentage
    /// </summary>
    public double MaxCpuUsagePercent { get; set; } = 80.0;
}