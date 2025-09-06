namespace GamificationEngine.Domain.Leaderboards;

/// <summary>
/// Represents the result of a leaderboard query
/// </summary>
public sealed class LeaderboardResult
{
    public LeaderboardResult(
        LeaderboardQuery query,
        IEnumerable<LeaderboardEntry> entries,
        int totalCount,
        int totalPages)
    {
        Query = query ?? throw new ArgumentNullException(nameof(query));
        Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        TotalCount = totalCount;
        TotalPages = totalPages;
    }

    public LeaderboardQuery Query { get; }
    public IEnumerable<LeaderboardEntry> Entries { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public int CurrentPage => Query.Page;
    public int PageSize => Query.PageSize;
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Gets the top entry (highest rank)
    /// </summary>
    public LeaderboardEntry? TopEntry => Entries.FirstOrDefault();

    /// <summary>
    /// Gets the user's rank if they exist in the leaderboard
    /// </summary>
    public LeaderboardEntry? GetUserEntry(string userId)
    {
        return Entries.FirstOrDefault(e => e.UserId == userId);
    }

    /// <summary>
    /// Validates the leaderboard result
    /// </summary>
    public bool IsValid()
    {
        return Query.IsValid() &&
               Entries.All(e => e.IsValid()) &&
               TotalCount >= 0 &&
               TotalPages >= 0 &&
               CurrentPage >= 1 &&
               PageSize >= 1;
    }
}
