using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Shared;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for performing dry-run evaluations of rules without executing rewards
/// </summary>
public class DryRunEvaluationService : IDryRunEvaluationService
{
    private readonly IRuleRepository _ruleRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IEventValidationService _eventValidationService;
    private readonly ILogger<DryRunEvaluationService> _logger;

    public DryRunEvaluationService(
        IRuleRepository ruleRepository,
        IEventRepository eventRepository,
        IEventValidationService eventValidationService,
        ILogger<DryRunEvaluationService> logger)
    {
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _eventValidationService = eventValidationService ?? throw new ArgumentNullException(nameof(eventValidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs a dry-run evaluation of rules for the given event without executing rewards
    /// </summary>
    /// <param name="triggerEvent">The event to evaluate rules against</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing detailed evaluation trace</returns>
    public async Task<Result<DryRunResponseDto, DomainError>> DryRunRulesAsync(
        Event triggerEvent,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var evaluatedAt = DateTimeOffset.UtcNow;

        try
        {
            _logger.LogInformation("Starting dry-run evaluation for event {EventId} of type {EventType} for user {UserId}",
                triggerEvent.EventId, triggerEvent.EventType, triggerEvent.UserId);

            // Validate the event first
            var isValid = await _eventValidationService.ValidateEventAsync(triggerEvent);
            var validationErrors = new List<string>();

            if (!isValid)
            {
                validationErrors.Add($"Event validation failed for event type: {triggerEvent.EventType}");
            }

            // Get all rules that can be triggered by this event type
            var applicableRules = await _ruleRepository.GetByTriggerAsync(triggerEvent.EventType);
            var activeRules = applicableRules.Where(r => r.IsActive).ToList();

            _logger.LogDebug("Found {RuleCount} active rules for event type {EventType}",
                activeRules.Count, triggerEvent.EventType);

            var ruleTraces = new List<RuleTrace>();
            var totalPredictedRewards = 0;

            // Evaluate each rule
            foreach (var rule in activeRules)
            {
                var ruleTrace = await EvaluateRuleWithTraceAsync(rule, triggerEvent, cancellationToken);
                ruleTraces.Add(ruleTrace);

                if (ruleTrace.WouldExecute)
                {
                    totalPredictedRewards += ruleTrace.PredictedRewards.Count();
                }
            }

            stopwatch.Stop();

            // Create summary
            var summary = new DryRunSummary
            {
                TotalRulesEvaluated = activeRules.Count,
                RulesThatWouldExecute = ruleTraces.Count(r => r.WouldExecute),
                TotalPredictedRewards = totalPredictedRewards,
                TotalEvaluationTimeMs = stopwatch.ElapsedMilliseconds,
                EventValid = isValid,
                ValidationErrors = validationErrors
            };

            // Create response
            var response = new DryRunResponseDto
            {
                TriggerEventId = triggerEvent.EventId,
                UserId = triggerEvent.UserId,
                EventType = triggerEvent.EventType,
                Rules = ruleTraces,
                Summary = summary,
                EvaluatedAt = evaluatedAt
            };

            _logger.LogInformation("Dry-run evaluation completed for event {EventId}. {RulesExecuted} rules would execute, {RewardCount} rewards predicted",
                triggerEvent.EventId, summary.RulesThatWouldExecute, summary.TotalPredictedRewards);

            return Result<DryRunResponseDto, DomainError>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during dry-run evaluation for event {EventId}", triggerEvent.EventId);
            return Result<DryRunResponseDto, DomainError>.Failure(
                new RuleEvaluationError($"Failed to perform dry-run evaluation: {ex.Message}"));
        }
    }

    /// <summary>
    /// Evaluates a single rule with detailed tracing
    /// </summary>
    private async Task<RuleTrace> EvaluateRuleWithTraceAsync(
        Rule rule,
        Event triggerEvent,
        CancellationToken cancellationToken)
    {
        var ruleStopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogDebug("Evaluating rule {RuleId} with tracing: {RuleSummary}", rule.RuleId, rule.GetSummary());

            // Check if trigger matches
            var triggerMatched = rule.ShouldTrigger(triggerEvent.EventType);

            if (!triggerMatched)
            {
                ruleStopwatch.Stop();
                return new RuleTrace
                {
                    RuleId = rule.RuleId,
                    Name = rule.Name,
                    Description = rule.Description ?? string.Empty,
                    TriggerMatched = false,
                    Conditions = new List<ConditionTrace>(),
                    PredictedRewards = new List<PredictedReward>(),
                    PredictedSpendings = new List<PredictedSpending>(),
                    WouldExecute = false,
                    EvaluationTimeMs = ruleStopwatch.ElapsedMilliseconds
                };
            }

            // Get user's recent events for condition evaluation
            var userEvents = await _eventRepository.GetByUserIdAsync(triggerEvent.UserId, 1000, 0);
            var eventsList = userEvents.ToList();

            // Evaluate each condition with tracing
            var conditionTraces = new List<ConditionTrace>();
            var allConditionsMet = true;

            foreach (var condition in rule.Conditions)
            {
                var conditionTrace = await EvaluateConditionWithTraceAsync(condition, eventsList, triggerEvent);
                conditionTraces.Add(conditionTrace);

                if (!conditionTrace.Result)
                {
                    allConditionsMet = false;
                }
            }

            // Predict rewards and spendings if conditions are met
            var predictedRewards = new List<PredictedReward>();
            var predictedSpendings = new List<PredictedSpending>();
            if (allConditionsMet)
            {
                predictedRewards = rule.Rewards.Select(reward => PredictReward(reward)).ToList();
                predictedSpendings = rule.Spendings.Select(spending => PredictSpending(spending, triggerEvent)).ToList();
            }

            ruleStopwatch.Stop();

            return new RuleTrace
            {
                RuleId = rule.RuleId,
                Name = rule.Name,
                Description = rule.Description ?? string.Empty,
                TriggerMatched = true,
                Conditions = conditionTraces,
                PredictedRewards = predictedRewards,
                PredictedSpendings = predictedSpendings,
                WouldExecute = allConditionsMet,
                EvaluationTimeMs = ruleStopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating rule {RuleId} with tracing", rule.RuleId);
            ruleStopwatch.Stop();

            return new RuleTrace
            {
                RuleId = rule.RuleId,
                Name = rule.Name,
                Description = rule.Description ?? string.Empty,
                TriggerMatched = true,
                Conditions = new List<ConditionTrace>
                {
                    new ConditionTrace
                    {
                        ConditionId = "ERROR",
                        Type = "ERROR",
                        Parameters = new Dictionary<string, object>(),
                        Result = false,
                        Details = $"Error evaluating rule: {ex.Message}",
                        EvaluationTimeMs = 0
                    }
                },
                PredictedRewards = new List<PredictedReward>(),
                WouldExecute = false,
                EvaluationTimeMs = ruleStopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <summary>
    /// Evaluates a single condition with detailed tracing
    /// </summary>
    private async Task<ConditionTrace> EvaluateConditionWithTraceAsync(
        Condition condition,
        IEnumerable<Event> events,
        Event triggerEvent)
    {
        var conditionStopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogDebug("Evaluating condition {ConditionId} of type {ConditionType}",
                condition.ConditionId, condition.Type);

            // Evaluate the condition
            var result = condition.Evaluate(events, triggerEvent);

            conditionStopwatch.Stop();

            // Create detailed trace information
            var details = CreateConditionDetails(condition, events, triggerEvent, result);

            return new ConditionTrace
            {
                ConditionId = condition.ConditionId,
                Type = condition.Type,
                Parameters = condition.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Result = result,
                Details = details,
                EvaluationTimeMs = conditionStopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating condition {ConditionId}", condition.ConditionId);
            conditionStopwatch.Stop();

            return new ConditionTrace
            {
                ConditionId = condition.ConditionId,
                Type = condition.Type,
                Parameters = condition.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Result = false,
                Details = $"Error evaluating condition: {ex.Message}",
                EvaluationTimeMs = conditionStopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <summary>
    /// Creates detailed information about condition evaluation
    /// </summary>
    private string CreateConditionDetails(Condition condition, IEnumerable<Event> events, Event triggerEvent, bool result)
    {
        var details = new List<string>();

        details.Add($"Condition Type: {condition.Type}");
        details.Add($"Parameters: {string.Join(", ", condition.Parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        details.Add($"Result: {result}");
        details.Add($"Events Count: {events.Count()}");
        details.Add($"Trigger Event: {triggerEvent.EventType} (ID: {triggerEvent.EventId})");

        // Add type-specific details
        switch (condition.Type.ToLowerInvariant())
        {
            case "count":
                if (condition.Parameters.TryGetValue("eventType", out var eventType))
                {
                    var count = events.Count(e => e.EventType.Equals(eventType.ToString(), StringComparison.OrdinalIgnoreCase));
                    details.Add($"Counted {count} events of type '{eventType}'");
                }
                break;

            case "attributeequals":
                if (condition.Parameters.TryGetValue("attribute", out var attribute) &&
                    condition.Parameters.TryGetValue("value", out var expectedValue))
                {
                    var actualValue = triggerEvent.Attributes.TryGetValue(attribute.ToString()!, out var value) ? value : "null";
                    details.Add($"Attribute '{attribute}' = '{actualValue}', Expected: '{expectedValue}'");
                }
                break;

            case "threshold":
                if (condition.Parameters.TryGetValue("attribute", out var thresholdAttribute) &&
                    condition.Parameters.TryGetValue("operator", out var op) &&
                    condition.Parameters.TryGetValue("value", out var thresholdValue))
                {
                    var actualValue = triggerEvent.Attributes.TryGetValue(thresholdAttribute.ToString()!, out var value) ? value : "null";
                    details.Add($"Attribute '{thresholdAttribute}' = '{actualValue}' {op} '{thresholdValue}'");
                }
                break;
        }

        return string.Join("; ", details);
    }

    /// <summary>
    /// Predicts what a reward would do without actually executing it
    /// </summary>
    private PredictedReward PredictReward(Reward reward)
    {
        return reward.Type.ToLowerInvariant() switch
        {
            "points" => PredictPointsReward((PointsReward)reward),
            "badge" => PredictBadgeReward((BadgeReward)reward),
            "trophy" => PredictTrophyReward((TrophyReward)reward),
            "level" => PredictLevelReward((LevelReward)reward),
            "penalty" => PredictPenaltyReward((PenaltyReward)reward),
            _ => new PredictedReward
            {
                Type = reward.Type,
                TargetId = reward.RewardId,
                Amount = null,
                Parameters = reward.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Name = $"Unknown {reward.Type}",
                Description = $"Unknown reward type: {reward.Type}"
            }
        };
    }

    private PredictedReward PredictPointsReward(PointsReward reward)
    {
        return new PredictedReward
        {
            Type = "points",
            TargetId = reward.Category,
            Amount = reward.Amount,
            Parameters = reward.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Name = $"{reward.Amount} {reward.Category} Points",
            Description = $"Award {reward.Amount} points in category '{reward.Category}'"
        };
    }

    private PredictedReward PredictBadgeReward(BadgeReward reward)
    {
        return new PredictedReward
        {
            Type = "badge",
            TargetId = reward.BadgeId,
            Amount = null,
            Parameters = reward.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Name = $"Badge: {reward.BadgeId}",
            Description = $"Award badge '{reward.BadgeId}'"
        };
    }

    private PredictedReward PredictTrophyReward(TrophyReward reward)
    {
        return new PredictedReward
        {
            Type = "trophy",
            TargetId = reward.TrophyId,
            Amount = null,
            Parameters = reward.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Name = $"Trophy: {reward.TrophyId}",
            Description = $"Award trophy '{reward.TrophyId}'"
        };
    }

    private PredictedReward PredictLevelReward(LevelReward reward)
    {
        return new PredictedReward
        {
            Type = "level",
            TargetId = reward.LevelId,
            Amount = null,
            Parameters = reward.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Name = $"Level: {reward.LevelId}",
            Description = $"Award level '{reward.LevelId}'"
        };
    }

    private PredictedReward PredictPenaltyReward(PenaltyReward reward)
    {
        return new PredictedReward
        {
            Type = "penalty",
            TargetId = reward.TargetId,
            Amount = reward.Amount,
            Parameters = reward.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Name = $"Penalty: {reward.PenaltyType}",
            Description = $"Apply penalty '{reward.PenaltyType}' to '{reward.TargetId}' with amount {reward.Amount}"
        };
    }

    /// <summary>
    /// Predicts what a spending would do based on the spending configuration and trigger event
    /// </summary>
    private PredictedSpending PredictSpending(RuleSpending spending, Event triggerEvent)
    {
        var predictedSpending = new PredictedSpending
        {
            Type = spending.Type.ToString().ToLower(),
            Category = spending.Category,
            Attributes = spending.Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };

        // Try to extract amount from event payload
        var amountStr = spending.GetAmountAttribute();
        if (long.TryParse(amountStr, out var amount))
        {
            predictedSpending.Amount = amount;
        }
        else if (triggerEvent.Attributes?.TryGetValue(amountStr, out var amountValue) == true)
        {
            if (long.TryParse(amountValue?.ToString(), out amount))
            {
                predictedSpending.Amount = amount;
            }
        }

        // For transfers, try to extract destination user
        if (spending.Type == RuleSpendingType.Transfer)
        {
            var destinationAttr = spending.GetDestinationAttribute();
            if (triggerEvent.Attributes?.TryGetValue(destinationAttr, out var destinationValue) == true)
            {
                predictedSpending.DestinationUserId = destinationValue?.ToString();
            }
        }

        return predictedSpending;
    }
}
