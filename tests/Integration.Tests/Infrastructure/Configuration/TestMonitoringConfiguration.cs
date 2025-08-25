using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GamificationEngine.Integration.Tests.Infrastructure.Configuration;

/// <summary>
/// Configures test monitoring, metrics collection, and performance tracking
/// </summary>
public static class TestMonitoringConfiguration
{
    /// <summary>
    /// Adds test monitoring services to the service collection
    /// </summary>
    public static IServiceCollection AddTestMonitoring(
        this IServiceCollection services,
        TestSettings testSettings)
    {
        if (testSettings.EnableDetailedLogging)
        {
            services.AddSingleton<ITestMetricsCollector, TestMetricsCollector>();
            services.AddSingleton<ITestPerformanceMonitor, TestPerformanceMonitor>();
        }

        return services;
    }

    /// <summary>
    /// Configures test monitoring with custom settings
    /// </summary>
    public static IServiceCollection AddTestMonitoring(
        this IServiceCollection services,
        Action<TestMonitoringOptions> configureOptions)
    {
        var options = new TestMonitoringOptions();
        configureOptions(options);

        services.AddSingleton(options);
        services.AddSingleton<ITestMetricsCollector, TestMetricsCollector>();
        services.AddSingleton<ITestPerformanceMonitor, TestPerformanceMonitor>();

        return services;
    }
}