using GamificationEngine.Domain.Rules.Conditions;
using GamificationEngine.Domain.Rules.Plugins;

namespace GamificationEngine.Domain.Rules;

/// <summary>
/// Factory for creating condition instances based on type
/// </summary>
public class ConditionFactory
{
    private readonly IConditionPluginRegistry? _pluginRegistry;

    public ConditionFactory(IConditionPluginRegistry? pluginRegistry = null)
    {
        _pluginRegistry = pluginRegistry;
    }

    /// <summary>
    /// Creates a condition instance based on the specified type
    /// </summary>
    /// <param name="conditionId">The unique identifier for the condition</param>
    /// <param name="type">The type of condition to create</param>
    /// <param name="parameters">Parameters for the condition</param>
    /// <returns>A condition instance</returns>
    public Condition CreateCondition(string conditionId, string type, IDictionary<string, object>? parameters = null)
    {
        // First check if there's a plugin for this condition type
        if (_pluginRegistry != null && _pluginRegistry.IsRegistered(type))
        {
            var plugin = _pluginRegistry.GetPlugin(type);
            if (plugin != null)
            {
                // Validate parameters using the plugin
                if (!plugin.ValidateParameters(parameters ?? new Dictionary<string, object>()))
                {
                    throw new ArgumentException($"Invalid parameters for condition type '{type}'", nameof(parameters));
                }

                return plugin.CreateCondition(conditionId, parameters);
            }
        }

        // Fall back to built-in condition types
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

    /// <summary>
    /// Gets all available condition types (built-in + plugins)
    /// </summary>
    /// <returns>Collection of available condition types</returns>
    public IEnumerable<string> GetAvailableConditionTypes()
    {
        var builtInTypes = new[]
        {
            ConditionTypes.AlwaysTrue,
            ConditionTypes.AttributeEquals,
            ConditionTypes.Count,
            ConditionTypes.Threshold,
            ConditionTypes.Sequence,
            ConditionTypes.TimeSinceLastEvent,
            ConditionTypes.FirstOccurrence,
            ConditionTypes.CustomScript
        };

        var pluginTypes = _pluginRegistry?.GetAllPlugins().Select(p => p.ConditionType) ?? Enumerable.Empty<string>();

        return builtInTypes.Concat(pluginTypes).Distinct();
    }

    /// <summary>
    /// Gets metadata for a condition type
    /// </summary>
    /// <param name="type">The condition type</param>
    /// <returns>Condition metadata if available</returns>
    public ConditionMetadata? GetConditionMetadata(string type)
    {
        if (_pluginRegistry != null && _pluginRegistry.IsRegistered(type))
        {
            var plugin = _pluginRegistry.GetPlugin(type);
            if (plugin != null)
            {
                return new ConditionMetadata(
                    plugin.ConditionType,
                    plugin.DisplayName,
                    plugin.Description,
                    plugin.Version,
                    plugin.GetRequiredParameters(),
                    plugin.GetOptionalParameters(),
                    true // isPlugin
                );
            }
        }

        // Return metadata for built-in types
        return GetBuiltInConditionMetadata(type);
    }

    private static ConditionMetadata? GetBuiltInConditionMetadata(string type)
    {
        return type switch
        {
            ConditionTypes.AlwaysTrue => new ConditionMetadata(
                ConditionTypes.AlwaysTrue,
                "Always True",
                "A condition that always evaluates to true",
                "1.0.0",
                new Dictionary<string, string>(),
                new Dictionary<string, string>(),
                false
            ),
            ConditionTypes.AttributeEquals => new ConditionMetadata(
                ConditionTypes.AttributeEquals,
                "Attribute Equals",
                "Checks if an event attribute equals a specific value",
                "1.0.0",
                new Dictionary<string, string> { { "attribute", "The attribute name to check" }, { "value", "The expected value" } },
                new Dictionary<string, string>(),
                false
            ),
            ConditionTypes.Count => new ConditionMetadata(
                ConditionTypes.Count,
                "Count",
                "Counts events of a specific type",
                "1.0.0",
                new Dictionary<string, string> { { "eventType", "The event type to count" }, { "minCount", "Minimum count required" } },
                new Dictionary<string, string> { { "maxCount", "Maximum count allowed" }, { "timeWindow", "Time window in minutes" } },
                false
            ),
            ConditionTypes.Threshold => new ConditionMetadata(
                ConditionTypes.Threshold,
                "Threshold",
                "Compares numeric attributes against thresholds",
                "1.0.0",
                new Dictionary<string, string> { { "attribute", "The attribute name" }, { "operator", "Comparison operator" }, { "value", "Threshold value" } },
                new Dictionary<string, string>(),
                false
            ),
            ConditionTypes.Sequence => new ConditionMetadata(
                ConditionTypes.Sequence,
                "Sequence",
                "Validates ordered sequence of events",
                "1.0.0",
                new Dictionary<string, string> { { "events", "Array of event types in sequence" } },
                new Dictionary<string, string> { { "timeWindow", "Time window in minutes" } },
                false
            ),
            ConditionTypes.TimeSinceLastEvent => new ConditionMetadata(
                ConditionTypes.TimeSinceLastEvent,
                "Time Since Last Event",
                "Ensures minimum time has passed since last event",
                "1.0.0",
                new Dictionary<string, string> { { "eventType", "The event type to check" }, { "minMinutes", "Minimum minutes required" } },
                new Dictionary<string, string>(),
                false
            ),
            ConditionTypes.FirstOccurrence => new ConditionMetadata(
                ConditionTypes.FirstOccurrence,
                "First Occurrence",
                "Checks if this is the first occurrence of an event type",
                "1.0.0",
                new Dictionary<string, string> { { "eventType", "The event type to check" } },
                new Dictionary<string, string>(),
                false
            ),
            ConditionTypes.CustomScript => new ConditionMetadata(
                ConditionTypes.CustomScript,
                "Custom Script",
                "Evaluates custom logic (placeholder for future implementation)",
                "1.0.0",
                new Dictionary<string, string>(),
                new Dictionary<string, string>(),
                false
            ),
            _ => null
        };
    }
}

/// <summary>
/// Metadata for a condition type
/// </summary>
public record ConditionMetadata(
    string Type,
    string DisplayName,
    string Description,
    string Version,
    IDictionary<string, string> RequiredParameters,
    IDictionary<string, string> OptionalParameters,
    bool IsPlugin
);
