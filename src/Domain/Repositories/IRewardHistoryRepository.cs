using GamificationEngine.Domain.Rewards;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository for storing and retrieving reward history
/// </summary>
public interface IRewardHistoryRepository
{
    /// <summary>
    /// Stores a new reward history record
    /// </summary>
    /// <param name="rewardHistory">The reward history to store</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task StoreAsync(RewardHistory rewardHistory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves reward history for a specific user
    /// </summary>
    /// <param name="userId">The user ID to retrieve history for</param>
    /// <param name="limit">Maximum number of records to retrieve</param>
    /// <param name="offset">Number of records to skip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of reward history records</returns>
    Task<IEnumerable<RewardHistory>> GetByUserIdAsync(string userId, int limit = 100, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves reward history for a specific reward type
    /// </summary>
    /// <param name="userId">The user ID to retrieve history for</param>
    /// <param name="rewardType">The reward type to filter by</param>
    /// <param name="limit">Maximum number of records to retrieve</param>
    /// <param name="offset">Number of records to skip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of reward history records</returns>
    Task<IEnumerable<RewardHistory>> GetByUserIdAndRewardTypeAsync(string userId, string rewardType, int limit = 100, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves reward history for a specific time range
    /// </summary>
    /// <param name="userId">The user ID to retrieve history for</param>
    /// <param name="startDate">Start date for the range</param>
    /// <param name="endDate">End date for the range</param>
    /// <param name="limit">Maximum number of records to retrieve</param>
    /// <param name="offset">Number of records to skip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of reward history records</returns>
    Task<IEnumerable<RewardHistory>> GetByUserIdAndDateRangeAsync(string userId, DateTimeOffset startDate, DateTimeOffset endDate, int limit = 100, int offset = 0, CancellationToken cancellationToken = default);
}
