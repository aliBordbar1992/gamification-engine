using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Rules.Plugins;

/// <summary>
/// Interface for condition plugins that can be dynamically loaded
/// </summary>
public interface IConditionPlugin
{
    /// <summary>
    /// Gets the unique identifier for this condition type
    /// </summary>
    string ConditionType { get; }

    /// <summary>
    /// Gets the display name for this condition type
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the description of what this condition does
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the version of this plugin
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the required parameters for this condition type
    /// </summary>
    /// <returns>Dictionary of parameter names and their descriptions</returns>
    IDictionary<string, string> GetRequiredParameters();

    /// <summary>
    /// Gets the optional parameters for this condition type
    /// </summary>
    /// <returns>Dictionary of parameter names and their descriptions</returns>
    IDictionary<string, string> GetOptionalParameters();

    /// <summary>
    /// Validates the parameters for this condition type
    /// </summary>
    /// <param name="parameters">The parameters to validate</param>
    /// <returns>True if parameters are valid, false otherwise</returns>
    bool ValidateParameters(IDictionary<string, object> parameters);

    /// <summary>
    /// Creates a condition instance with the given parameters
    /// </summary>
    /// <param name="conditionId">The unique identifier for the condition</param>
    /// <param name="parameters">The parameters for the condition</param>
    /// <returns>A condition instance</returns>
    Condition CreateCondition(string conditionId, IDictionary<string, object>? parameters = null);

    /// <summary>
    /// Gets the plugin metadata
    /// </summary>
    /// <returns>Plugin metadata information</returns>
    PluginMetadata GetMetadata();
}

/// <summary>
/// Metadata information for a plugin
/// </summary>
public record PluginMetadata(
    string Name,
    string Version,
    string Author,
    string Description,
    DateTime CreatedAt,
    string[] Tags,
    string[] Dependencies
);
