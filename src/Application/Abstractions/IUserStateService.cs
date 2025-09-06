using GamificationEngine.Application.DTOs;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for managing user state (points, badges, trophies, levels)
/// </summary>
public interface IUserStateService
{
    /// <summary>
    /// Gets the complete user state including points, badges, trophies, and current level
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete user state</returns>
    Task<Result<UserStateDto, string>> GetUserStateAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user points for all categories
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User points by category</returns>
    Task<Result<Dictionary<string, long>, string>> GetUserPointsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user points for a specific category
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="category">The point category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User points for the category</returns>
    Task<Result<long, string>> GetUserPointsForCategoryAsync(string userId, string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user badges
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of user badges</returns>
    Task<Result<IEnumerable<BadgeDto>, string>> GetUserBadgesAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user trophies
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of user trophies</returns>
    Task<Result<IEnumerable<TrophyDto>, string>> GetUserTrophiesAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user current level for a specific category
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="category">The point category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current level for the category</returns>
    Task<Result<LevelDto?, string>> GetUserCurrentLevelAsync(string userId, string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user current levels for all categories
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current levels by category</returns>
    Task<Result<Dictionary<string, LevelDto>, string>> GetUserCurrentLevelsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user reward history
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of entries per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User reward history</returns>
    Task<Result<UserRewardHistoryDto, string>> GetUserRewardHistoryAsync(string userId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
}
