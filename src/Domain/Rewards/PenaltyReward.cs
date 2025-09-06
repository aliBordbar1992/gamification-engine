namespace GamificationEngine.Domain.Rewards;

/// <summary>
/// Represents a penalty reward that can be applied to users (points reduction, badge removal, restrictions)
/// </summary>
public class PenaltyReward : Reward
{
    // EF Core requires a parameterless constructor
    protected PenaltyReward() { }

    public PenaltyReward(string rewardId, string penaltyType, string targetId, long? amount = null, IDictionary<string, object>? parameters = null)
        : base(rewardId, "penalty", parameters)
    {
        if (string.IsNullOrWhiteSpace(penaltyType)) throw new ArgumentException("penaltyType cannot be empty", nameof(penaltyType));
        if (string.IsNullOrWhiteSpace(targetId)) throw new ArgumentException("targetId cannot be empty", nameof(targetId));

        PenaltyType = penaltyType;
        TargetId = targetId;
        Amount = amount;
    }

    public string PenaltyType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public long? Amount { get; set; }

    /// <summary>
    /// Gets the type of penalty to apply
    /// </summary>
    public string GetPenaltyType() => PenaltyType;

    /// <summary>
    /// Gets the target ID (badge ID, category, etc.)
    /// </summary>
    public string GetTargetId() => TargetId;

    /// <summary>
    /// Gets the amount for penalties that require it (e.g., point reduction)
    /// </summary>
    public long? GetAmount() => Amount;

    /// <summary>
    /// Validates the penalty reward configuration
    /// </summary>
    public override bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(PenaltyType) || string.IsNullOrWhiteSpace(TargetId))
            return false;

        // For point penalties, amount must be specified
        if (PenaltyType.Equals("points", StringComparison.OrdinalIgnoreCase) && !Amount.HasValue)
            return false;

        return true;
    }
}
