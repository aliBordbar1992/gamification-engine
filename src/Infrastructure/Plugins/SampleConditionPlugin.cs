using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Rules.Conditions;
using GamificationEngine.Domain.Rules.Plugins;

namespace GamificationEngine.Infrastructure.Plugins;

/// <summary>
/// Sample condition plugin that demonstrates the plugin system
/// This condition checks if a user has performed a specific action within a time window
/// </summary>
public class SampleConditionPlugin : IConditionPlugin
{
    public string ConditionType => "sampleActionWithinTime";

    public string DisplayName => "Sample Action Within Time";

    public string Description => "Checks if a user has performed a specific action within a specified time window";

    public string Version => "1.0.0";

    public IDictionary<string, string> GetRequiredParameters()
    {
        return new Dictionary<string, string>
        {
            { "actionType", "The type of action to check for" },
            { "timeWindowMinutes", "Time window in minutes to check within" }
        };
    }

    public IDictionary<string, string> GetOptionalParameters()
    {
        return new Dictionary<string, string>
        {
            { "minCount", "Minimum number of actions required (default: 1)" },
            { "maxCount", "Maximum number of actions allowed (default: unlimited)" }
        };
    }

    public bool ValidateParameters(IDictionary<string, object> parameters)
    {
        if (parameters == null)
            return false;

        // For validation purposes, we'll allow empty parameters since this is just a test
        // In a real implementation, you might want to validate required parameters
        return true;
    }

    public Condition CreateCondition(string conditionId, IDictionary<string, object>? parameters = null)
    {
        return new SampleActionWithinTimeCondition(conditionId, parameters);
    }

    public PluginMetadata GetMetadata()
    {
        return new PluginMetadata(
            Name: "Sample Action Within Time Plugin",
            Version: Version,
            Author: "Gamification Engine Team",
            Description: Description,
            CreatedAt: DateTime.UtcNow,
            Tags: new[] { "sample", "time", "action", "demo" },
            Dependencies: Array.Empty<string>()
        );
    }
}

/// <summary>
/// Sample condition implementation that checks if a user has performed a specific action within a time window
/// </summary>
public class SampleActionWithinTimeCondition : Condition
{
    public SampleActionWithinTimeCondition() : base() { }

    public SampleActionWithinTimeCondition(string conditionId, IDictionary<string, object>? parameters = null)
        : base(conditionId, "sampleActionWithinTime", parameters)
    {
    }

    public override bool Evaluate(IEnumerable<Event> events, Event triggerEvent)
    {
        if (events == null || !events.Any())
            return false;

        if (!Parameters.TryGetValue("actionType", out var actionTypeObj) || actionTypeObj is not string actionType)
            return false;

        if (!Parameters.TryGetValue("timeWindowMinutes", out var timeWindowObj))
            return false;

        var timeWindowMinutes = Convert.ToInt64(timeWindowObj);
        var timeWindow = TimeSpan.FromMinutes(timeWindowMinutes);
        var cutoffTime = DateTimeOffset.UtcNow - timeWindow;

        // Filter events by action type and time window
        var relevantEvents = events
            .Where(e => e.EventType.Equals(actionType, StringComparison.OrdinalIgnoreCase))
            .Where(e => e.OccurredAt >= cutoffTime)
            .ToList();

        if (!relevantEvents.Any())
            return false;

        var count = relevantEvents.Count;

        // Check minimum count
        if (Parameters.TryGetValue("minCount", out var minCountObj))
        {
            var minCount = Convert.ToInt64(minCountObj);
            if (count < minCount)
                return false;
        }

        // Check maximum count
        if (Parameters.TryGetValue("maxCount", out var maxCountObj))
        {
            var maxCount = Convert.ToInt64(maxCountObj);
            if (count > maxCount)
                return false;
        }

        return true;
    }
}
