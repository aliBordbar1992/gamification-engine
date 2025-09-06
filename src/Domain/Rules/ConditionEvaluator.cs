using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Rules;

/// <summary>
/// Evaluates collections of conditions with AND/OR logic
/// </summary>
public class ConditionEvaluator
{
    /// <summary>
    /// Evaluates a collection of conditions using the specified logic
    /// </summary>
    /// <param name="conditions">The conditions to evaluate</param>
    /// <param name="events">The events to evaluate against</param>
    /// <param name="triggerEvent">The event that triggered the evaluation</param>
    /// <param name="logic">The logic to use: "all" (AND) or "any" (OR)</param>
    /// <returns>True if the condition group is satisfied</returns>
    public bool EvaluateConditions(
        IEnumerable<Condition> conditions,
        IEnumerable<Event> events,
        Event triggerEvent,
        string logic = "all")
    {
        var conditionList = conditions.ToList();

        if (!conditionList.Any())
            return true; // No conditions means always true

        return logic.ToLowerInvariant() switch
        {
            "all" => conditionList.All(condition => condition.Evaluate(events, triggerEvent)),
            "any" => conditionList.Any(condition => condition.Evaluate(events, triggerEvent)),
            _ => throw new ArgumentException($"Invalid logic '{logic}'. Must be 'all' or 'any'.", nameof(logic))
        };
    }
}
