using GamificationEngine.Integration.Tests.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

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