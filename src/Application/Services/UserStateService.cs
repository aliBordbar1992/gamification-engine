using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for managing user state (points, badges, trophies, levels)
/// </summary>
public class UserStateService : IUserStateService
{
    private readonly IUserStateRepository _userStateRepository;
    private readonly IBadgeRepository _badgeRepository;
    private readonly ITrophyRepository _trophyRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IRewardHistoryRepository _rewardHistoryRepository;

    public UserStateService(
        IUserStateRepository userStateRepository,
        IBadgeRepository badgeRepository,
        ITrophyRepository trophyRepository,
        ILevelRepository levelRepository,
        IRewardHistoryRepository rewardHistoryRepository)
    {
        _userStateRepository = userStateRepository ?? throw new ArgumentNullException(nameof(userStateRepository));
        _badgeRepository = badgeRepository ?? throw new ArgumentNullException(nameof(badgeRepository));
        _trophyRepository = trophyRepository ?? throw new ArgumentNullException(nameof(trophyRepository));
        _levelRepository = levelRepository ?? throw new ArgumentNullException(nameof(levelRepository));
        _rewardHistoryRepository = rewardHistoryRepository ?? throw new ArgumentNullException(nameof(rewardHistoryRepository));
    }

    public async Task<Result<UserStateDto, string>> GetUserStateAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<UserStateDto, string>.Failure("User ID cannot be empty");

