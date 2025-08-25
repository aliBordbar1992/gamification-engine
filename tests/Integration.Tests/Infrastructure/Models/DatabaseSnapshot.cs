namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Represents a snapshot of the database state at a specific point in time
/// </summary>
public class DatabaseSnapshot
{
    /// <summary>
    /// Name of the snapshot
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of events in the snapshot
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// Number of user states in the snapshot
    /// </summary>
    public int UserStateCount { get; set; }

    /// <summary>
    /// When the snapshot was created
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// The actual events data (optional, for full state restoration)
    /// </summary>
    public List<Domain.Events.Event>? Events { get; set; }

    /// <summary>
    /// The actual user states data (optional, for full state restoration)
    /// </summary>
    public List<Domain.Users.UserState>? UserStates { get; set; }
}