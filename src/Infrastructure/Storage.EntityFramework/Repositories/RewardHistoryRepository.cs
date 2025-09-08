using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rewards;

namespace GamificationEngine.Infrastructure.Storage.EntityFramework.Repositories;

/// <summary>
/// EF Core implementation of RewardHistory repository with PostgreSQL support
/// </summary>
public class RewardHistoryRepository : IRewardHistoryRepository
{
    private readonly GamificationEngineDbContext _context;
    private readonly ILogger<RewardHistoryRepository> _logger;

    public RewardHistoryRepository(GamificationEngineDbContext context, ILogger<RewardHistoryRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StoreAsync(RewardHistory rewardHistory, CancellationToken cancellationToken = default)
    {
        try
        {
            if (rewardHistory == null) throw new ArgumentNullException(nameof(rewardHistory));

            _context.RewardHistories.Add(rewardHistory);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reward history for reward {RewardId} stored successfully", rewardHistory.RewardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing reward history for reward {RewardId}", rewardHistory?.RewardId);
            throw;
        }
    }

    public async Task<IEnumerable<RewardHistory>> GetByUserIdAsync(string userId, int limit = 100, int offset = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId cannot be empty", nameof(userId));

            var histories = await _context.RewardHistories
                .Where(rh => rh.UserId == userId)
                .OrderByDescending(rh => rh.AwardedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return histories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reward history for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<RewardHistory>> GetByUserIdAndRewardTypeAsync(string userId, string rewardType, int limit = 100, int offset = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(rewardType)) throw new ArgumentException("rewardType cannot be empty", nameof(rewardType));

            var histories = await _context.RewardHistories
                .Where(rh => rh.UserId == userId && rh.RewardType == rewardType)
                .OrderByDescending(rh => rh.AwardedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return histories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reward history for user {UserId} and reward type {RewardType}", userId, rewardType);
            throw;
        }
    }

    public async Task<IEnumerable<RewardHistory>> GetByUserIdAndDateRangeAsync(string userId, DateTimeOffset startDate, DateTimeOffset endDate, int limit = 100, int offset = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId cannot be empty", nameof(userId));

            var histories = await _context.RewardHistories
                .Where(rh => rh.UserId == userId && rh.AwardedAt >= startDate && rh.AwardedAt <= endDate)
                .OrderByDescending(rh => rh.AwardedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return histories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reward history for user {UserId} in date range {StartDate} to {EndDate}", userId, startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets all reward histories for a specific time range (used for leaderboard filtering)
    /// </summary>
    public async Task<IEnumerable<RewardHistory>> GetByDateRangeAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var histories = await _context.RewardHistories
                .Where(rh => rh.AwardedAt >= startDate && rh.AwardedAt <= endDate)
                .OrderByDescending(rh => rh.AwardedAt)
                .ToListAsync(cancellationToken);

            return histories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reward history in date range {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets reward histories for multiple users within a time range (used for leaderboard filtering)
    /// </summary>
    public async Task<IEnumerable<RewardHistory>> GetByUserIdsAndDateRangeAsync(IEnumerable<string> userIds, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            if (userIds == null) throw new ArgumentNullException(nameof(userIds));

            var userIdList = userIds.ToList();
            var histories = await _context.RewardHistories
                .Where(rh => userIdList.Contains(rh.UserId) && rh.AwardedAt >= startDate && rh.AwardedAt <= endDate)
                .OrderByDescending(rh => rh.AwardedAt)
                .ToListAsync(cancellationToken);

            return histories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reward history for users in date range {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }
}
