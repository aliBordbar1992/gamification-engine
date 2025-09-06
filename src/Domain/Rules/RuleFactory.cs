using GamificationEngine.Domain.Rewards;

namespace GamificationEngine.Domain.Rules;

/// <summary>
/// Factory for creating Rule instances from configuration data
/// </summary>
public static class RuleFactory
{
    /// <summary>
    /// Creates a Rule instance from configuration data
    /// </summary>
    /// <param name="ruleConfig">The rule configuration data</param>
    /// <param name="conditionFactory">Factory for creating conditions</param>
    /// <param name="rewardFactory">Factory for creating rewards</param>
    /// <returns>A new Rule instance</returns>
    /// <exception cref="ArgumentException">Thrown when the configuration is invalid</exception>
    public static Rule CreateFromConfiguration(
        RuleConfiguration ruleConfig,
        Func<string, string, IDictionary<string, object>?, Condition> conditionFactory,
        Func<string, string, IDictionary<string, object>?, Reward> rewardFactory)
    {
        if (ruleConfig == null) throw new ArgumentNullException(nameof(ruleConfig));
        if (conditionFactory == null) throw new ArgumentNullException(nameof(conditionFactory));
        if (rewardFactory == null) throw new ArgumentNullException(nameof(rewardFactory));

        // Validate required fields
        if (string.IsNullOrWhiteSpace(ruleConfig.RuleId))
            throw new ArgumentException("RuleId is required", nameof(ruleConfig));
        if (string.IsNullOrWhiteSpace(ruleConfig.Name))
            throw new ArgumentException("Name is required", nameof(ruleConfig));
        if (ruleConfig.Triggers == null || ruleConfig.Triggers.Length == 0)
            throw new ArgumentException("Triggers are required", nameof(ruleConfig));
        if (ruleConfig.Conditions == null || ruleConfig.Conditions.Length == 0)
            throw new ArgumentException("Conditions are required", nameof(ruleConfig));
        if (ruleConfig.Rewards == null || ruleConfig.Rewards.Length == 0)
            throw new ArgumentException("Rewards are required", nameof(ruleConfig));

        // Create conditions
        var conditions = new List<Condition>();
        foreach (var conditionConfig in ruleConfig.Conditions)
        {
            var condition = conditionFactory(conditionConfig.ConditionId, conditionConfig.Type, conditionConfig.Parameters);
            conditions.Add(condition);
        }

        // Create rewards
        var rewards = new List<Reward>();
        foreach (var rewardConfig in ruleConfig.Rewards)
        {
            var reward = rewardFactory(rewardConfig.RewardId, rewardConfig.Type, rewardConfig.Parameters);
            rewards.Add(reward);
        }

        // Create the rule
        return new Rule(
            ruleConfig.RuleId,
            ruleConfig.Name,
            ruleConfig.Triggers,
            conditions,
            rewards,
            ruleConfig.IsActive,
            ruleConfig.Description,
            ruleConfig.CreatedAt,
            ruleConfig.UpdatedAt);
    }

    /// <summary>
    /// Creates multiple Rule instances from a collection of configuration data
    /// </summary>
    /// <param name="ruleConfigs">The rule configuration data collection</param>
    /// <param name="conditionFactory">Factory for creating conditions</param>
    /// <param name="rewardFactory">Factory for creating rewards</param>
    /// <returns>A collection of new Rule instances</returns>
    public static IEnumerable<Rule> CreateFromConfiguration(
        IEnumerable<RuleConfiguration> ruleConfigs,
        Func<string, string, IDictionary<string, object>?, Condition> conditionFactory,
        Func<string, string, IDictionary<string, object>?, Reward> rewardFactory)
    {
        if (ruleConfigs == null) throw new ArgumentNullException(nameof(ruleConfigs));

        return ruleConfigs.Select(config => CreateFromConfiguration(config, conditionFactory, rewardFactory));
    }
}

/// <summary>
/// Configuration data structure for rules
/// </summary>
public record RuleConfiguration
{
    public string RuleId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string[] Triggers { get; init; } = Array.Empty<string>();
    public ConditionConfiguration[] Conditions { get; init; } = Array.Empty<ConditionConfiguration>();
    public RewardConfiguration[] Rewards { get; init; } = Array.Empty<RewardConfiguration>();
    public bool IsActive { get; init; } = true;
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>
/// Configuration data structure for conditions
/// </summary>
public record ConditionConfiguration
{
    public string ConditionId { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public IDictionary<string, object>? Parameters { get; init; }
}

/// <summary>
/// Configuration data structure for rewards
/// </summary>
public record RewardConfiguration
{
    public string RewardId { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public IDictionary<string, object>? Parameters { get; init; }
}
