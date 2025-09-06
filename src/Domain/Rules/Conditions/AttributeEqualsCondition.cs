using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Rules.Conditions;

/// <summary>
/// A condition that checks if an event attribute equals a specific value
/// </summary>
public class AttributeEqualsCondition : Condition
{
    public AttributeEqualsCondition() : base() { }

    public AttributeEqualsCondition(string conditionId, IDictionary<string, object>? parameters = null)
        : base(conditionId, ConditionTypes.AttributeEquals, parameters)
    {
    }

    public override bool Evaluate(IEnumerable<Event> events, Event triggerEvent)
    {
        if (!Parameters.TryGetValue("attributeName", out var attributeNameObj) || attributeNameObj is not string attributeName)
            return false;

        if (!Parameters.TryGetValue("expectedValue", out var expectedValue))
            return false;

        // Check if the trigger event has the specified attribute with the expected value
        if (!triggerEvent.Attributes.TryGetValue(attributeName, out var actualValue))
            return false;

        return actualValue?.Equals(expectedValue) == true;
    }
}
