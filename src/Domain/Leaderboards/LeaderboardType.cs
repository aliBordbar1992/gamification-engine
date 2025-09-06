namespace GamificationEngine.Domain.Leaderboards;

/// <summary>
/// Represents the type of leaderboard (points, badges, trophies, etc.)
/// </summary>
public static class LeaderboardType
{
    public const string Points = "points";
    public const string Badges = "badges";
    public const string Trophies = "trophies";
    public const string Level = "level";

    /// <summary>
    /// Gets all valid leaderboard types
    /// </summary>
    public static readonly string[] AllTypes = { Points, Badges, Trophies, Level };

    /// <summary>
    /// Validates if a leaderboard type is valid
    /// </summary>
    public static bool IsValid(string type)
    {
        return !string.IsNullOrWhiteSpace(type) && AllTypes.Contains(type);
    }
}
