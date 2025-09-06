namespace GamificationEngine.Domain.Leaderboards;

/// <summary>
/// Represents a query for retrieving leaderboard data
/// </summary>
public sealed class LeaderboardQuery
{
    public LeaderboardQuery(
        string type,
        string? category = null,
        string timeRange = "alltime",
        int page = 1,
        int pageSize = 50,
        DateTime? referenceDate = null)
    {
        if (!LeaderboardType.IsValid(type))
            throw new ArgumentException($"Invalid leaderboard type: {type}", nameof(type));

        if (!GamificationEngine.Domain.Leaderboards.TimeRange.IsValid(timeRange))
            throw new ArgumentException($"Invalid time range: {timeRange}", nameof(timeRange));

        if (page < 1)
            throw new ArgumentException("Page must be at least 1", nameof(page));

        if (pageSize < 1 || pageSize > 1000)
            throw new ArgumentException("Page size must be between 1 and 1000", nameof(pageSize));

        Type = type;
        Category = category;
        TimeRange = timeRange;
        Page = page;
        PageSize = pageSize;
        ReferenceDate = referenceDate ?? DateTime.UtcNow;
    }

    public string Type { get; }
    public string? Category { get; }
    public string TimeRange { get; }
    public int Page { get; }
    public int PageSize { get; }
    public DateTime ReferenceDate { get; }

    /// <summary>
    /// Gets the start date for this query
    /// </summary>
    public DateTime StartDate => GamificationEngine.Domain.Leaderboards.TimeRange.GetStartDate(TimeRange, ReferenceDate);

    /// <summary>
    /// Gets the end date for this query
    /// </summary>
    public DateTime EndDate => GamificationEngine.Domain.Leaderboards.TimeRange.GetEndDate(TimeRange, ReferenceDate);

    /// <summary>
    /// Gets the skip count for pagination
    /// </summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// Validates the query parameters
    /// </summary>
    public bool IsValid()
    {
        return LeaderboardType.IsValid(Type) &&
               GamificationEngine.Domain.Leaderboards.TimeRange.IsValid(TimeRange) &&
               Page >= 1 &&
               PageSize >= 1 && PageSize <= 1000;
    }
}
