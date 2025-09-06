namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data transfer object for complete user state
/// </summary>
public sealed class UserStateDto
{
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, long> PointsByCategory { get; set; } = new();
    public IEnumerable<BadgeDto> Badges { get; set; } = new List<BadgeDto>();
    public IEnumerable<TrophyDto> Trophies { get; set; } = new List<TrophyDto>();
    public Dictionary<string, LevelDto> CurrentLevelsByCategory { get; set; } = new();
}

/// <summary>
/// Data transfer object for user reward history entry
/// </summary>
public sealed class UserRewardHistoryEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string RewardType { get; set; } = string.Empty;
    public string RewardId { get; set; } = string.Empty;
    public string RewardName { get; set; } = string.Empty;
    public long? PointsAmount { get; set; }
    public string? PointCategory { get; set; }
    public DateTime AwardedAt { get; set; }
    public string? TriggerEventType { get; set; }
    public string? TriggerEventId { get; set; }
}

/// <summary>
/// Data transfer object for user reward history with pagination
/// </summary>
public sealed class UserRewardHistoryDto
{
    public IEnumerable<UserRewardHistoryEntryDto> Entries { get; set; } = new List<UserRewardHistoryEntryDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
