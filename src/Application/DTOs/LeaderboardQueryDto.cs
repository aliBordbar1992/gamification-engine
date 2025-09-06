namespace GamificationEngine.Application.DTOs;

/// <summary>
/// DTO for leaderboard query parameters
/// </summary>
public sealed class LeaderboardQueryDto
{
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string TimeRange { get; set; } = "alltime";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public DateTime? ReferenceDate { get; set; }
}
