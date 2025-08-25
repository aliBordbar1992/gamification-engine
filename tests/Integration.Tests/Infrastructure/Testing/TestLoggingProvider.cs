using GamificationEngine.Integration.Tests.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

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