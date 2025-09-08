using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Domain.Users;
using GamificationEngine.Shared;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service responsible for executing rewards
/// </summary>
public class RewardExecutionService : IRewardExecutionService
{
    private readonly IUserStateRepository _userStateRepository;
    private readonly IRewardHistoryRepository _rewardHistoryRepository;
    private readonly ILogger<RewardExecutionService> _logger;

    public RewardExecutionService(
        IUserStateRepository userStateRepository,
        IRewardHistoryRepository rewardHistoryRepository,
        ILogger<RewardExecutionService> logger)
    {
        _userStateRepository = userStateRepository ?? throw new ArgumentNullException(nameof(userStateRepository));
        _rewardHistoryRepository = rewardHistoryRepository ?? throw new ArgumentNullException(nameof(rewardHistoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a reward for a user
    /// </summary>
    /// <param name="reward">The reward to execute</param>
    /// <param name="userId">The user ID to award the reward to</param>
    /// <param name="triggerEvent">The event that triggered the reward</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the execution outcome</returns>
    public async Task<Result<RewardExecutionResult, DomainError>> ExecuteRewardAsync(
        Reward reward,
        string userId,
        Event triggerEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing reward {RewardId} of type {RewardType} for user {UserId}",
                reward.RewardId, reward.Type, userId);

            if (!reward.IsValid())
            {
                var error = new RuleEvaluationError($"Invalid reward configuration for reward {reward.RewardId}");
                await LogRewardExecutionAsync(reward, userId, triggerEvent, false, error.Message, cancellationToken);
                return Result<RewardExecutionResult, DomainError>.Failure(error);
            }

            // Get or create user state
            var userState = await _userStateRepository.GetByUserIdAsync(userId, cancellationToken)
                ?? new UserState(userId);

            // Execute the reward based on its type
            var executionResult = reward.Type.ToLowerInvariant() switch
            {
                "points" => await ExecutePointsRewardAsync((PointsReward)reward, userState, cancellationToken),
                "badge" => await ExecuteBadgeRewardAsync((BadgeReward)reward, userState, cancellationToken),
                "trophy" => await ExecuteTrophyRewardAsync((TrophyReward)reward, userState, cancellationToken),
                "level" => await ExecuteLevelRewardAsync((LevelReward)reward, userState, cancellationToken),
                "penalty" => await ExecutePenaltyRewardAsync((PenaltyReward)reward, userState, cancellationToken),
                _ => new RewardExecutionResult(reward.RewardId, reward.Type, userId, triggerEvent.EventId, DateTimeOffset.UtcNow, false, $"Unsupported reward type: {reward.Type}")
            };

            // Save user state if reward was successful
            if (executionResult.Success)
            {
                await _userStateRepository.SaveAsync(userState, cancellationToken);
            }

            // Log the reward execution
            await LogRewardExecutionAsync(reward, userId, triggerEvent, executionResult.Success, executionResult.Message, cancellationToken);

            _logger.LogInformation("Successfully executed reward {RewardId} for user {UserId}: {Message}",
                reward.RewardId, userId, executionResult.Message);

            return Result<RewardExecutionResult, DomainError>.Success(executionResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing reward {RewardId} for user {UserId}", reward.RewardId, userId);

            var error = new RuleEvaluationError($"Failed to execute reward {reward.RewardId}: {ex.Message}");
            await LogRewardExecutionAsync(reward, userId, triggerEvent, false, ex.Message, cancellationToken);

            return Result<RewardExecutionResult, DomainError>.Failure(error);
        }
    }

    private async Task<RewardExecutionResult> ExecutePointsRewardAsync(PointsReward reward, UserState userState, CancellationToken cancellationToken)
    {
        try
        {
            var amount = reward.GetPointsAmount();
            var category = reward.GetCategory();

            userState.AddPoints(category, amount);

            return new RewardExecutionResult(
                reward.RewardId,
                reward.Type,
                userState.UserId,
                string.Empty, // Will be set by caller
                DateTimeOffset.UtcNow,
                true,
                $"Awarded {amount} {category} points");
        }
        catch (Exception ex)
        {
            return new RewardExecutionResult(
                reward.RewardId,
                reward.Type,
                userState.UserId,
                string.Empty,
                DateTimeOffset.UtcNow,
                false,
                $"Failed to award points: {ex.Message}");
        }
    }

    private async Task<RewardExecutionResult> ExecuteBadgeRewardAsync(BadgeReward reward, UserState userState, CancellationToken cancellationToken)
    {
        try
        {
            var badgeId = reward.GetBadgeId();

            // Check if user already has this badge
            if (userState.Badges.Contains(badgeId))
            {
                return new RewardExecutionResult(
                    reward.RewardId,
                    reward.Type,
                    userState.UserId,
                    string.Empty,
                    DateTimeOffset.UtcNow,
                    true,
                    $"Badge {badgeId} already awarded");
            }

            userState.GrantBadge(badgeId);

            return new RewardExecutionResult(
                reward.RewardId,
                reward.Type,
                userState.UserId,
                string.Empty,
                DateTimeOffset.UtcNow,
                true,
                $"Awarded badge {badgeId}");
        }
        catch (Exception ex)
        {
            return new RewardExecutionResult(
                reward.RewardId,
                reward.Type,
                userState.UserId,
                string.Empty,
                DateTimeOffset.UtcNow,
                false,
                $"Failed to award badge: {ex.Message}");
        }
    }

    private async Task<RewardExecutionResult> ExecuteTrophyRewardAsync(TrophyReward reward, UserState userState, CancellationToken cancellationToken)
    {
        try
        {
            var trophyId = reward.GetTrophyId();

            // Check if user already has this trophy
            if (userState.Trophies.Contains(trophyId))
            {
                return new RewardExecutionResult(
                    reward.RewardId,
                    reward.Type,
                    userState.UserId,
                    string.Empty,
                    DateTimeOffset.UtcNow,
                    true,
                    $"Trophy {trophyId} already awarded");
            }

            userState.GrantTrophy(trophyId);

            return new RewardExecutionResult(
                reward.RewardId,
                reward.Type,
                userState.UserId,
                string.Empty,
                DateTimeOffset.UtcNow,
                true,
                $"Awarded trophy {trophyId}");
        }
        catch (Exception ex)
        {
            return new RewardExecutionResult(
                reward.RewardId,
                reward.Type,
                userState.UserId,
                string.Empty,
                DateTimeOffset.UtcNow,
                false,
                $"Failed to award trophy: {ex.Message}");
        }
    }

    private async Task<RewardExecutionResult> ExecuteLevelRewardAsync(LevelReward reward, UserState userState, CancellationToken cancellationToken)
    {
        try
        {
            var levelId = reward.GetLevelId();
            var category = reward.GetCategory();

            // For now, we'll just log that a level was awarded
            // In a more complete implementation, this would check point thresholds and calculate the appropriate level
            // This is a placeholder for the level progression logic

            return new RewardExecutionResult(
                reward.RewardId,
                reward.Type,
                userState.UserId,
                string.Empty,
                DateTimeOffset.UtcNow,
                true,
                $"Level {levelId} progression calculated for {category} category");
        }
        catch (Exception ex)
        {
            return new RewardExecutionResult(
                reward.RewardId,
                reward.Type,
                userState.UserId,
                string.Empty,
                DateTimeOffset.UtcNow,
                false,
                $"Failed to calculate level progression: {ex.Message}");
        }
    }

    private async Task<RewardExecutionResult> ExecutePenaltyRewardAsync(PenaltyReward reward, UserState userState, CancellationToken cancellationToken)
    {
        try
        {
            var penaltyType = reward.GetPenaltyType();
            var targetId = reward.GetTargetId();
            var amount = reward.GetAmount();

            switch (penaltyType.ToLowerInvariant())
            {
                case "points":
                    if (amount.HasValue)
                    {
                        userState.AddPoints(targetId, -amount.Value); // Negative amount for penalty
                        return new RewardExecutionResult(
                            reward.RewardId,
                            reward.Type,
                            userState.UserId,
                            string.Empty,
                            DateTimeOffset.UtcNow,
                            true,
                            $"Penalty: Removed {amount.Value} {targetId} points");
                    }
                    break;

                case "badge":
                    // Remove badge if present
                    if (userState.Badges.Contains(targetId))
                    {
                        userState.RemoveBadge(targetId);
                        return new RewardExecutionResult(
                            reward.RewardId,
                            reward.Type,
                            userState.UserId,
                            string.Empty,
                            DateTimeOffset.UtcNow,
                            true,
                            $"Penalty: Removed badge {targetId}");
                    }
                    break;

                case "trophy":
                    // Remove trophy if present
                    if (userState.Trophies.Contains(targetId))
                    {
                        userState.RemoveTrophy(targetId);
                        return new RewardExecutionResult(
                            reward.RewardId,
                            reward.Type,
                            userState.UserId,
                            string.Empty,
                            DateTimeOffset.UtcNow,
                            true,
                            $"Penalty: Removed trophy {targetId}");
                    }
                    break;

                default:
                    return new RewardExecutionResult(
                        reward.RewardId,
                        reward.Type,
                        userState.UserId,
                        string.Empty,
                        DateTimeOffset.UtcNow,
                        false,
                        $"Unsupported penalty type: {penaltyType}");
            }

            return new RewardExecutionResult(
                reward.RewardId,
                reward.Type,
                userState.UserId,
                string.Empty,
                DateTimeOffset.UtcNow,
                true,
                $"Penalty applied: {penaltyType} on {targetId}");
        }
        catch (Exception ex)
        {
            return new RewardExecutionResult(
                reward.RewardId,
                reward.Type,
                userState.UserId,
                string.Empty,
                DateTimeOffset.UtcNow,
                false,
                $"Failed to apply penalty: {ex.Message}");
        }
    }

    private async Task LogRewardExecutionAsync(Reward reward, string userId, Event triggerEvent, bool success, string message, CancellationToken cancellationToken)
    {
        try
        {
            // Create enhanced details that include reward-specific information for leaderboard filtering
            var details = new Dictionary<string, object>(reward.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            // Add reward-specific details based on type
            switch (reward.Type.ToLowerInvariant())
            {
                case "points":
                    if (reward is PointsReward pointsReward)
                    {
                        details["amount"] = pointsReward.GetPointsAmount();
                        details["category"] = pointsReward.GetCategory();
                    }
                    break;

                case "badge":
                    if (reward is BadgeReward badgeReward)
                    {
                        details["badgeId"] = badgeReward.GetBadgeId();
                    }
                    break;

                case "trophy":
                    if (reward is TrophyReward trophyReward)
                    {
                        details["trophyId"] = trophyReward.GetTrophyId();
                    }
                    break;

                case "penalty":
                    if (reward is PenaltyReward penaltyReward)
                    {
                        details["penaltyType"] = penaltyReward.GetPenaltyType();
                        details["targetId"] = penaltyReward.GetTargetId();
                        if (penaltyReward.GetAmount().HasValue)
                        {
                            details["amount"] = penaltyReward.GetAmount().Value;
                        }
                    }
                    break;
            }

            var rewardHistory = new RewardHistory(
                Guid.NewGuid().ToString(),
                userId,
                reward.RewardId,
                reward.Type,
                triggerEvent.EventId,
                DateTimeOffset.UtcNow,
                success,
                message,
                details);

            await _rewardHistoryRepository.StoreAsync(rewardHistory, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log reward execution for reward {RewardId}", reward.RewardId);
        }
    }
}
