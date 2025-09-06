namespace GamificationEngine.Domain.Rewards;

/// <summary>
/// Represents a level reward that can be awarded to users based on point thresholds
/// </summary>
public class LevelReward : Reward
{
    // EF Core requires a parameterless constructor
    protected LevelReward() { }

    public LevelReward(string rewardId, string levelId, string? category = null, IDictionary<string, object>? parameters = null)
        : base(rewardId, "level", parameters)
    {
        if (string.IsNullOrWhiteSpace(levelId)) throw new ArgumentException("levelId cannot be empty", nameof(levelId));

        LevelId = levelId;
        Category = category ?? "xp"; // Default to XP category
    }

    public string LevelId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets the level ID to award
    /// </summary>
    public string GetLevelId() => LevelId;

    /// <summary>
    /// Gets the category for level calculation
    /// </summary>
    public string GetCategory() => Category;

    /// <summary>
    /// Validates the level reward configuration
    /// </summary>
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(LevelId) && !string.IsNullOrWhiteSpace(Category);
    }
}
