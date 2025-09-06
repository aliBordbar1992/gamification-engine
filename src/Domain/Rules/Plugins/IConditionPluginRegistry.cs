using GamificationEngine.Domain.Rules.Plugins;

namespace GamificationEngine.Domain.Rules.Plugins;

/// <summary>
/// Registry for managing condition plugins
/// </summary>
public interface IConditionPluginRegistry
{
    /// <summary>
    /// Registers a condition plugin
    /// </summary>
    /// <param name="plugin">The plugin to register</param>
    /// <returns>True if registration was successful, false otherwise</returns>
    bool RegisterPlugin(IConditionPlugin plugin);

    /// <summary>
    /// Unregisters a condition plugin
    /// </summary>
    /// <param name="conditionType">The condition type to unregister</param>
    /// <returns>True if unregistration was successful, false otherwise</returns>
    bool UnregisterPlugin(string conditionType);

    /// <summary>
    /// Gets a registered plugin by condition type
    /// </summary>
    /// <param name="conditionType">The condition type</param>
    /// <returns>The plugin if found, null otherwise</returns>
    IConditionPlugin? GetPlugin(string conditionType);

    /// <summary>
    /// Gets all registered plugins
    /// </summary>
    /// <returns>Collection of all registered plugins</returns>
    IEnumerable<IConditionPlugin> GetAllPlugins();

    /// <summary>
    /// Checks if a condition type is registered
    /// </summary>
    /// <param name="conditionType">The condition type to check</param>
    /// <returns>True if registered, false otherwise</returns>
    bool IsRegistered(string conditionType);

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
