using GamificationEngine.Domain.Rewards;
using GamificationEngine.Domain.Rules.Plugins;

namespace GamificationEngine.Domain.Rewards.Plugins;

/// <summary>
/// Interface for reward plugins that can be dynamically loaded
/// </summary>
public interface IRewardPlugin
{
    /// <summary>
    /// Gets the unique identifier for this reward type
    /// </summary>
    string RewardType { get; }

    /// <summary>
    /// Gets the display name for this reward type
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the description of what this reward does
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the version of this plugin
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the required parameters for this reward type
    /// </summary>
    /// <returns>Dictionary of parameter names and their descriptions</returns>
    IDictionary<string, string> GetRequiredParameters();

    /// <summary>
    /// Gets the optional parameters for this reward type
    /// </summary>
    /// <returns>Dictionary of parameter names and their descriptions</returns>
    IDictionary<string, string> GetOptionalParameters();

    /// <summary>
    /// Validates the parameters for this reward type
    /// </summary>
    /// <param name="parameters">The parameters to validate</param>
    /// <returns>True if parameters are valid, false otherwise</returns>
    bool ValidateParameters(IDictionary<string, object> parameters);

    /// <summary>
    /// Creates a reward instance with the given parameters
    /// </summary>
    /// <param name="rewardId">The unique identifier for the reward</param>
    /// <param name="parameters">The parameters for the reward</param>
    /// <returns>A reward instance</returns>
    Reward CreateReward(string rewardId, IDictionary<string, object>? parameters = null);

    /// <summary>
    /// Gets the plugin metadata
    /// </summary>
    /// <returns>Plugin metadata information</returns>
    PluginMetadata GetMetadata();
}
