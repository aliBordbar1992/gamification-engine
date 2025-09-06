using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Rules.Conditions;

/// <summary>
/// A condition that checks if the count of events of a specific type meets a threshold
/// </summary>
public class CountCondition : Condition
{
    public CountCondition() : base() { }

    public CountCondition(string conditionId, IDictionary<string, object>? parameters = null)
        : base(conditionId, ConditionTypes.Count, parameters)
    {
    }

    public override bool Evaluate(IEnumerable<Event> events, Event triggerEvent)
    {
        if (!Parameters.TryGetValue("eventType", out var eventTypeObj) || eventTypeObj is not string eventType)
            return false;

        if (!Parameters.TryGetValue("minCount", out var minCountObj) || !int.TryParse(minCountObj.ToString(), out var minCount))
            return false;

        // Get time window if specified
        TimeSpan? timeWindow = null;
        if (Parameters.TryGetValue("timeWindowMinutes", out var timeWindowObj) && int.TryParse(timeWindowObj.ToString(), out var minutes))
        {
            timeWindow = TimeSpan.FromMinutes(minutes);
        }

        // Filter events by type and user
        var relevantEvents = events.Where(e => e.EventType == eventType && e.UserId == triggerEvent.UserId);

        // Apply time window if specified
        if (timeWindow.HasValue)
        {
            var cutoffTime = triggerEvent.OccurredAt.Subtract(timeWindow.Value);
            relevantEvents = relevantEvents.Where(e => e.OccurredAt >= cutoffTime);
        }

        var eventCount = relevantEvents.Count();
        return eventCount >= minCount;
    }
}