            var userState = await _userStateRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userState == null)
            {
                // Return empty state for new users
                return Result<UserStateDto, string>.Success(new UserStateDto
                {
                    UserId = userId,
                    PointsByCategory = new Dictionary<string, long>(),
                    Badges = new List<BadgeDto>(),
                    Trophies = new List<TrophyDto>(),
                    CurrentLevelsByCategory = new Dictionary<string, LevelDto>()
                });
            }

            // Get badge and trophy details
            var badges = await GetUserBadgesAsync(userId, cancellationToken);
            var trophies = await GetUserTrophiesAsync(userId, cancellationToken);
            var currentLevels = await GetUserCurrentLevelsAsync(userId, cancellationToken);

            var userStateDto = new UserStateDto
            {
                UserId = userId,
                PointsByCategory = userState.PointsByCategory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Badges = badges.IsSuccess ? badges.Value : new List<BadgeDto>(),
                Trophies = trophies.IsSuccess ? trophies.Value : new List<TrophyDto>(),
                CurrentLevelsByCategory = currentLevels.IsSuccess ? currentLevels.Value : new Dictionary<string, LevelDto>()
            };

            return Result<UserStateDto, string>.Success(userStateDto);
        }
        catch (Exception ex)
        {
            return Result<UserStateDto, string>.Failure($"Failed to get user state: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, long>, string>> GetUserPointsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<Dictionary<string, long>, string>.Failure("User ID cannot be empty");

            var userState = await _userStateRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userState == null)
                return Result<Dictionary<string, long>, string>.Success(new Dictionary<string, long>());

            return Result<Dictionary<string, long>, string>.Success(
                userState.PointsByCategory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }
        catch (Exception ex)
        {
            return Result<Dictionary<string, long>, string>.Failure($"Failed to get user points: {ex.Message}");
        }
    }

    public async Task<Result<long, string>> GetUserPointsForCategoryAsync(string userId, string category, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<long, string>.Failure("User ID cannot be empty");

            if (string.IsNullOrWhiteSpace(category))
                return Result<long, string>.Failure("Category cannot be empty");

            var userState = await _userStateRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userState == null)
                return Result<long, string>.Success(0);

            var points = userState.PointsByCategory.TryGetValue(category, out var categoryPoints) ? categoryPoints : 0;
            return Result<long, string>.Success(points);
        }
        catch (Exception ex)
        {
            return Result<long, string>.Failure($"Failed to get user points for category: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BadgeDto>, string>> GetUserBadgesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<IEnumerable<BadgeDto>, string>.Failure("User ID cannot be empty");

            var userState = await _userStateRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userState == null)
                return Result<IEnumerable<BadgeDto>, string>.Success(new List<BadgeDto>());

            var badgeDtos = new List<BadgeDto>();
            foreach (var badgeId in userState.Badges)
            {
                var badge = await _badgeRepository.GetByIdAsync(badgeId, cancellationToken);
                if (badge != null)
                {
                    badgeDtos.Add(new BadgeDto
                    {
                        Id = badge.Id,
                        Name = badge.Name,
                        Description = badge.Description,
                        Image = badge.Image,
                        Visible = badge.Visible
                    });
                }
            }

            return Result<IEnumerable<BadgeDto>, string>.Success(badgeDtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<BadgeDto>, string>.Failure($"Failed to get user badges: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TrophyDto>, string>> GetUserTrophiesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<IEnumerable<TrophyDto>, string>.Failure("User ID cannot be empty");

            var userState = await _userStateRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userState == null)
                return Result<IEnumerable<TrophyDto>, string>.Success(new List<TrophyDto>());

            var trophyDtos = new List<TrophyDto>();
            foreach (var trophyId in userState.Trophies)
            {
                var trophy = await _trophyRepository.GetByIdAsync(trophyId, cancellationToken);
                if (trophy != null)
                {
                    trophyDtos.Add(new TrophyDto
                    {
                        Id = trophy.Id,
                        Name = trophy.Name,
                        Description = trophy.Description,
                        Image = trophy.Image,
                        Visible = trophy.Visible
                    });
                }
            }

            return Result<IEnumerable<TrophyDto>, string>.Success(trophyDtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TrophyDto>, string>.Failure($"Failed to get user trophies: {ex.Message}");
        }
    }

    public async Task<Result<LevelDto?, string>> GetUserCurrentLevelAsync(string userId, string category, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<LevelDto?, string>.Failure("User ID cannot be empty");

            if (string.IsNullOrWhiteSpace(category))
                return Result<LevelDto?, string>.Failure("Category cannot be empty");

            var userState = await _userStateRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userState == null)
                return Result<LevelDto?, string>.Success(null);

            var points = userState.PointsByCategory.TryGetValue(category, out var categoryPoints) ? categoryPoints : 0;
            var level = await _levelRepository.GetLevelForPointsAsync(category, points, cancellationToken);

            if (level == null)
                return Result<LevelDto?, string>.Success(null);

            return Result<LevelDto?, string>.Success(new LevelDto
            {
                Id = level.Id,
                Name = level.Name,
                Category = level.Category,
                MinPoints = level.MinPoints
            });
        }
        catch (Exception ex)
        {
            return Result<LevelDto?, string>.Failure($"Failed to get user current level: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, LevelDto>, string>> GetUserCurrentLevelsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<Dictionary<string, LevelDto>, string>.Failure("User ID cannot be empty");

            var userState = await _userStateRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userState == null)
                return Result<Dictionary<string, LevelDto>, string>.Success(new Dictionary<string, LevelDto>());

            var levelsByCategory = new Dictionary<string, LevelDto>();
            foreach (var categoryPoints in userState.PointsByCategory)
            {
                var level = await _levelRepository.GetLevelForPointsAsync(categoryPoints.Key, categoryPoints.Value, cancellationToken);
                if (level != null)
                {
                    levelsByCategory[categoryPoints.Key] = new LevelDto
                    {
                        Id = level.Id,
                        Name = level.Name,
                        Category = level.Category,
                        MinPoints = level.MinPoints
                    };
                }
            }

            return Result<Dictionary<string, LevelDto>, string>.Success(levelsByCategory);
        }
        catch (Exception ex)
        {
            return Result<Dictionary<string, LevelDto>, string>.Failure($"Failed to get user current levels: {ex.Message}");
        }
    }

    public async Task<Result<UserRewardHistoryDto, string>> GetUserRewardHistoryAsync(string userId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<UserRewardHistoryDto, string>.Failure("User ID cannot be empty");

            if (page < 1)
                return Result<UserRewardHistoryDto, string>.Failure("Page must be greater than 0");

            if (pageSize < 1 || pageSize > 1000)
                return Result<UserRewardHistoryDto, string>.Failure("Page size must be between 1 and 1000");

            // Calculate pagination
            var offset = (page - 1) * pageSize;

            // Get reward history from repository
            var rewardHistories = await _rewardHistoryRepository.GetByUserIdAsync(userId, pageSize, offset, cancellationToken);
            var rewardHistoriesList = rewardHistories.ToList();

            // Get total count for pagination (we need to get all records to count them)
            // Note: This is not optimal for large datasets, but the repository interface doesn't provide a count method
            var allRewardHistories = await _rewardHistoryRepository.GetByUserIdAsync(userId, int.MaxValue, 0, cancellationToken);
            var totalCount = allRewardHistories.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Convert to DTOs
            var entries = rewardHistoriesList.Select(rh => new UserRewardHistoryEntryDto
            {
                Id = rh.RewardHistoryId,
                UserId = rh.UserId,
                RewardType = rh.RewardType,
                RewardId = rh.RewardId,
                RewardName = GetRewardName(rh.RewardId, rh.RewardType),
                PointsAmount = ExtractPointsAmount(rh),
                PointCategory = ExtractPointCategory(rh),
                AwardedAt = rh.AwardedAt.DateTime,
                TriggerEventType = ExtractTriggerEventType(rh.TriggerEventId),
                TriggerEventId = rh.TriggerEventId
            }).ToList();

            return Result<UserRewardHistoryDto, string>.Success(new UserRewardHistoryDto
            {
                Entries = entries,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            return Result<UserRewardHistoryDto, string>.Failure($"Failed to get user reward history: {ex.Message}");
        }
    }

    public async Task<Result<UserSummariesDto, string>> GetUserSummariesAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return Result<UserSummariesDto, string>.Failure("Page must be greater than 0");

            if (pageSize < 1 || pageSize > 1000)
                return Result<UserSummariesDto, string>.Failure("Page size must be between 1 and 1000");

            // Get all user states
            var allUserStates = await _userStateRepository.GetAllAsync(cancellationToken);
            var userStatesList = allUserStates.ToList();

            // Calculate pagination
            var totalCount = userStatesList.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var skip = (page - 1) * pageSize;
            var paginatedUserStates = userStatesList.Skip(skip).Take(pageSize);

            // Convert to summaries
            var userSummaries = new List<UserSummaryDto>();
            foreach (var userState in paginatedUserStates)
            {
                var summary = await CreateUserSummaryAsync(userState, cancellationToken);
                userSummaries.Add(summary);
            }

            var result = new UserSummariesDto
            {
                Users = userSummaries,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };

            return Result<UserSummariesDto, string>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<UserSummariesDto, string>.Failure($"Failed to get user summaries: {ex.Message}");
        }
    }

    private async Task<UserSummaryDto> CreateUserSummaryAsync(Domain.Users.UserState userState, CancellationToken cancellationToken)
    {
        // Get badge and trophy details
        var badges = await GetUserBadgesAsync(userState.UserId, cancellationToken);
        var trophies = await GetUserTrophiesAsync(userState.UserId, cancellationToken);
        var currentLevels = await GetUserCurrentLevelsAsync(userState.UserId, cancellationToken);

        var totalPoints = userState.PointsByCategory.Values.Sum();

        return new UserSummaryDto
        {
            UserId = userState.UserId,
            TotalPoints = totalPoints,
            BadgeCount = badges.IsSuccess ? badges.Value.Count() : 0,
            TrophyCount = trophies.IsSuccess ? trophies.Value.Count() : 0,
            PointsByCategory = userState.PointsByCategory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            CurrentLevelsByCategory = currentLevels.IsSuccess ? currentLevels.Value : new Dictionary<string, LevelDto>()
        };
    }

    /// <summary>
    /// Gets a human-readable name for a reward based on its ID and type
    /// </summary>
    private static string GetRewardName(string rewardId, string rewardType)
    {
        return rewardType.ToLower() switch
        {
            "points" => GetPointsRewardName(rewardId),
            "badge" => GetBadgeRewardName(rewardId),
            "trophy" => GetTrophyRewardName(rewardId),
            _ => rewardId
        };
    }

    /// <summary>
    /// Gets a human-readable name for points rewards
    /// </summary>
    private static string GetPointsRewardName(string rewardId)
    {
        return rewardId switch
        {
            "reward-welcome-bonus" => "Welcome Bonus",
            "reward-initial-credits" => "Initial Credits",
            "reward-profile-completion" => "Profile Completion",
            "reward-profile-completion-score" => "Profile Completion Score",
            "reward-profile-completion-credits" => "Profile Completion Credits",
            var id when id.StartsWith("reward-comment-") => "Comment Reward",
            var id when id.StartsWith("reward-score-bonus-") => "Quality Comment Bonus",
            var id when id.StartsWith("reward-credits-bonus-") => "Helpful Comment Bonus",
            _ => rewardId
        };
    }

    /// <summary>
    /// Gets a human-readable name for badge rewards
    /// </summary>
    private static string GetBadgeRewardName(string rewardId)
    {
        return rewardId switch
        {
            "reward-commenter-badge" => "Commenter Badge",
            "reward-profile-badge" => "Profile Completion Badge",
            _ => rewardId
        };
    }

    /// <summary>
    /// Gets a human-readable name for trophy rewards
    /// </summary>
    private static string GetTrophyRewardName(string rewardId)
    {
        return rewardId; // For now, just return the ID
    }

    /// <summary>
    /// Extracts the points amount from reward history details
    /// </summary>
    private static long? ExtractPointsAmount(Domain.Rewards.RewardHistory rewardHistory)
    {
        if (rewardHistory.Details.TryGetValue("amount", out var amountObj))
        {
            return amountObj switch
            {
                long l => l,
                int i => i,
                string s when long.TryParse(s, out var parsedLong) => parsedLong,
                string s when int.TryParse(s, out var parsedInt) => parsedInt,
                _ => null
            };
        }
        return null;
    }

    /// <summary>
    /// Extracts the point category from reward history details
    /// </summary>
    private static string? ExtractPointCategory(Domain.Rewards.RewardHistory rewardHistory)
    {
        if (rewardHistory.Details.TryGetValue("category", out var categoryObj))
        {
            return categoryObj switch
            {
                string s => s,
                _ => categoryObj?.ToString()
            };
        }
        return null;
    }

    /// <summary>
    /// Extracts the trigger event type from the trigger event ID
    /// </summary>
    private static string? ExtractTriggerEventType(string triggerEventId)
    {
        return triggerEventId switch
        {
            "event-welcome" => "Welcome",
            "event-registration" => "Registration",
            "event-profile-complete" => "Profile Complete",
            var id when id.StartsWith("event-comment-") => "Comment",
            var id when id.StartsWith("event-quality-comment-") => "Quality Comment",
            var id when id.StartsWith("event-helpful-comment-") => "Helpful Comment",
            _ => null
        };
    }
}
