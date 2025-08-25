using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

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