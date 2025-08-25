namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Information about the current database state
/// </summary>
public class DatabaseStateInfo
{
    /// <summary>
    /// Number of events in the database
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// Number of user states in the database
    /// </summary>
    public int UserStateCount { get; set; }

    /// <summary>
    /// When this state info was captured
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}