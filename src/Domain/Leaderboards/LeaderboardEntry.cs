namespace GamificationEngine.Domain.Leaderboards;

/// <summary>
/// Represents a single entry in a leaderboard
/// </summary>
public sealed class LeaderboardEntry
{
    // EF Core requires a parameterless constructor
    private LeaderboardEntry() { }

    public LeaderboardEntry(string userId, long points, int rank, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId cannot be empty", nameof(userId));
        if (points < 0) throw new ArgumentException("Points cannot be negative", nameof(points));
        if (rank < 1) throw new ArgumentException("Rank must be at least 1", nameof(rank));

        UserId = userId;
        Points = points;
        Rank = rank;
        DisplayName = displayName;
    }

    public string UserId { get; private set; } = string.Empty;
    public long Points { get; private set; }
    public int Rank { get; private set; }
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Updates the rank of this entry
    /// </summary>
    public void UpdateRank(int rank)
    {
        if (rank < 1) throw new ArgumentException("Rank must be at least 1", nameof(rank));
        Rank = rank;
    }

    /// <summary>
    /// Updates the points for this entry
    /// </summary>
    public void UpdatePoints(long points)
    {
        if (points < 0) throw new ArgumentException("Points cannot be negative", nameof(points));
        Points = points;
    }

    /// <summary>
    /// Updates the display name for this entry
    /// </summary>
    public void UpdateDisplayName(string? displayName)
    {
        DisplayName = displayName;
    }

    /// <summary>
    /// Validates the leaderboard entry
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               Points >= 0 &&
               Rank >= 1;
    }
}
