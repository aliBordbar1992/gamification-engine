namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data transfer object for user summary information
/// </summary>
public sealed class UserSummaryDto
{
    public string UserId { get; set; } = string.Empty;
    public long TotalPoints { get; set; }
    public int BadgeCount { get; set; }
    public int TrophyCount { get; set; }
    public Dictionary<string, long> PointsByCategory { get; set; } = new();
    public Dictionary<string, LevelDto> CurrentLevelsByCategory { get; set; } = new();
}

/// <summary>
/// Data transfer object for paginated user summaries
/// </summary>
public sealed class UserSummariesDto
{
    public IEnumerable<UserSummaryDto> Users { get; set; } = new List<UserSummaryDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
