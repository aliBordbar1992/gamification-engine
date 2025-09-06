namespace GamificationEngine.Domain.Rewards;

/// <summary>
/// Represents a badge reward that can be awarded to users
/// </summary>
public class BadgeReward : Reward
{
    // EF Core requires a parameterless constructor
    protected BadgeReward() { }

    public BadgeReward(string rewardId, string badgeId, IDictionary<string, object>? parameters = null)
        : base(rewardId, "badge", parameters)
    {
        if (string.IsNullOrWhiteSpace(badgeId)) throw new ArgumentException("badgeId cannot be empty", nameof(badgeId));

        BadgeId = badgeId;
    }

    public string BadgeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets the badge ID to award
    /// </summary>
    public string GetBadgeId() => BadgeId;

    /// <summary>
    /// Validates the badge reward configuration
    /// </summary>
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(BadgeId);
    }
}
