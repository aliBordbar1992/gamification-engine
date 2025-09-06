using GamificationEngine.Domain.Rules.Conditions;

namespace GamificationEngine.Domain.Rules;

/// <summary>
/// Factory for creating condition instances based on type
/// </summary>
public class ConditionFactory
{
    /// <summary>
    /// Creates a condition instance based on the specified type
    /// </summary>
    /// <param name="conditionId">The unique identifier for the condition</param>
    /// <param name="type">The type of condition to create</param>
    /// <param name="parameters">Parameters for the condition</param>
    /// <returns>A condition instance</returns>
    public Condition CreateCondition(string conditionId, string type, IDictionary<string, object>? parameters = null)
    {
        return type switch
        {
            ConditionTypes.AlwaysTrue => new AlwaysTrueCondition(conditionId, parameters),
            ConditionTypes.AttributeEquals => new AttributeEqualsCondition(conditionId, parameters),
            ConditionTypes.Count => new CountCondition(conditionId, parameters),
            ConditionTypes.Threshold => new ThresholdCondition(conditionId, parameters),
            ConditionTypes.Sequence => new SequenceCondition(conditionId, parameters),
            ConditionTypes.TimeSinceLastEvent => new TimeSinceLastEventCondition(conditionId, parameters),
            ConditionTypes.FirstOccurrence => new FirstOccurrenceCondition(conditionId, parameters),
            ConditionTypes.CustomScript => new CustomScriptCondition(conditionId, parameters),
            _ => throw new ArgumentException($"Unknown condition type: {type}", nameof(type))
        };
    }
}
