using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Leaderboards;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for leaderboard operations
/// </summary>
public interface ILeaderboardService
{
    /// <summary>
    /// Gets leaderboard data based on the provided query
    /// </summary>
    Task<Result<LeaderboardDto, string>> GetLeaderboardAsync(LeaderboardQueryDto query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets leaderboard data for a specific point category
    /// </summary>
    Task<Result<LeaderboardDto, string>> GetPointsLeaderboardAsync(string category, string timeRange = TimeRange.AllTime, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets leaderboard data for badges
    /// </summary>
    Task<Result<LeaderboardDto, string>> GetBadgesLeaderboardAsync(string timeRange = TimeRange.AllTime, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets leaderboard data for trophies
    /// </summary>
    Task<Result<LeaderboardDto, string>> GetTrophiesLeaderboardAsync(string timeRange = TimeRange.AllTime, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets leaderboard data for levels
    /// </summary>
    Task<Result<LeaderboardDto, string>> GetLevelsLeaderboardAsync(string category, string timeRange = TimeRange.AllTime, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's rank in a specific leaderboard
    /// </summary>
    Task<Result<UserRankDto, string>> GetUserRankAsync(string userId, LeaderboardQueryDto query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes leaderboard cache for better performance
    /// </summary>
    Task<Result<bool, string>> RefreshLeaderboardCacheAsync(LeaderboardQueryDto query, CancellationToken cancellationToken = default);
}
