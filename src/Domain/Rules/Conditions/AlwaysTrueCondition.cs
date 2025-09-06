using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Rules.Conditions;

/// <summary>
/// A condition that always evaluates to true
/// </summary>
public class AlwaysTrueCondition : Condition
{
    public AlwaysTrueCondition() : base() { }

    public AlwaysTrueCondition(string conditionId, IDictionary<string, object>? parameters = null)
        : base(conditionId, ConditionTypes.AlwaysTrue, parameters)
    {
    }

    public override bool Evaluate(IEnumerable<Event> events, Event triggerEvent)
    {
        return true;
    }
}
