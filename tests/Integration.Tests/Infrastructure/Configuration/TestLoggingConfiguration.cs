using GamificationEngine.Integration.Tests.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Configuration;

/// <summary>
/// Configures test-specific logging including structured logging, log levels, and test monitoring
/// </summary>
public static class TestLoggingConfiguration
{
    /// <summary>
    /// Configures logging services for testing with configurable log levels and providers
    /// </summary>
    public static IServiceCollection AddTestLogging(
        this IServiceCollection services,
        TestSettings testSettings,
        LoggingSettings loggingSettings)
    {
        // Clear existing logging configuration
        services.ClearLogging();

        // Configure logging based on test settings
        if (testSettings.EnableDetailedLogging)
        {
            services.AddLogging(builder =>
            {
                // Set default log level
                builder.SetMinimumLevel(GetLogLevel(loggingSettings.DefaultLevel));

                // Configure framework-specific log levels
                builder.AddFilter("Microsoft.AspNetCore", GetLogLevel(loggingSettings.MicrosoftAspNetCoreLevel));
                builder.AddFilter("Microsoft.EntityFrameworkCore", GetLogLevel(loggingSettings.MicrosoftEntityFrameworkLevel));

                // Add console logging for test output
                builder.AddConsole(options =>
                {
                    options.FormatterName = loggingSettings.EnableStructuredLogging ? "json" : "simple";
                });

                // Add debug logging for development
                builder.AddDebug();

                // Configure JSON formatter for structured logging
                if (loggingSettings.EnableStructuredLogging)
                {
                    // Note: JsonConsoleFormatter is not available in .NET 9, using simple formatter instead
                    builder.AddConsole();
                }

                // Add custom test logging provider
                builder.AddProvider(new TestLoggingProvider(testSettings));
            });
        }
        else
        {
            // Minimal logging for performance testing
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddConsole();
            });
        }

        return services;
    }

    /// <summary>
    /// Configures logging with custom configuration
    /// </summary>
    public static IServiceCollection AddTestLogging(
        this IServiceCollection services,
        Action<ILoggingBuilder> configureLogging)
    {
        services.ClearLogging();
        services.AddLogging(configureLogging);
        return services;
    }

    /// <summary>
    /// Clears existing logging configuration
    /// </summary>
    private static IServiceCollection ClearLogging(this IServiceCollection services)
    {
        var loggingDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ILoggerFactory));
        if (loggingDescriptor != null)
        {
            services.Remove(loggingDescriptor);
        }

        var loggingBuilderDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ILoggingBuilder));
        if (loggingBuilderDescriptor != null)
        {
            services.Remove(loggingBuilderDescriptor);
        }

        return services;
    }

    /// <summary>
    /// Converts string log level to LogLevel enum
    /// </summary>
    private static LogLevel GetLogLevel(string level)
    {
        return level?.ToLowerInvariant() switch
        {
            "trace" => LogLevel.Trace,
            "debug" => LogLevel.Debug,
            "information" => LogLevel.Information,
            "warning" => LogLevel.Warning,
            "error" => LogLevel.Error,
            "critical" => LogLevel.Critical,
            "none" => LogLevel.None,
            _ => LogLevel.Information
        };
    }
}