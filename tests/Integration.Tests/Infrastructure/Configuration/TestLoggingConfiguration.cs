using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

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

/// <summary>
/// Custom logging provider for test scenarios
/// </summary>
public class TestLoggingProvider : ILoggerProvider
{
    private readonly TestSettings _testSettings;
    private readonly List<TestLogEntry> _logEntries;

    public TestLoggingProvider(TestSettings testSettings)
    {
        _testSettings = testSettings;
        _logEntries = new List<TestLogEntry>();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(categoryName, _testSettings, _logEntries);
    }

    public void Dispose()
    {
        // Cleanup resources if needed
    }

    /// <summary>
    /// Gets all log entries captured during testing
    /// </summary>
    public IReadOnlyList<TestLogEntry> GetLogEntries()
    {
        return _logEntries.AsReadOnly();
    }

    /// <summary>
    /// Clears all captured log entries
    /// </summary>
    public void ClearLogEntries()
    {
        _logEntries.Clear();
    }

    /// <summary>
    /// Gets log entries for a specific category
    /// </summary>
    public IEnumerable<TestLogEntry> GetLogEntriesForCategory(string category)
    {
        return _logEntries.Where(e => e.Category == category);
    }

    /// <summary>
    /// Gets log entries for a specific log level
    /// </summary>
    public IEnumerable<TestLogEntry> GetLogEntriesForLevel(LogLevel level)
    {
        return _logEntries.Where(e => e.LogLevel == level);
    }
}

/// <summary>
/// Test logger implementation that captures log entries for testing
/// </summary>
public class TestLogger : ILogger
{
    private readonly string _categoryName;
    private readonly TestSettings _testSettings;
    private readonly List<TestLogEntry> _logEntries;

    public TestLogger(string categoryName, TestSettings testSettings, List<TestLogEntry> logEntries)
    {
        _categoryName = categoryName;
        _testSettings = testSettings;
        _logEntries = logEntries;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _testSettings.EnableDetailedLogging || logLevel >= LogLevel.Warning;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var logEntry = new TestLogEntry
        {
            Timestamp = DateTime.UtcNow,
            LogLevel = logLevel,
            Category = _categoryName,
            EventId = eventId,
            Message = formatter(state, exception),
            Exception = exception,
            State = state
        };

        lock (_logEntries)
        {
            _logEntries.Add(logEntry);
        }

        // Also output to console for immediate feedback during testing
        if (_testSettings.EnableDetailedLogging)
        {
            var color = logLevel switch
            {
                LogLevel.Trace => ConsoleColor.Gray,
                LogLevel.Debug => ConsoleColor.DarkGray,
                LogLevel.Information => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.White
            };

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{logLevel}] {_categoryName}: {logEntry.Message}");
            if (exception != null)
            {
                Console.WriteLine($"Exception: {exception}");
            }
            Console.ForegroundColor = originalColor;
        }
    }
}

/// <summary>
/// Represents a captured log entry for testing purposes
/// </summary>
public class TestLogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel LogLevel { get; set; }
    public string Category { get; set; } = string.Empty;
    public EventId EventId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public object? State { get; set; }
}