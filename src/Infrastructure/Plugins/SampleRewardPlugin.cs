using GamificationEngine.Domain.Rewards;
using GamificationEngine.Domain.Rewards.Plugins;
using GamificationEngine.Domain.Rules.Plugins;

namespace GamificationEngine.Infrastructure.Plugins;

/// <summary>
/// Sample reward plugin that demonstrates the plugin system
/// This reward gives bonus points based on a multiplier
/// </summary>
public class SampleRewardPlugin : IRewardPlugin
{
    public string RewardType => "bonusPoints";

    public string DisplayName => "Bonus Points";

    public string Description => "Awards bonus points based on a multiplier applied to the base points";

    public string Version => "1.0.0";

    public IDictionary<string, string> GetRequiredParameters()
    {
        return new Dictionary<string, string>
        {
            { "category", "The point category to award bonus points in" },
            { "baseAmount", "The base amount of points to multiply" },
            { "multiplier", "The multiplier to apply to the base amount" }
        };
    }

    public IDictionary<string, string> GetOptionalParameters()
    {
        return new Dictionary<string, string>
        {
            { "maxBonus", "Maximum bonus points that can be awarded (default: unlimited)" },
            { "reason", "Reason for the bonus points" }
        };
    }

    public bool ValidateParameters(IDictionary<string, object> parameters)
    {
        if (parameters == null)
            return false;

        // Check required parameters
        if (!parameters.TryGetValue("category", out var categoryObj) || categoryObj is not string || string.IsNullOrWhiteSpace((string)categoryObj))
            return false;

        if (!parameters.TryGetValue("baseAmount", out var baseAmountObj) || !(baseAmountObj is int || baseAmountObj is long))
            return false;

        var baseAmount = Convert.ToInt64(baseAmountObj);
        if (baseAmount <= 0)
            return false;

        if (!parameters.TryGetValue("multiplier", out var multiplierObj) || !(multiplierObj is double || multiplierObj is int || multiplierObj is long))
            return false;

        var multiplier = Convert.ToDouble(multiplierObj);
        if (multiplier <= 0)
            return false;

        // Check optional parameters if provided
        if (parameters.TryGetValue("maxBonus", out var maxBonusObj))
        {
            if (!(maxBonusObj is int || maxBonusObj is long))
                return false;
            if (Convert.ToInt64(maxBonusObj) < 0)
                return false;
        }

        return true;
    }

    public Reward CreateReward(string rewardId, IDictionary<string, object>? parameters = null)
    {
        return new BonusPointsReward(rewardId, parameters);
    }

    public PluginMetadata GetMetadata()
    {
        return new PluginMetadata(
            Name: "Bonus Points Plugin",
            Version: Version,
            Author: "Gamification Engine Team",
            Description: Description,
            CreatedAt: DateTime.UtcNow,
            Tags: new[] { "bonus", "points", "multiplier", "sample" },
            Dependencies: Array.Empty<string>()
        );
    }
}

/// <summary>
/// Sample reward implementation that awards bonus points based on a multiplier
/// </summary>
public class BonusPointsReward : Reward
{
    public BonusPointsReward() : base() { }

    public BonusPointsReward(string rewardId, IDictionary<string, object>? parameters = null)
        : base(rewardId, "bonusPoints", parameters)
    {
    }

    /// <summary>
    /// Gets the point category for this bonus reward
    /// </summary>
    public string GetCategory()
    {
        return Parameters.TryGetValue("category", out var categoryObj) && categoryObj is string category
            ? category
            : "xp";
    }

    /// <summary>
    /// Gets the base amount of points
    /// </summary>
    public long GetBaseAmount()
    {
        return Parameters.TryGetValue("baseAmount", out var baseAmountObj) && baseAmountObj is long baseAmount
            ? baseAmount
            : 0;
    }

    /// <summary>
    /// Gets the multiplier to apply
    /// </summary>
    public double GetMultiplier()
    {
        return Parameters.TryGetValue("multiplier", out var multiplierObj)
            ? Convert.ToDouble(multiplierObj)
            : 1.0;
    }

    /// <summary>
    /// Gets the maximum bonus points allowed
    /// </summary>
    public long? GetMaxBonus()
    {
        return Parameters.TryGetValue("maxBonus", out var maxBonusObj) && maxBonusObj is long maxBonus
            ? maxBonus
            : null;
    }

    /// <summary>
    /// Gets the reason for the bonus
    /// </summary>
    public string GetReason()
    {
        return Parameters.TryGetValue("reason", out var reasonObj) && reasonObj is string reason
            ? reason
            : "Bonus points awarded";
    }

    /// <summary>
    /// Calculates the actual bonus points to award
    /// </summary>
    public long CalculateBonusPoints()
    {
        var baseAmount = GetBaseAmount();
        var multiplier = GetMultiplier();
        var bonusPoints = (long)(baseAmount * multiplier);
        var maxBonus = GetMaxBonus();

        if (maxBonus.HasValue && bonusPoints > maxBonus.Value)
        {
            return maxBonus.Value;
        }

        return bonusPoints;
    }

    /// <summary>
    /// Validates the bonus points reward configuration
    /// </summary>
    public override bool IsValid()
    {
        if (!base.IsValid())
            return false;

        try
        {
            var category = GetCategory();
            var baseAmount = GetBaseAmount();
            var multiplier = GetMultiplier();

            return !string.IsNullOrWhiteSpace(category) && baseAmount > 0 && multiplier > 0;
        }
        catch
        {
            return false;
        }
    }
}
