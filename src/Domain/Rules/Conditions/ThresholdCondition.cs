using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Rules.Conditions;

/// <summary>
/// A condition that checks if a numeric attribute meets a threshold
/// </summary>
public class ThresholdCondition : Condition
{
    public ThresholdCondition() : base() { }

    public ThresholdCondition(string conditionId, IDictionary<string, object>? parameters = null)
        : base(conditionId, ConditionTypes.Threshold, parameters)
    {
    }

    public override bool Evaluate(IEnumerable<Event> events, Event triggerEvent)
    {
        if (!Parameters.TryGetValue("attributeName", out var attributeNameObj) || attributeNameObj is not string attributeName)
            return false;

        if (!Parameters.TryGetValue("threshold", out var thresholdObj) || !decimal.TryParse(thresholdObj.ToString(), out var threshold))
            return false;

        var operation = Parameters.TryGetValue("operation", out var operationObj) ? operationObj.ToString() : ">=";

        // Check if the trigger event has the specified attribute
        if (!triggerEvent.Attributes.TryGetValue(attributeName, out var actualValueObj))
            return false;

        if (!decimal.TryParse(actualValueObj.ToString(), out var actualValue))
            return false;

        return operation switch
        {
            ">=" => actualValue >= threshold,
            ">" => actualValue > threshold,
            "<=" => actualValue <= threshold,
            "<" => actualValue < threshold,
            "=" or "==" => actualValue == threshold,
            "!=" => actualValue != threshold,
            _ => false
        };
    }
}
