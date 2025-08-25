using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using GamificationEngine.Integration.Tests.Infrastructure.Monitoring;
using GamificationEngine.Integration.Tests.Infrastructure.Testing;
using Microsoft.Extensions.DependencyInjection;

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