using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Shared;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service responsible for evaluating rules against events and executing rewards
/// </summary>
public class RuleEvaluationService : IRuleEvaluationService
{
    private readonly IRuleRepository _ruleRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IRewardExecutionService _rewardExecutionService;
    private readonly ILogger<RuleEvaluationService> _logger;

    public RuleEvaluationService(
        IRuleRepository ruleRepository,
        IEventRepository eventRepository,
        IRewardExecutionService rewardExecutionService,
        ILogger<RuleEvaluationService> logger)
    {
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _rewardExecutionService = rewardExecutionService ?? throw new ArgumentNullException(nameof(rewardExecutionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Evaluates all applicable rules for the given event and executes rewards if conditions are met
    /// </summary>
    /// <param name="triggerEvent">The event that triggered the rule evaluation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the evaluation outcome and any executed rewards</returns>
    public async Task<Result<RuleEvaluationResult, DomainError>> EvaluateRulesAsync(
        Event triggerEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting rule evaluation for event {EventId} of type {EventType} for user {UserId}",
                triggerEvent.EventId, triggerEvent.EventType, triggerEvent.UserId);

            // Get all rules that can be triggered by this event type
            var applicableRules = await _ruleRepository.GetByTriggerAsync(triggerEvent.EventType);
            var activeRules = applicableRules.Where(r => r.IsActive).ToList();

            if (!activeRules.Any())
            {
                _logger.LogDebug("No active rules found for event type {EventType}", triggerEvent.EventType);
                return Result<RuleEvaluationResult, DomainError>.Success(new RuleEvaluationResult(
                    triggerEvent.EventId,
                    triggerEvent.UserId,
                    new List<RewardExecutionResult>()));
            }

            _logger.LogDebug("Found {RuleCount} active rules for event type {EventType}",
                activeRules.Count, triggerEvent.EventType);

            var executedRewards = new List<RewardExecutionResult>();

            // Evaluate each rule
            foreach (var rule in activeRules)
            {
                var ruleResult = await EvaluateSingleRuleAsync(rule, triggerEvent, cancellationToken);
                if (ruleResult.IsSuccess)
                {
                    executedRewards.AddRange(ruleResult.Value?.ExecutedRewards ?? new List<RewardExecutionResult>());
                }
                else
                {
                    _logger.LogWarning("Rule evaluation failed for rule {RuleId}: {Error}",
                        rule.RuleId, ruleResult.Error?.Message ?? "Unknown error");
                    // If a rule evaluation fails due to a system error (not just condition failure),
                    // we should fail the entire operation
                    if (ruleResult.Error.Code == "RULE_EVALUATION_ERROR")
                    {
                        return ruleResult;
                    }
                }
            }

            var evaluationResult = new RuleEvaluationResult(
                triggerEvent.EventId,
                triggerEvent.UserId,
                executedRewards);

            _logger.LogInformation("Rule evaluation completed for event {EventId}. Executed {RewardCount} rewards",
                triggerEvent.EventId, executedRewards.Count);

            return Result<RuleEvaluationResult, DomainError>.Success(evaluationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during rule evaluation for event {EventId}", triggerEvent.EventId);
            return Result<RuleEvaluationResult, DomainError>.Failure(
                new RuleEvaluationError($"Failed to evaluate rules: {ex.Message}"));
        }
    }

    /// <summary>
    /// Evaluates a single rule against the trigger event
    /// </summary>
    /// <param name="rule">The rule to evaluate</param>
    /// <param name="triggerEvent">The event that triggered the evaluation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the rule evaluation outcome</returns>
    private async Task<Result<RuleEvaluationResult, DomainError>> EvaluateSingleRuleAsync(
        Rule rule,
        Event triggerEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Evaluating rule {RuleId}: {RuleSummary}", rule.RuleId, rule.GetSummary());

            // Get user's recent events for condition evaluation
            var userEvents = await _eventRepository.GetByUserIdAsync(triggerEvent.UserId, 1000, 0);
            var eventsList = userEvents.ToList();

            // Evaluate rule conditions
            var conditionsMet = rule.EvaluateConditions(eventsList, triggerEvent);

            if (!conditionsMet)
            {
                _logger.LogDebug("Rule {RuleId} conditions not met", rule.RuleId);
                return Result<RuleEvaluationResult, DomainError>.Success(new RuleEvaluationResult(
                    triggerEvent.EventId,
                    triggerEvent.UserId,
                    new List<RewardExecutionResult>()));
            }

            _logger.LogInformation("Rule {RuleId} conditions met, executing rewards", rule.RuleId);

            // Execute rewards
            var executedRewards = new List<RewardExecutionResult>();
            foreach (var reward in rule.Rewards)
            {
                var rewardResult = await _rewardExecutionService.ExecuteRewardAsync(reward, triggerEvent.UserId, triggerEvent, cancellationToken);
                if (rewardResult.IsSuccess && rewardResult.Value != null)
                {
                    executedRewards.Add(rewardResult.Value);
                }
                else
                {
                    _logger.LogWarning("Failed to execute reward {RewardId}: {Error}", reward.RewardId, rewardResult.Error?.Message ?? "Unknown error");
                    // Still add a failed execution result for tracking
                    executedRewards.Add(new RewardExecutionResult(
                        reward.RewardId,
                        reward.Type,
                        triggerEvent.UserId,
                        triggerEvent.EventId,
                        DateTimeOffset.UtcNow,
                        false,
                        rewardResult.Error.Message));
                }
            }

            return Result<RuleEvaluationResult, DomainError>.Success(new RuleEvaluationResult(
                triggerEvent.EventId,
                triggerEvent.UserId,
                executedRewards));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating rule {RuleId}", rule.RuleId);
            return Result<RuleEvaluationResult, DomainError>.Failure(
                new RuleEvaluationError($"Failed to evaluate rule {rule.RuleId}: {ex.Message}"));
        }
    }

}

