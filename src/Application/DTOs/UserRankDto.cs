namespace GamificationEngine.Application.DTOs;

/// <summary>
/// DTO for user rank information
/// </summary>
public sealed class UserRankDto
{
    public string UserId { get; set; } = string.Empty;
    public int? Rank { get; set; }
    public long Points { get; set; }
    public string? DisplayName { get; set; }
    public bool IsInLeaderboard { get; set; }
}
