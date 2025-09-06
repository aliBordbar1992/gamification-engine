using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Rules.Conditions;

/// <summary>
/// A condition that checks if enough time has passed since the last event of a specific type
/// </summary>
public class TimeSinceLastEventCondition : Condition
{
    public TimeSinceLastEventCondition() : base() { }

    public TimeSinceLastEventCondition(string conditionId, IDictionary<string, object>? parameters = null)
        : base(conditionId, ConditionTypes.TimeSinceLastEvent, parameters)
    {
    }

    public override bool Evaluate(IEnumerable<Event> events, Event triggerEvent)
    {
        if (!Parameters.TryGetValue("eventType", out var eventTypeObj) || eventTypeObj is not string eventType)
            return false;

        if (!Parameters.TryGetValue("minMinutes", out var minMinutesObj) || !int.TryParse(minMinutesObj.ToString(), out var minMinutes))
            return false;

        var minTimeSpan = TimeSpan.FromMinutes(minMinutes);

        // Find the most recent event of the specified type for this user (excluding the trigger event)
        var lastEvent = events
            .Where(e => e.EventType == eventType &&
                       e.UserId == triggerEvent.UserId &&
                       e.EventId != triggerEvent.EventId)
            .OrderByDescending(e => e.OccurredAt)
            .FirstOrDefault();

        // If no previous event exists, condition is satisfied
        if (lastEvent == null)
            return true;

        // Check if enough time has passed
        var timeSinceLastEvent = triggerEvent.OccurredAt - lastEvent.OccurredAt;
        return timeSinceLastEvent >= minTimeSpan;
    }
}
