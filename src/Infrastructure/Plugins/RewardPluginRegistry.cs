using GamificationEngine.Domain.Rewards.Plugins;

namespace GamificationEngine.Infrastructure.Plugins;

/// <summary>
/// Thread-safe implementation of the reward plugin registry
/// </summary>
public class RewardPluginRegistry : IRewardPluginRegistry
{
    private readonly Dictionary<string, IRewardPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public bool RegisterPlugin(IRewardPlugin plugin)
    {
        if (plugin == null)
            throw new ArgumentNullException(nameof(plugin));

        if (string.IsNullOrWhiteSpace(plugin.RewardType))
            throw new ArgumentException("Plugin reward type cannot be empty", nameof(plugin));

        lock (_lock)
        {
            if (_plugins.ContainsKey(plugin.RewardType))
            {
                return false; // Already registered
            }

            _plugins[plugin.RewardType] = plugin;
            return true;
        }
    }

    public bool UnregisterPlugin(string rewardType)
    {
        if (string.IsNullOrWhiteSpace(rewardType))
            throw new ArgumentException("Reward type cannot be empty", nameof(rewardType));

        lock (_lock)
        {
            return _plugins.Remove(rewardType);
        }
    }

    public IRewardPlugin? GetPlugin(string rewardType)
    {
        if (string.IsNullOrWhiteSpace(rewardType))
            throw new ArgumentException("Reward type cannot be empty", nameof(rewardType));

        lock (_lock)
        {
            _plugins.TryGetValue(rewardType, out var plugin);
            return plugin;
        }
    }

    public IEnumerable<IRewardPlugin> GetAllPlugins()
    {
        lock (_lock)
        {
            return _plugins.Values.ToList();
        }
    }

    public bool IsRegistered(string rewardType)
    {
        if (string.IsNullOrWhiteSpace(rewardType))
            throw new ArgumentException("Reward type cannot be empty", nameof(rewardType));

        lock (_lock)
        {
            return _plugins.ContainsKey(rewardType);
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
                    if (string.IsNullOrWhiteSpace(plugin.RewardType))
                        errors.Add($"Plugin {plugin.GetType().Name} has empty reward type");

                    if (string.IsNullOrWhiteSpace(plugin.DisplayName))
                        errors.Add($"Plugin {plugin.RewardType} has empty display name");

                    if (string.IsNullOrWhiteSpace(plugin.Description))
                        errors.Add($"Plugin {plugin.RewardType} has empty description");

                    if (string.IsNullOrWhiteSpace(plugin.Version))
                        errors.Add($"Plugin {plugin.RewardType} has empty version");

                    // Test parameter validation with empty parameters
                    if (!plugin.ValidateParameters(new Dictionary<string, object>()))
                    {
                        errors.Add($"Plugin {plugin.RewardType} failed parameter validation test");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Plugin {plugin.RewardType} validation failed: {ex.Message}");
                }
            }
        }

        return errors;
    }
}
