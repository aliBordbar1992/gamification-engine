using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Rules.Conditions;

/// <summary>
/// A condition that checks if this is the first occurrence of an event type for a user
/// </summary>
public class FirstOccurrenceCondition : Condition
{
    public FirstOccurrenceCondition() : base() { }

    public FirstOccurrenceCondition(string conditionId, IDictionary<string, object>? parameters = null)
        : base(conditionId, ConditionTypes.FirstOccurrence, parameters)
    {
    }

    public override bool Evaluate(IEnumerable<Event> events, Event triggerEvent)
    {
        var eventType = triggerEvent.EventType;

        // If eventType parameter is specified, use that instead
        if (Parameters.TryGetValue("eventType", out var eventTypeObj) && eventTypeObj is string specifiedEventType)
        {
            eventType = specifiedEventType;
        }

        // Check if there are any previous events of this type for this user
        var previousEvents = events
            .Where(e => e.EventType == eventType &&
                       e.UserId == triggerEvent.UserId &&
                       e.OccurredAt < triggerEvent.OccurredAt)
            .Any();

        return !previousEvents;
    }
}
