using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Rules;

/// <summary>
/// Base class for all rule conditions
/// </summary>
public abstract class Condition
{
    // EF Core requires a parameterless constructor
    protected Condition() { }

    protected Condition(string conditionId, string type, IDictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(conditionId)) throw new ArgumentException("conditionId cannot be empty", nameof(conditionId));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("type cannot be empty", nameof(type));

        ConditionId = conditionId;
        Type = type;
        Parameters = parameters?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();
    }

    public string ConditionId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Evaluates the condition against a collection of events
    /// </summary>
    /// <param name="events">The events to evaluate against</param>
    /// <param name="triggerEvent">The specific event that triggered the rule evaluation</param>
    /// <returns>True if the condition is satisfied, false otherwise</returns>
    public abstract bool Evaluate(IEnumerable<Event> events, Event triggerEvent);
}