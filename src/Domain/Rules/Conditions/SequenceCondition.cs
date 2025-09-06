using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Rules.Conditions;

/// <summary>
/// A condition that checks if a sequence of events occurred in order
/// </summary>
public class SequenceCondition : Condition
{
    public SequenceCondition() : base() { }

    public SequenceCondition(string conditionId, IDictionary<string, object>? parameters = null)
        : base(conditionId, ConditionTypes.Sequence, parameters)
    {
    }

    public override bool Evaluate(IEnumerable<Event> events, Event triggerEvent)
    {
        if (!Parameters.TryGetValue("eventTypes", out var eventTypesObj))
            return false;

        List<string> requiredSequence;
        try
        {
            if (eventTypesObj is List<object> objectList)
            {
                requiredSequence = objectList.Cast<string>().ToList();
            }
            else if (eventTypesObj is string[] stringArray)
            {
                requiredSequence = stringArray.ToList();
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        if (requiredSequence.Count == 0)
            return false;

        // Get time window if specified (default to 24 hours)
        var timeWindowMinutes = 24 * 60; // Default 24 hours
        if (Parameters.TryGetValue("timeWindowMinutes", out var timeWindowObj) && int.TryParse(timeWindowObj.ToString(), out var minutes))
        {
            timeWindowMinutes = minutes;
        }

        var timeWindow = TimeSpan.FromMinutes(timeWindowMinutes);
        var cutoffTime = triggerEvent.OccurredAt.Subtract(timeWindow);

        // Get events for the user within the time window, ordered by occurrence time
        var userEvents = events
            .Where(e => e.UserId == triggerEvent.UserId && e.OccurredAt >= cutoffTime && e.OccurredAt <= triggerEvent.OccurredAt)
            .OrderBy(e => e.OccurredAt)
            .ToList();

        // Check if the sequence exists
        return ContainsSequence(userEvents, requiredSequence);
    }

    private static bool ContainsSequence(List<Event> events, List<string> requiredSequence)
    {
        if (requiredSequence.Count > events.Count)
            return false;

        var sequenceIndex = 0;

        foreach (var evt in events)
        {
            if (evt.EventType == requiredSequence[sequenceIndex])
            {
                sequenceIndex++;
                if (sequenceIndex == requiredSequence.Count)
                    return true;
            }
        }

        return false;
    }
}
