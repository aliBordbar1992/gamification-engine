using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for evaluating rules against events and executing rewards
/// </summary>
public interface IRuleEvaluationService
{
    /// <summary>
    /// Evaluates all applicable rules for the given event and executes rewards if conditions are met
    /// </summary>
    /// <param name="triggerEvent">The event that triggered the rule evaluation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the evaluation outcome and any executed rewards</returns>
    Task<Result<RuleEvaluationResult, DomainError>> EvaluateRulesAsync(
        Event triggerEvent,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a rule evaluation containing executed rewards and spendings
/// </summary>
public record RuleEvaluationResult(
    string TriggerEventId,
    string UserId,
    IReadOnlyCollection<RewardExecutionResult> ExecutedRewards,
    IReadOnlyCollection<SpendingExecutionResult> ExecutedSpendings);

/// <summary>
/// Result of executing a single reward
/// </summary>
public record RewardExecutionResult(
    string RewardId,
    string RewardType,
    string UserId,
    string TriggerEventId,
    DateTimeOffset ExecutedAt,
    bool Success,
    string Message);
