using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data transfer object for condition evaluation trace
/// </summary>
public sealed class ConditionTrace
{
    /// <summary>
    /// Unique identifier for the condition
    /// </summary>
    [JsonPropertyName("conditionId")]
    public string ConditionId { get; set; } = string.Empty;

    /// <summary>
    /// Type of the condition (e.g., "alwaysTrue", "attributeEquals", "count", etc.)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Parameters used for condition evaluation
    /// </summary>
    [JsonPropertyName("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Result of the condition evaluation (true/false)
    /// </summary>
    [JsonPropertyName("result")]
    public bool Result { get; set; }

    /// <summary>
    /// Additional details about the evaluation process
    /// </summary>
    [JsonPropertyName("details")]
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Time taken to evaluate the condition in milliseconds
    /// </summary>
    [JsonPropertyName("evaluationTimeMs")]
    public long EvaluationTimeMs { get; set; }
}

/// <summary>
/// Data transfer object for predicted reward outcome
/// </summary>
public sealed class PredictedReward
{
    /// <summary>
    /// Type of the reward (e.g., "points", "badge", "trophy", "level")
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Target ID of the reward (badge ID, trophy ID, point category, etc.)
    /// </summary>
    [JsonPropertyName("targetId")]
    public string TargetId { get; set; } = string.Empty;

    /// <summary>
    /// Amount of the reward (for points)
    /// </summary>
    [JsonPropertyName("amount")]
    public long? Amount { get; set; }

    /// <summary>
    /// Additional parameters for the reward
    /// </summary>
    [JsonPropertyName("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Name of the reward for display purposes
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the reward
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for rule evaluation trace
/// </summary>
public sealed class RuleTrace
{
    /// <summary>
    /// Unique identifier for the rule
    /// </summary>
    [JsonPropertyName("ruleId")]
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the rule
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the rule
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether the rule's trigger matched the event
    /// </summary>
    [JsonPropertyName("triggerMatched")]
    public bool TriggerMatched { get; set; }

    /// <summary>
    /// List of condition traces for this rule
    /// </summary>
    [JsonPropertyName("conditions")]
    public IEnumerable<ConditionTrace> Conditions { get; set; } = new List<ConditionTrace>();

    /// <summary>
    /// List of predicted rewards that would be awarded if this rule executes
    /// </summary>
    [JsonPropertyName("predictedRewards")]
    public IEnumerable<PredictedReward> PredictedRewards { get; set; } = new List<PredictedReward>();

    /// <summary>
    /// Whether all conditions passed (rule would execute)
    /// </summary>
    [JsonPropertyName("wouldExecute")]
    public bool WouldExecute { get; set; }

    /// <summary>
    /// Time taken to evaluate the entire rule in milliseconds
    /// </summary>
    [JsonPropertyName("evaluationTimeMs")]
    public long EvaluationTimeMs { get; set; }
}

/// <summary>
/// Data transfer object for dry-run evaluation summary
/// </summary>
public sealed class DryRunSummary
{
    /// <summary>
    /// Total number of rules evaluated
    /// </summary>
    [JsonPropertyName("totalRulesEvaluated")]
    public int TotalRulesEvaluated { get; set; }

    /// <summary>
    /// Number of rules that would execute (all conditions passed)
    /// </summary>
    [JsonPropertyName("rulesThatWouldExecute")]
    public int RulesThatWouldExecute { get; set; }

    /// <summary>
    /// Total number of predicted rewards
    /// </summary>
    [JsonPropertyName("totalPredictedRewards")]
    public int TotalPredictedRewards { get; set; }

    /// <summary>
    /// Total evaluation time in milliseconds
    /// </summary>
    [JsonPropertyName("totalEvaluationTimeMs")]
    public long TotalEvaluationTimeMs { get; set; }

    /// <summary>
    /// Whether the event was valid for processing
    /// </summary>
    [JsonPropertyName("eventValid")]
    public bool EventValid { get; set; }

    /// <summary>
    /// Any validation errors encountered
    /// </summary>
    [JsonPropertyName("validationErrors")]
    public IEnumerable<string> ValidationErrors { get; set; } = new List<string>();
}

/// <summary>
/// Data transfer object for dry-run response
/// </summary>
public sealed class DryRunResponseDto
{
    /// <summary>
    /// ID of the trigger event that was evaluated
    /// </summary>
    [JsonPropertyName("triggerEventId")]
    public string TriggerEventId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user for whom the evaluation was performed
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Type of the trigger event
    /// </summary>
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// List of rule traces for all evaluated rules
    /// </summary>
    [JsonPropertyName("rules")]
    public IEnumerable<RuleTrace> Rules { get; set; } = new List<RuleTrace>();

    /// <summary>
    /// Summary of the dry-run evaluation
    /// </summary>
    [JsonPropertyName("summary")]
    public DryRunSummary Summary { get; set; } = new DryRunSummary();

    /// <summary>
    /// Timestamp when the dry-run was performed
    /// </summary>
    [JsonPropertyName("evaluatedAt")]
    public DateTimeOffset EvaluatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Request model for dry-run evaluation (reuses IngestEventRequest structure)
/// </summary>
public sealed class DryRunRequestDto
{
    /// <summary>
    /// Optional event ID. If not provided, a new GUID will be generated
    /// </summary>
    [JsonPropertyName("eventId")]
    public string? EventId { get; set; }

    /// <summary>
    /// The type of event (e.g., "USER_COMMENTED", "PRODUCT_PURCHASED")
    /// </summary>
    [JsonPropertyName("eventType")]
    [Required]
    [MinLength(1)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user who performed the action
    /// </summary>
    [JsonPropertyName("userId")]
    [Required]
    [MinLength(1)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// When the event occurred. If not provided, current UTC time will be used
    /// </summary>
    [JsonPropertyName("occurredAt")]
    public DateTimeOffset? OccurredAt { get; set; }

    /// <summary>
    /// Additional attributes for the event
    /// </summary>
    [JsonPropertyName("attributes")]
    public Dictionary<string, object>? Attributes { get; set; }
}
