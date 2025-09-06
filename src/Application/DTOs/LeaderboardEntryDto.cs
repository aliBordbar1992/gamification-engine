namespace GamificationEngine.Application.DTOs;

/// <summary>
/// DTO for a leaderboard entry
/// </summary>
public sealed class LeaderboardEntryDto
{
    public string UserId { get; set; } = string.Empty;
    public long Points { get; set; }
    public int Rank { get; set; }
    public string? DisplayName { get; set; }
}
