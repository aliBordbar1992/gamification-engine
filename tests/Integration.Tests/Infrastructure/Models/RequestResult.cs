namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Result of a single request in a load test
/// </summary>
public class RequestResult
{
    public int RequestId { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public Exception? Exception { get; set; }
}