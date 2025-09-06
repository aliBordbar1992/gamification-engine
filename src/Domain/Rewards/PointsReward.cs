namespace GamificationEngine.Domain.Rewards;

/// <summary>
/// Represents a points reward that can be awarded to users
/// </summary>
public class PointsReward : Reward
{
    // EF Core requires a parameterless constructor
    protected PointsReward() { }

    public PointsReward(string rewardId, string category, long amount, IDictionary<string, object>? parameters = null)
        : base(rewardId, "points", parameters)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("category cannot be empty", nameof(category));
        if (amount == 0) throw new ArgumentException("amount cannot be zero", nameof(amount));

        Category = category;
        Amount = amount;
    }

    public string Category { get; set; } = string.Empty;
    public long Amount { get; set; }

    /// <summary>
    /// Gets the amount of points to award (can be negative for penalties)
    /// </summary>
    public long GetPointsAmount() => Amount;

    /// <summary>
    /// Gets the category of points to award
    /// </summary>
    public string GetCategory() => Category;

    /// <summary>
    /// Validates the points reward configuration
    /// </summary>
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Category) && Amount != 0;
    }
}
