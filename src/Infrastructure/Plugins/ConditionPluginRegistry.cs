using GamificationEngine.Domain.Rules.Plugins;

namespace GamificationEngine.Infrastructure.Plugins;

/// <summary>
/// Thread-safe implementation of the condition plugin registry
/// </summary>
public class ConditionPluginRegistry : IConditionPluginRegistry
{
    private readonly Dictionary<string, IConditionPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public bool RegisterPlugin(IConditionPlugin plugin)
    {
        if (plugin == null)
            throw new ArgumentNullException(nameof(plugin));

        if (string.IsNullOrWhiteSpace(plugin.ConditionType))
            throw new ArgumentException("Plugin condition type cannot be empty", nameof(plugin));

        lock (_lock)
        {
            if (_plugins.ContainsKey(plugin.ConditionType))
            {
                return false; // Already registered
            }

            _plugins[plugin.ConditionType] = plugin;
            return true;
        }
    }

    public bool UnregisterPlugin(string conditionType)
    {
        if (string.IsNullOrWhiteSpace(conditionType))
            throw new ArgumentException("Condition type cannot be empty", nameof(conditionType));

        lock (_lock)
        {
            return _plugins.Remove(conditionType);
        }
    }

    public IConditionPlugin? GetPlugin(string conditionType)
    {
        if (string.IsNullOrWhiteSpace(conditionType))
            throw new ArgumentException("Condition type cannot be empty", nameof(conditionType));

        lock (_lock)
        {
            _plugins.TryGetValue(conditionType, out var plugin);
            return plugin;
        }
    }

    public IEnumerable<IConditionPlugin> GetAllPlugins()
    {
        lock (_lock)
        {
            return _plugins.Values.ToList();
        }
    }

    public bool IsRegistered(string conditionType)
    {
        if (string.IsNullOrWhiteSpace(conditionType))
            throw new ArgumentException("Condition type cannot be empty", nameof(conditionType));

        lock (_lock)
        {
            return _plugins.ContainsKey(conditionType);
        }
    }

    public int GetPluginCount()
    {
        lock (_lock)
        {
            return _plugins.Count;
        }
    }

    public void ClearPlugins()
    {
        lock (_lock)
        {
            _plugins.Clear();
        }
    }

    public IEnumerable<string> ValidatePlugins()
    {
        var errors = new List<string>();

        lock (_lock)
        {
            foreach (var plugin in _plugins.Values)
            {
                try
                {
                    // Validate plugin metadata
                    if (string.IsNullOrWhiteSpace(plugin.ConditionType))
                        errors.Add($"Plugin {plugin.GetType().Name} has empty condition type");

                    if (string.IsNullOrWhiteSpace(plugin.DisplayName))
                        errors.Add($"Plugin {plugin.ConditionType} has empty display name");

                    if (string.IsNullOrWhiteSpace(plugin.Description))
                        errors.Add($"Plugin {plugin.ConditionType} has empty description");

                    if (string.IsNullOrWhiteSpace(plugin.Version))
                        errors.Add($"Plugin {plugin.ConditionType} has empty version");

                    // Test parameter validation with empty parameters
                    if (!plugin.ValidateParameters(new Dictionary<string, object>()))
                    {
                        errors.Add($"Plugin {plugin.ConditionType} failed parameter validation test");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Plugin {plugin.ConditionType} validation failed: {ex.Message}");
                }
            }
        }

        return errors;
    }
}
