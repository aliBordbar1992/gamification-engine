namespace GamificationEngine.Application.DTOs;

/// <summary>
/// DTO for leaderboard result
/// </summary>
public sealed class LeaderboardDto
{
    public LeaderboardQueryDto Query { get; set; } = new();
    public IEnumerable<LeaderboardEntryDto> Entries { get; set; } = new List<LeaderboardEntryDto>();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public LeaderboardEntryDto? TopEntry { get; set; }
}
