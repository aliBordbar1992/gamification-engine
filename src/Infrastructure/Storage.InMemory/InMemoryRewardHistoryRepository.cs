using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rewards;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of reward history repository
/// </summary>
public class InMemoryRewardHistoryRepository : IRewardHistoryRepository
{
    private readonly List<RewardHistory> _rewardHistories = new();

    public Task StoreAsync(RewardHistory rewardHistory, CancellationToken cancellationToken = default)
    {
        if (rewardHistory == null) throw new ArgumentNullException(nameof(rewardHistory));

        _rewardHistories.Add(rewardHistory);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<RewardHistory>> GetByUserIdAsync(string userId, int limit = 100, int offset = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId cannot be empty", nameof(userId));

        var histories = _rewardHistories
            .Where(rh => rh.UserId == userId)
            .OrderByDescending(rh => rh.AwardedAt)
            .Skip(offset)
            .Take(limit);

        return Task.FromResult<IEnumerable<RewardHistory>>(histories);
    }

    public Task<IEnumerable<RewardHistory>> GetByUserIdAndRewardTypeAsync(string userId, string rewardType, int limit = 100, int offset = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(rewardType)) throw new ArgumentException("rewardType cannot be empty", nameof(rewardType));

        var histories = _rewardHistories
            .Where(rh => rh.UserId == userId && rh.RewardType == rewardType)
            .OrderByDescending(rh => rh.AwardedAt)
            .Skip(offset)
            .Take(limit);

        return Task.FromResult<IEnumerable<RewardHistory>>(histories);
    }

    public Task<IEnumerable<RewardHistory>> GetByUserIdAndDateRangeAsync(string userId, DateTimeOffset startDate, DateTimeOffset endDate, int limit = 100, int offset = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId cannot be empty", nameof(userId));

        var histories = _rewardHistories
            .Where(rh => rh.UserId == userId && rh.AwardedAt >= startDate && rh.AwardedAt <= endDate)
            .OrderByDescending(rh => rh.AwardedAt)
            .Skip(offset)
            .Take(limit);

        return Task.FromResult<IEnumerable<RewardHistory>>(histories);
    }

    /// <summary>
    /// Gets all reward histories for a specific time range (used for leaderboard filtering)
    /// </summary>
    public Task<IEnumerable<RewardHistory>> GetByDateRangeAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        var histories = _rewardHistories
            .Where(rh => rh.AwardedAt >= startDate && rh.AwardedAt <= endDate)
            .OrderByDescending(rh => rh.AwardedAt);

        return Task.FromResult<IEnumerable<RewardHistory>>(histories);
    }

    /// <summary>
    /// Gets reward histories for multiple users within a time range (used for leaderboard filtering)
    /// </summary>
    public Task<IEnumerable<RewardHistory>> GetByUserIdsAndDateRangeAsync(IEnumerable<string> userIds, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        if (userIds == null) throw new ArgumentNullException(nameof(userIds));

        var userIdSet = userIds.ToHashSet();
        var histories = _rewardHistories
            .Where(rh => userIdSet.Contains(rh.UserId) && rh.AwardedAt >= startDate && rh.AwardedAt <= endDate)
            .OrderByDescending(rh => rh.AwardedAt);

        return Task.FromResult<IEnumerable<RewardHistory>>(histories);
    }
}
