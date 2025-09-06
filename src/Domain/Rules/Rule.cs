using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Rewards;

namespace GamificationEngine.Domain.Rules;

/// <summary>
/// Represents a rule that can be evaluated against events
/// </summary>
public class Rule
{
    // EF Core requires a parameterless constructor
    protected Rule() { }

    public Rule(string ruleId, string name, string[] triggers, IReadOnlyCollection<Condition> conditions, IReadOnlyCollection<Reward> rewards,
        bool isActive = true, string? description = null, DateTimeOffset? createdAt = null, DateTimeOffset? updatedAt = null)
    {
        if (string.IsNullOrWhiteSpace(ruleId)) throw new ArgumentException("ruleId cannot be empty", nameof(ruleId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name cannot be empty", nameof(name));
        if (triggers == null || triggers.Length == 0) throw new ArgumentException("triggers cannot be null or empty", nameof(triggers));
        if (conditions == null || conditions.Count == 0) throw new ArgumentException("conditions cannot be null or empty", nameof(conditions));
        if (rewards == null || rewards.Count == 0) throw new ArgumentException("rewards cannot be null or empty", nameof(rewards));

        RuleId = ruleId;
        Name = name;
        Triggers = triggers;
        Conditions = conditions;
        Rewards = rewards;
        IsActive = isActive;
        Description = description;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }

    public string RuleId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string[] Triggers { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<Condition> Conditions { get; set; } = new List<Condition>();
    public IReadOnlyCollection<Reward> Rewards { get; set; } = new List<Reward>();
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Checks if the rule should be triggered by the given event type
    /// </summary>
    /// <param name="eventType">The event type to check</param>
    /// <returns>True if the rule should be triggered, false otherwise</returns>
    public bool ShouldTrigger(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType)) return false;
        if (!IsActive) return false;

        return Triggers.Contains(eventType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Evaluates all conditions in the rule against the provided events
    /// </summary>
    /// <param name="events">The events to evaluate against</param>
    /// <param name="triggerEvent">The specific event that triggered the rule evaluation</param>
    /// <returns>True if all conditions are satisfied, false otherwise</returns>
    public bool EvaluateConditions(IEnumerable<Event> events, Event triggerEvent)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));
        if (triggerEvent == null) throw new ArgumentNullException(nameof(triggerEvent));
        if (!IsActive) return false;

        return Conditions.All(condition => condition.Evaluate(events, triggerEvent));
    }

    /// <summary>
    /// Validates the rule configuration for consistency and completeness
    /// </summary>
    /// <returns>True if the rule is valid, false otherwise</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(RuleId)) return false;
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (Triggers.Length == 0) return false;
        if (Conditions.Count == 0) return false;
        if (Rewards.Count == 0) return false;

        // Validate that all conditions have valid IDs and types
        if (Conditions.Any(c => string.IsNullOrWhiteSpace(c.ConditionId) || string.IsNullOrWhiteSpace(c.Type)))
            return false;

        // Validate that all rewards have valid IDs and types
        if (Rewards.Any(r => string.IsNullOrWhiteSpace(r.RewardId) || string.IsNullOrWhiteSpace(r.Type)))
            return false;

        return true;
    }

    /// <summary>
    /// Gets a summary of the rule for logging and debugging purposes
    /// </summary>
    /// <returns>A string representation of the rule summary</returns>
    public string GetSummary()
    {
        return $"Rule '{Name}' (ID: {RuleId}) - Triggers: [{string.Join(", ", Triggers)}], " +
               $"Conditions: {Conditions.Count}, Rewards: {Rewards.Count}, Active: {IsActive}";
    }
}