using GamificationEngine.Domain.Rewards.Plugins;

namespace GamificationEngine.Domain.Rewards.Plugins;

/// <summary>
/// Registry for managing reward plugins
/// </summary>
public interface IRewardPluginRegistry
{
    /// <summary>
    /// Registers a reward plugin
    /// </summary>
    /// <param name="plugin">The plugin to register</param>
    /// <returns>True if registration was successful, false otherwise</returns>
    bool RegisterPlugin(IRewardPlugin plugin);

    /// <summary>
    /// Unregisters a reward plugin
    /// </summary>
    /// <param name="rewardType">The reward type to unregister</param>
    /// <returns>True if unregistration was successful, false otherwise</returns>
    bool UnregisterPlugin(string rewardType);

    /// <summary>
    /// Gets a registered plugin by reward type
    /// </summary>
    /// <param name="rewardType">The reward type</param>
    /// <returns>The plugin if found, null otherwise</returns>
    IRewardPlugin? GetPlugin(string rewardType);

    /// <summary>
    /// Gets all registered plugins
    /// </summary>
    /// <returns>Collection of all registered plugins</returns>
    IEnumerable<IRewardPlugin> GetAllPlugins();

    /// <summary>
    /// Checks if a reward type is registered
    /// </summary>
    /// <param name="rewardType">The reward type to check</param>
    /// <returns>True if registered, false otherwise</returns>
    bool IsRegistered(string rewardType);

    /// <summary>
    /// Gets the count of registered plugins
    /// </summary>
    /// <returns>Number of registered plugins</returns>
    int GetPluginCount();

    /// <summary>
    /// Clears all registered plugins
    /// </summary>
    void ClearPlugins();

    /// <summary>
    /// Validates all registered plugins
    /// </summary>
    /// <returns>Collection of validation errors</returns>
    IEnumerable<string> ValidatePlugins();
}
