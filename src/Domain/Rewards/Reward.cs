namespace GamificationEngine.Domain.Rewards;

/// <summary>
/// Base class for all rule rewards
/// </summary>
public abstract class Reward
{
    // EF Core requires a parameterless constructor
    protected Reward() { }

    protected Reward(string rewardId, string type, IDictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(rewardId)) throw new ArgumentException("rewardId cannot be empty", nameof(rewardId));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("type cannot be empty", nameof(type));

        RewardId = rewardId;
        Type = type;
        Parameters = parameters?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();
    }

    public string RewardId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
}