using GamificationEngine.Domain.Leaderboards;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Domain.Users;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of leaderboard repository
/// </summary>
public class InMemoryLeaderboardRepository : ILeaderboardRepository
{
    private readonly IUserStateRepository _userStateRepository;
    private readonly IRewardHistoryRepository _rewardHistoryRepository;
    private readonly Dictionary<string, LeaderboardResult> _cache = new();

    public InMemoryLeaderboardRepository(IUserStateRepository userStateRepository, IRewardHistoryRepository rewardHistoryRepository)
    {
        _userStateRepository = userStateRepository ?? throw new ArgumentNullException(nameof(userStateRepository));
        _rewardHistoryRepository = rewardHistoryRepository ?? throw new ArgumentNullException(nameof(rewardHistoryRepository));
    }

    public async Task<LeaderboardResult> GetLeaderboardAsync(LeaderboardQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(query);

        // Check cache first
        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            return ApplyPagination(cachedResult, query);
        }

        // Generate leaderboard data
        var result = await GenerateLeaderboardAsync(query, cancellationToken);

        // Cache the result
        _cache[cacheKey] = result;

        return ApplyPagination(result, query);
    }

    public async Task<int> GetTotalCountAsync(LeaderboardQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(query);

        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            return cachedResult.TotalCount;
        }

        // Generate leaderboard data to get count
        var result = await GenerateLeaderboardAsync(query, cancellationToken);
        _cache[cacheKey] = result;

        return result.TotalCount;
    }

    public async Task<int?> GetUserRankAsync(string userId, LeaderboardQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(query);

        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            return cachedResult.GetUserEntry(userId)?.Rank;
        }

        // Generate leaderboard data
        var result = await GenerateLeaderboardAsync(query, cancellationToken);
        _cache[cacheKey] = result;

        return result.GetUserEntry(userId)?.Rank;
    }

    public async Task<IEnumerable<LeaderboardEntry>> GetUserContextAsync(string userId, LeaderboardQuery query, int contextSize = 5, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(query);

        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            return GetUserContextFromResult(cachedResult, userId, contextSize);
        }

        // Generate leaderboard data
        var result = await GenerateLeaderboardAsync(query, cancellationToken);
        _cache[cacheKey] = result;

        return GetUserContextFromResult(result, userId, contextSize);
    }

    public Task RefreshCacheAsync(LeaderboardQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(query);
        _cache.Remove(cacheKey);
        return Task.CompletedTask;
    }

    public Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        _cache.Clear();
        return Task.CompletedTask;
    }

    private async Task<LeaderboardResult> GenerateLeaderboardAsync(LeaderboardQuery query, CancellationToken cancellationToken)
    {
        // For time-based filtering, we need to calculate rewards earned within the time range
        if (query.TimeRange != TimeRange.AllTime)
        {
            return await GenerateTimeFilteredLeaderboardAsync(query, cancellationToken);
        }

        // For "alltime", use the existing logic with current user states
        var allUsers = await GetAllUserStatesAsync(cancellationToken);
        var entries = GenerateEntries(allUsers, query);
        var rankedEntries = RankEntries(entries, query);

        // Calculate pagination info
        var totalCount = rankedEntries.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        return new LeaderboardResult(query, rankedEntries, totalCount, totalPages);
    }

    private async Task<LeaderboardResult> GenerateTimeFilteredLeaderboardAsync(LeaderboardQuery query, CancellationToken cancellationToken)
    {
        // Get reward history for the time range
        var rewardHistories = await _rewardHistoryRepository.GetByDateRangeAsync(
            query.StartDate,
            query.EndDate,
            cancellationToken);

        // Filter to only successful rewards
        var successfulRewards = rewardHistories.Where(rh => rh.Success).ToList();

        // Calculate aggregated values for each user within the time range
        var userAggregates = CalculateUserAggregatesForTimeRange(successfulRewards, query);

        // Generate entries based on leaderboard type
        var entries = GenerateEntriesFromAggregates(userAggregates, query);

        // Sort and rank entries
        var rankedEntries = RankEntries(entries, query);

        // Calculate pagination info
        var totalCount = rankedEntries.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        return new LeaderboardResult(query, rankedEntries, totalCount, totalPages);
    }

    private async Task<IEnumerable<UserState>> GetAllUserStatesAsync(CancellationToken cancellationToken)
    {
        return await _userStateRepository.GetAllAsync(cancellationToken);
    }

    private Dictionary<string, UserTimeRangeAggregate> CalculateUserAggregatesForTimeRange(IEnumerable<RewardHistory> rewards, LeaderboardQuery query)
    {
        var userAggregates = new Dictionary<string, UserTimeRangeAggregate>();

        foreach (var reward in rewards)
        {
            if (!userAggregates.ContainsKey(reward.UserId))
            {
                userAggregates[reward.UserId] = new UserTimeRangeAggregate(reward.UserId);
            }

            var aggregate = userAggregates[reward.UserId];

            switch (reward.RewardType.ToLowerInvariant())
            {
                case "points":
                    if (reward.Details.TryGetValue("amount", out var amountObj) &&
                        reward.Details.TryGetValue("category", out var categoryObj))
                    {
                        var amount = Convert.ToInt64(amountObj);
                        var category = categoryObj.ToString() ?? "default";
                        aggregate.AddPoints(category, amount);
                    }
                    break;

                case "badge":
                    if (reward.Details.TryGetValue("badgeId", out var badgeIdObj))
                    {
                        var badgeId = badgeIdObj.ToString() ?? "";
                        aggregate.AddBadge(badgeId);
                    }
                    break;

                case "trophy":
                    if (reward.Details.TryGetValue("trophyId", out var trophyIdObj))
                    {
                        var trophyId = trophyIdObj.ToString() ?? "";
                        aggregate.AddTrophy(trophyId);
                    }
                    break;

                case "penalty":
                    if (reward.Details.TryGetValue("penaltyType", out var penaltyTypeObj) &&
                        reward.Details.TryGetValue("amount", out var penaltyAmountObj))
                    {
                        var penaltyType = penaltyTypeObj.ToString() ?? "";
                        var penaltyAmount = Convert.ToInt64(penaltyAmountObj);

                        if (penaltyType == "points" && reward.Details.TryGetValue("targetId", out var targetIdObj))
                        {
                            var category = targetIdObj.ToString() ?? "default";
                            aggregate.AddPoints(category, -penaltyAmount); // Negative for penalty
                        }
                    }
                    break;
            }
        }

        return userAggregates;
    }

    private IEnumerable<LeaderboardEntry> GenerateEntriesFromAggregates(Dictionary<string, UserTimeRangeAggregate> userAggregates, LeaderboardQuery query)
    {
        return query.Type switch
        {
            LeaderboardType.Points => GeneratePointsEntriesFromAggregates(userAggregates, query.Category!),
            LeaderboardType.Badges => GenerateBadgesEntriesFromAggregates(userAggregates),
            LeaderboardType.Trophies => GenerateTrophiesEntriesFromAggregates(userAggregates),
            LeaderboardType.Level => GenerateLevelEntriesFromAggregates(userAggregates, query.Category!),
            _ => throw new ArgumentException($"Unsupported leaderboard type: {query.Type}")
        };
    }

    private IEnumerable<LeaderboardEntry> GeneratePointsEntriesFromAggregates(Dictionary<string, UserTimeRangeAggregate> userAggregates, string category)
    {
        return userAggregates.Values
            .Where(aggregate => aggregate.PointsByCategory.ContainsKey(category))
            .Select(aggregate => new LeaderboardEntry(aggregate.UserId, aggregate.PointsByCategory[category], 1))
            .Where(entry => entry.Points > 0);
    }

    private IEnumerable<LeaderboardEntry> GenerateBadgesEntriesFromAggregates(Dictionary<string, UserTimeRangeAggregate> userAggregates)
    {
        return userAggregates.Values
            .Where(aggregate => aggregate.Badges.Count > 0)
            .Select(aggregate => new LeaderboardEntry(aggregate.UserId, aggregate.Badges.Count, 1));
    }

    private IEnumerable<LeaderboardEntry> GenerateTrophiesEntriesFromAggregates(Dictionary<string, UserTimeRangeAggregate> userAggregates)
    {
        return userAggregates.Values
            .Where(aggregate => aggregate.Trophies.Count > 0)
            .Select(aggregate => new LeaderboardEntry(aggregate.UserId, aggregate.Trophies.Count, 1));
    }

    private IEnumerable<LeaderboardEntry> GenerateLevelEntriesFromAggregates(Dictionary<string, UserTimeRangeAggregate> userAggregates, string category)
    {
        return userAggregates.Values
            .Where(aggregate => aggregate.PointsByCategory.ContainsKey(category))
            .Select(aggregate => new LeaderboardEntry(aggregate.UserId, aggregate.PointsByCategory[category], 1))
            .Where(entry => entry.Points > 0);
    }

    private IEnumerable<LeaderboardEntry> GenerateEntries(IEnumerable<UserState> users, LeaderboardQuery query)
    {
        return query.Type switch
        {
            LeaderboardType.Points => GeneratePointsEntries(users, query.Category!),
            LeaderboardType.Badges => GenerateBadgesEntries(users),
            LeaderboardType.Trophies => GenerateTrophiesEntries(users),
            LeaderboardType.Level => GenerateLevelEntries(users, query.Category!),
            _ => throw new ArgumentException($"Unsupported leaderboard type: {query.Type}")
        };
    }

    private IEnumerable<LeaderboardEntry> GeneratePointsEntries(IEnumerable<UserState> users, string category)
    {
        return users
            .Where(u => u.PointsByCategory.ContainsKey(category))
            .Select(u => new LeaderboardEntry(u.UserId, u.PointsByCategory[category], 1))
            .Where(e => e.Points > 0);
    }

    private IEnumerable<LeaderboardEntry> GenerateBadgesEntries(IEnumerable<UserState> users)
    {
        return users
            .Where(u => u.Badges.Count > 0)
            .Select(u => new LeaderboardEntry(u.UserId, u.Badges.Count, 1));
    }

    private IEnumerable<LeaderboardEntry> GenerateTrophiesEntries(IEnumerable<UserState> users)
    {
        return users
            .Where(u => u.Trophies.Count > 0)
            .Select(u => new LeaderboardEntry(u.UserId, u.Trophies.Count, 1));
    }

    private IEnumerable<LeaderboardEntry> GenerateLevelEntries(IEnumerable<UserState> users, string category)
    {
        return users
            .Where(u => u.PointsByCategory.ContainsKey(category))
            .Select(u => new LeaderboardEntry(u.UserId, u.PointsByCategory[category], 1))
            .Where(e => e.Points > 0);
    }

    private List<LeaderboardEntry> RankEntries(IEnumerable<LeaderboardEntry> entries, LeaderboardQuery query)
    {
        var sortedEntries = query.Type switch
        {
            LeaderboardType.Points or LeaderboardType.Level => entries.OrderByDescending(e => e.Points),
            LeaderboardType.Badges or LeaderboardType.Trophies => entries.OrderByDescending(e => e.Points),
            _ => entries.OrderByDescending(e => e.Points)
        };

        var rankedEntries = new List<LeaderboardEntry>();
        var rank = 1;

        foreach (var entry in sortedEntries)
        {
            var rankedEntry = new LeaderboardEntry(entry.UserId, entry.Points, rank, entry.DisplayName);
            rankedEntries.Add(rankedEntry);
            rank++;
        }

        return rankedEntries;
    }

    private LeaderboardResult ApplyPagination(LeaderboardResult result, LeaderboardQuery query)
    {
        var paginatedEntries = result.Entries
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToList();

        return new LeaderboardResult(query, paginatedEntries, result.TotalCount, result.TotalPages);
    }

    private IEnumerable<LeaderboardEntry> GetUserContextFromResult(LeaderboardResult result, string userId, int contextSize)
    {
        var userEntry = result.GetUserEntry(userId);
        if (userEntry == null)
            return new List<LeaderboardEntry>();

        var userRank = userEntry.Rank;
        var startRank = Math.Max(1, userRank - contextSize / 2);
        var endRank = Math.Min(result.TotalCount, startRank + contextSize - 1);

        return result.Entries
            .Where(e => e.Rank >= startRank && e.Rank <= endRank)
            .OrderBy(e => e.Rank);
    }

    private static string GetCacheKey(LeaderboardQuery query)
    {
        return $"{query.Type}:{query.Category}:{query.TimeRange}:{query.ReferenceDate:yyyy-MM-dd}";
    }
}
