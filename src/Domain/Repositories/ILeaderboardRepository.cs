using GamificationEngine.Domain.Leaderboards;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository interface for leaderboard data access
/// </summary>
public interface ILeaderboardRepository
{
    /// <summary>
    /// Gets leaderboard entries for a specific query
    /// </summary>
    Task<LeaderboardResult> GetLeaderboardAsync(LeaderboardQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of users for a leaderboard query (without pagination)
    /// </summary>
    Task<int> GetTotalCountAsync(LeaderboardQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's rank in a specific leaderboard
    /// </summary>
    Task<int?> GetUserRankAsync(string userId, LeaderboardQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets leaderboard entries around a specific user (for showing user's position in context)
    /// </summary>
    Task<IEnumerable<LeaderboardEntry>> GetUserContextAsync(string userId, LeaderboardQuery query, int contextSize = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the leaderboard cache for a specific query
    /// </summary>
    Task RefreshCacheAsync(LeaderboardQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all leaderboard caches
    /// </summary>
    Task ClearCacheAsync(CancellationToken cancellationToken = default);
}
