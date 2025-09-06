namespace GamificationEngine.Domain.Rewards;

/// <summary>
/// Represents a trophy reward that can be awarded to users (meta-badges)
/// </summary>
public class TrophyReward : Reward
{
    // EF Core requires a parameterless constructor
    protected TrophyReward() { }

    public TrophyReward(string rewardId, string trophyId, IDictionary<string, object>? parameters = null)
        : base(rewardId, "trophy", parameters)
    {
        if (string.IsNullOrWhiteSpace(trophyId)) throw new ArgumentException("trophyId cannot be empty", nameof(trophyId));

        TrophyId = trophyId;
    }

    public string TrophyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets the trophy ID to award
    /// </summary>
    public string GetTrophyId() => TrophyId;

    /// <summary>
    /// Validates the trophy reward configuration
    /// </summary>
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(TrophyId);
    }
}
