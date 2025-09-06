using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Rules.Conditions;

/// <summary>
/// A condition that evaluates custom logic (placeholder for future implementation)
/// </summary>
public class CustomScriptCondition : Condition
{
    public CustomScriptCondition() : base() { }

    public CustomScriptCondition(string conditionId, IDictionary<string, object>? parameters = null)
        : base(conditionId, ConditionTypes.CustomScript, parameters)
    {
    }

    public override bool Evaluate(IEnumerable<Event> events, Event triggerEvent)
    {
        // For now, always return false as this is a placeholder
        // In the future, this could evaluate JavaScript, Lua, or other scripting languages
        return false;
    }
}
