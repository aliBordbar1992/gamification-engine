using GamificationEngine.Domain.Leaderboards;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Users;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of leaderboard repository
/// </summary>
public class InMemoryLeaderboardRepository : ILeaderboardRepository
{
    private readonly IUserStateRepository _userStateRepository;
    private readonly Dictionary<string, LeaderboardResult> _cache = new();

    public InMemoryLeaderboardRepository(IUserStateRepository userStateRepository)
    {
        _userStateRepository = userStateRepository ?? throw new ArgumentNullException(nameof(userStateRepository));
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
        // Get all user states
        var allUsers = await GetAllUserStatesAsync(cancellationToken);

        // Filter users based on time range if needed
        var filteredUsers = await FilterUsersByTimeRangeAsync(allUsers, query, cancellationToken);

        // Generate entries based on leaderboard type
        var entries = GenerateEntries(filteredUsers, query);

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

    private async Task<IEnumerable<UserState>> FilterUsersByTimeRangeAsync(IEnumerable<UserState> users, LeaderboardQuery query, CancellationToken cancellationToken)
    {
        // For time-based filtering, we would need to check when users earned their points/badges/trophies
        // This would require additional data structures or queries to the reward history
        // For now, we'll return all users
        return users;
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
