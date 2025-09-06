using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Leaderboards;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for leaderboard operations
/// </summary>
public sealed class LeaderboardService : ILeaderboardService
{
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly IUserStateRepository _userStateRepository;
    private readonly IPointCategoryRepository _pointCategoryRepository;
    private readonly ILevelRepository _levelRepository;

    public LeaderboardService(
        ILeaderboardRepository leaderboardRepository,
        IUserStateRepository userStateRepository,
        IPointCategoryRepository pointCategoryRepository,
        ILevelRepository levelRepository)
    {
        _leaderboardRepository = leaderboardRepository ?? throw new ArgumentNullException(nameof(leaderboardRepository));
        _userStateRepository = userStateRepository ?? throw new ArgumentNullException(nameof(userStateRepository));
        _pointCategoryRepository = pointCategoryRepository ?? throw new ArgumentNullException(nameof(pointCategoryRepository));
        _levelRepository = levelRepository ?? throw new ArgumentNullException(nameof(levelRepository));
    }

    public async Task<Result<LeaderboardDto, string>> GetLeaderboardAsync(LeaderboardQueryDto queryDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate query
            var validationResult = ValidateQuery(queryDto);
            if (!validationResult.IsSuccess)
                return Result.Failure<LeaderboardDto, string>(validationResult.Error);

            // Convert DTO to domain query
            var domainQuery = MapToDomainQuery(queryDto);

            // Get leaderboard data
            var result = await _leaderboardRepository.GetLeaderboardAsync(domainQuery, cancellationToken);

            // Convert to DTO
            var dto = MapToDto(result);
            return Result.Success<LeaderboardDto, string>(dto);
        }
        catch (Exception ex)
        {
            return Result.Failure<LeaderboardDto, string>($"Failed to get leaderboard: {ex.Message}");
        }
    }

    public async Task<Result<LeaderboardDto, string>> GetPointsLeaderboardAsync(string category, string timeRange = TimeRange.AllTime, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Points,
            Category = category,
            TimeRange = timeRange,
            Page = page,
            PageSize = pageSize
        };

        return await GetLeaderboardAsync(query, cancellationToken);
    }

    public async Task<Result<LeaderboardDto, string>> GetBadgesLeaderboardAsync(string timeRange = TimeRange.AllTime, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Badges,
            TimeRange = timeRange,
            Page = page,
            PageSize = pageSize
        };

        return await GetLeaderboardAsync(query, cancellationToken);
    }

    public async Task<Result<LeaderboardDto, string>> GetTrophiesLeaderboardAsync(string timeRange = TimeRange.AllTime, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Trophies,
            TimeRange = timeRange,
            Page = page,
            PageSize = pageSize
        };

        return await GetLeaderboardAsync(query, cancellationToken);
    }

    public async Task<Result<LeaderboardDto, string>> GetLevelsLeaderboardAsync(string category, string timeRange = TimeRange.AllTime, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Level,
            Category = category,
            TimeRange = timeRange,
            Page = page,
            PageSize = pageSize
        };

        return await GetLeaderboardAsync(query, cancellationToken);
    }

    public async Task<Result<UserRankDto, string>> GetUserRankAsync(string userId, LeaderboardQueryDto queryDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result.Failure<UserRankDto, string>("UserId cannot be empty");

            // Validate query
            var validationResult = ValidateQuery(queryDto);
            if (!validationResult.IsSuccess)
                return Result.Failure<UserRankDto, string>(validationResult.Error);

            // Convert DTO to domain query
            var domainQuery = MapToDomainQuery(queryDto);

            // Get user rank
            var rank = await _leaderboardRepository.GetUserRankAsync(userId, domainQuery, cancellationToken);

            // Get user state for additional info
            var userState = await _userStateRepository.GetByUserIdAsync(userId, cancellationToken);

            var dto = new UserRankDto
            {
                UserId = userId,
                Rank = rank,
                Points = GetUserPoints(userState, domainQuery),
                IsInLeaderboard = rank.HasValue
            };

            return Result.Success<UserRankDto, string>(dto);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserRankDto, string>($"Failed to get user rank: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> RefreshLeaderboardCacheAsync(LeaderboardQueryDto queryDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate query
            var validationResult = ValidateQuery(queryDto);
            if (!validationResult.IsSuccess)
                return Result.Failure<bool, string>(validationResult.Error);

            // Convert DTO to domain query
            var domainQuery = MapToDomainQuery(queryDto);

            // Refresh cache
            await _leaderboardRepository.RefreshCacheAsync(domainQuery, cancellationToken);

            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to refresh leaderboard cache: {ex.Message}");
        }
    }

    private Result<bool, string> ValidateQuery(LeaderboardQueryDto query)
    {
        if (!LeaderboardType.IsValid(query.Type))
            return Result.Failure<bool, string>($"Invalid leaderboard type: {query.Type}");

        if (!TimeRange.IsValid(query.TimeRange))
            return Result.Failure<bool, string>($"Invalid time range: {query.TimeRange}");

        if (query.Page < 1)
            return Result.Failure<bool, string>("Page must be at least 1");

        if (query.PageSize < 1 || query.PageSize > 1000)
            return Result.Failure<bool, string>("Page size must be between 1 and 1000");

        // Validate category requirement
        if (query.Type == LeaderboardType.Points || query.Type == LeaderboardType.Level)
        {
            if (string.IsNullOrWhiteSpace(query.Category))
                return Result.Failure<bool, string>($"Category is required for {query.Type} leaderboard");
        }

        return Result.Success<bool, string>(true);
    }

    private LeaderboardQuery MapToDomainQuery(LeaderboardQueryDto dto)
    {
        return new LeaderboardQuery(
            dto.Type,
            dto.Category,
            dto.TimeRange,
            dto.Page,
            dto.PageSize,
            dto.ReferenceDate
        );
    }

    private LeaderboardDto MapToDto(LeaderboardResult result)
    {
        return new LeaderboardDto
        {
            Query = new LeaderboardQueryDto
            {
                Type = result.Query.Type,
                Category = result.Query.Category,
                TimeRange = result.Query.TimeRange,
                Page = result.Query.Page,
                PageSize = result.Query.PageSize,
                ReferenceDate = result.Query.ReferenceDate
            },
            Entries = result.Entries.Select(MapEntryToDto),
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages,
            CurrentPage = result.CurrentPage,
            PageSize = result.PageSize,
            HasNextPage = result.HasNextPage,
            HasPreviousPage = result.HasPreviousPage,
            TopEntry = result.TopEntry != null ? MapEntryToDto(result.TopEntry) : null
        };
    }

    private LeaderboardEntryDto MapEntryToDto(LeaderboardEntry entry)
    {
        return new LeaderboardEntryDto
        {
            UserId = entry.UserId,
            Points = entry.Points,
            Rank = entry.Rank,
            DisplayName = entry.DisplayName
        };
    }

    private long GetUserPoints(Domain.Users.UserState? userState, LeaderboardQuery query)
    {
        if (userState == null)
            return 0;

        return query.Type switch
        {
            LeaderboardType.Points => userState.PointsByCategory.TryGetValue(query.Category ?? "", out var points) ? points : 0,
            LeaderboardType.Badges => userState.Badges.Count,
            LeaderboardType.Trophies => userState.Trophies.Count,
            LeaderboardType.Level => userState.PointsByCategory.TryGetValue(query.Category ?? "", out var levelPoints) ? levelPoints : 0,
            _ => 0
        };
    }
}
