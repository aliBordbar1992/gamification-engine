using GamificationEngine.Domain.Rewards.Plugins;

namespace GamificationEngine.Domain.Rewards;

/// <summary>
/// Factory for creating reward instances from configuration data
/// </summary>
public class RewardFactory
{
    private readonly IRewardPluginRegistry? _pluginRegistry;

    public RewardFactory(IRewardPluginRegistry? pluginRegistry = null)
    {
        _pluginRegistry = pluginRegistry;
    }

    /// <summary>
    /// Creates a reward instance from configuration data
    /// </summary>
    /// <param name="rewardId">The reward ID</param>
    /// <param name="rewardType">The type of reward to create</param>
    /// <param name="parameters">Configuration parameters for the reward</param>
    /// <returns>A reward instance</returns>
    /// <exception cref="ArgumentException">Thrown when the reward type is not supported or parameters are invalid</exception>
    public Reward CreateReward(string rewardId, string rewardType, IDictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(rewardId)) throw new ArgumentException("rewardId cannot be empty", nameof(rewardId));
        if (string.IsNullOrWhiteSpace(rewardType)) throw new ArgumentException("rewardType cannot be empty", nameof(rewardType));

        // First check if there's a plugin for this reward type
        if (_pluginRegistry != null && _pluginRegistry.IsRegistered(rewardType))
        {
            var plugin = _pluginRegistry.GetPlugin(rewardType);
            if (plugin != null)
            {
                // Validate parameters using the plugin
                if (!plugin.ValidateParameters(parameters ?? new Dictionary<string, object>()))
                {
                    throw new ArgumentException($"Invalid parameters for reward type '{rewardType}'", nameof(parameters));
                }

                return plugin.CreateReward(rewardId, parameters);
            }
        }

