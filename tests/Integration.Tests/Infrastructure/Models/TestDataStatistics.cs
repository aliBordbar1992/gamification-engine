namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Statistics about test data usage
/// </summary>
public class TestDataStatistics
{
    /// <summary>
    /// Total number of events created
    /// </summary>
    public int TotalEvents { get; set; }

    /// <summary>
    /// Total number of user states created
    /// </summary>
    public int TotalUserStates { get; set; }

    /// <summary>
    /// Number of unique event types
    /// </summary>
    public int UniqueEventTypes { get; set; }

    /// <summary>
    /// Number of unique users
    /// </summary>
    public int UniqueUsers { get; set; }

    /// <summary>
    /// Time range of test data
    /// </summary>
    public TimeSpan DataTimeRange { get; set; }
}