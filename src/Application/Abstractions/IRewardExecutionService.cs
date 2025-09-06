using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for executing rewards
/// </summary>
public interface IRewardExecutionService
{
    /// <summary>
    /// Executes a reward for a user
    /// </summary>
    /// <param name="reward">The reward to execute</param>
    /// <param name="userId">The user ID to award the reward to</param>
    /// <param name="triggerEvent">The event that triggered the reward</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the execution outcome</returns>
    Task<Result<RewardExecutionResult, DomainError>> ExecuteRewardAsync(
        Reward reward,
        string userId,
        Event triggerEvent,
        CancellationToken cancellationToken = default);
}
