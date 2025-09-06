namespace GamificationEngine.Domain.Rewards;

/// <summary>
/// Factory for creating reward instances from configuration data
/// </summary>
public static class RewardFactory
{
    /// <summary>
    /// Creates a reward instance from configuration data
    /// </summary>
    /// <param name="rewardId">The reward ID</param>
    /// <param name="rewardType">The type of reward to create</param>
    /// <param name="parameters">Configuration parameters for the reward</param>
    /// <returns>A reward instance</returns>
    /// <exception cref="ArgumentException">Thrown when the reward type is not supported or parameters are invalid</exception>
    public static Reward CreateReward(string rewardId, string rewardType, IDictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(rewardId)) throw new ArgumentException("rewardId cannot be empty", nameof(rewardId));
        if (string.IsNullOrWhiteSpace(rewardType)) throw new ArgumentException("rewardType cannot be empty", nameof(rewardType));

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

    private static PointsReward CreatePointsReward(string rewardId, IDictionary<string, object>? parameters)
    {
        if (parameters == null || !parameters.TryGetValue("category", out var categoryObj) || categoryObj is not string category)
            throw new ArgumentException("Points reward requires 'category' parameter");

        if (!parameters.TryGetValue("amount", out var amountObj) || amountObj is not long amount)
            throw new ArgumentException("Points reward requires 'amount' parameter");

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