        // Fall back to built-in reward types
        return rewardType.ToLowerInvariant() switch
        {
            "points" => CreatePointsReward(rewardId, parameters),
            "badge" => CreateBadgeReward(rewardId, parameters),
            "trophy" => CreateTrophyReward(rewardId, parameters),
            "level" => CreateLevelReward(rewardId, parameters),
            "penalty" => CreatePenaltyReward(rewardId, parameters),
            _ => throw new ArgumentException($"Unsupported reward type: {rewardType}", nameof(rewardType))
        };
    }

    /// <summary>
    /// Gets all available reward types (built-in + plugins)
    /// </summary>
    /// <returns>Collection of available reward types</returns>
    public IEnumerable<string> GetAvailableRewardTypes()
    {
        var builtInTypes = new[] { "points", "badge", "trophy", "level", "penalty" };
        var pluginTypes = _pluginRegistry?.GetAllPlugins().Select(p => p.RewardType) ?? Enumerable.Empty<string>();

        return builtInTypes.Concat(pluginTypes).Distinct();
    }

    /// <summary>
    /// Gets metadata for a reward type
    /// </summary>
    /// <param name="rewardType">The reward type</param>
    /// <returns>Reward metadata if available</returns>
    public RewardMetadata? GetRewardMetadata(string rewardType)
    {
        if (_pluginRegistry != null && _pluginRegistry.IsRegistered(rewardType))
        {
            var plugin = _pluginRegistry.GetPlugin(rewardType);
            if (plugin != null)
            {
                return new RewardMetadata(
                    plugin.RewardType,
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
        return GetBuiltInRewardMetadata(rewardType);
    }

    private static RewardMetadata? GetBuiltInRewardMetadata(string rewardType)
    {
        return rewardType.ToLowerInvariant() switch
        {
            "points" => new RewardMetadata(
                "points",
                "Points",
                "Awards points to a user in a specific category",
                "1.0.0",
                new Dictionary<string, string> { { "category", "The point category" }, { "amount", "The number of points to award" } },
                new Dictionary<string, string>(),
                false
            ),
            "badge" => new RewardMetadata(
                "badge",
                "Badge",
                "Awards a badge to a user",
                "1.0.0",
                new Dictionary<string, string> { { "badgeId", "The ID of the badge to award" } },
                new Dictionary<string, string>(),
                false
            ),
            "trophy" => new RewardMetadata(
                "trophy",
                "Trophy",
                "Awards a trophy to a user",
                "1.0.0",
                new Dictionary<string, string> { { "trophyId", "The ID of the trophy to award" } },
                new Dictionary<string, string>(),
                false
            ),
            "level" => new RewardMetadata(
                "level",
                "Level",
                "Awards a level to a user",
                "1.0.0",
                new Dictionary<string, string> { { "levelId", "The ID of the level to award" } },
                new Dictionary<string, string> { { "category", "The point category (default: xp)" } },
                false
            ),
            "penalty" => new RewardMetadata(
                "penalty",
                "Penalty",
                "Applies a penalty to a user",
                "1.0.0",
                new Dictionary<string, string> { { "penaltyType", "The type of penalty" }, { "targetId", "The target ID for the penalty" } },
                new Dictionary<string, string> { { "amount", "The amount for penalties that require it" } },
                false
            ),
            _ => null
        };
    }

    private static PointsReward CreatePointsReward(string rewardId, IDictionary<string, object>? parameters)
    {
        if (parameters == null || !parameters.TryGetValue("category", out var categoryObj) || categoryObj is not string category)
            throw new ArgumentException("Points reward requires 'category' parameter");

        if (!parameters.TryGetValue("amount", out var amountObj))
            throw new ArgumentException("Points reward requires 'amount' parameter");

        long amount = amountObj switch
        {
            long l => l,
            int i => i,
            double d => (long)d,
            decimal dec => (long)dec,
            float f => (long)f,
            string s when long.TryParse(s, out var parsed) => parsed,
            string s when int.TryParse(s, out var parsed) => parsed,
            string s => throw new ArgumentException($"Points reward 'amount' parameter must be a valid number, got string '{s}'"),
            _ => throw new ArgumentException($"Points reward 'amount' parameter must be a number, got {amountObj?.GetType().Name ?? "null"} with value '{amountObj}'")
        };

        return new PointsReward(rewardId, category, amount, parameters);
    }

    private static BadgeReward CreateBadgeReward(string rewardId, IDictionary<string, object>? parameters)
    {
        if (parameters == null || !parameters.TryGetValue("badgeId", out var badgeIdObj) || badgeIdObj is not string badgeId)
            throw new ArgumentException("Badge reward requires 'badgeId' parameter");

        return new BadgeReward(rewardId, badgeId, parameters);
    }

    private static TrophyReward CreateTrophyReward(string rewardId, IDictionary<string, object>? parameters)
    {
        if (parameters == null || !parameters.TryGetValue("trophyId", out var trophyIdObj) || trophyIdObj is not string trophyId)
            throw new ArgumentException("Trophy reward requires 'trophyId' parameter");

        return new TrophyReward(rewardId, trophyId, parameters);
    }

    private static LevelReward CreateLevelReward(string rewardId, IDictionary<string, object>? parameters)
    {
        if (parameters == null || !parameters.TryGetValue("levelId", out var levelIdObj) || levelIdObj is not string levelId)
            throw new ArgumentException("Level reward requires 'levelId' parameter");

        var category = parameters.TryGetValue("category", out var categoryObj) && categoryObj is string cat ? cat : "xp";
        return new LevelReward(rewardId, levelId, category, parameters);
    }

    private static PenaltyReward CreatePenaltyReward(string rewardId, IDictionary<string, object>? parameters)
    {
        if (parameters == null || !parameters.TryGetValue("penaltyType", out var penaltyTypeObj) || penaltyTypeObj is not string penaltyType)
            throw new ArgumentException("Penalty reward requires 'penaltyType' parameter");

        if (!parameters.TryGetValue("targetId", out var targetIdObj) || targetIdObj is not string targetId)
            throw new ArgumentException("Penalty reward requires 'targetId' parameter");

        var amount = parameters.TryGetValue("amount", out var amountObj) && amountObj is long amt ? amt : (long?)null;
        return new PenaltyReward(rewardId, penaltyType, targetId, amount, parameters);
    }
}

/// <summary>
/// Metadata for a reward type
/// </summary>
public record RewardMetadata(
    string Type,
    string DisplayName,
    string Description,
    string Version,
    IDictionary<string, string> RequiredParameters,
    IDictionary<string, string> OptionalParameters,
    bool IsPlugin
);
