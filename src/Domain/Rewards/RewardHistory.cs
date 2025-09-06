namespace GamificationEngine.Domain.Rewards;

/// <summary>
/// Represents a historical record of a reward being awarded to a user
/// </summary>
public class RewardHistory
{
    // EF Core requires a parameterless constructor
    protected RewardHistory() { }

    public RewardHistory(string rewardHistoryId, string userId, string rewardId, string rewardType,
        string triggerEventId, DateTimeOffset awardedAt, bool success, string? message = null,
        IDictionary<string, object>? details = null)
    {
        if (string.IsNullOrWhiteSpace(rewardHistoryId)) throw new ArgumentException("rewardHistoryId cannot be empty", nameof(rewardHistoryId));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(rewardId)) throw new ArgumentException("rewardId cannot be empty", nameof(rewardId));
        if (string.IsNullOrWhiteSpace(rewardType)) throw new ArgumentException("rewardType cannot be empty", nameof(rewardType));
        if (string.IsNullOrWhiteSpace(triggerEventId)) throw new ArgumentException("triggerEventId cannot be empty", nameof(triggerEventId));

        RewardHistoryId = rewardHistoryId;
        UserId = userId;
        RewardId = rewardId;
        RewardType = rewardType;
        TriggerEventId = triggerEventId;
        AwardedAt = awardedAt;
        Success = success;
        Message = message;
        Details = details?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();
    }

    public string RewardHistoryId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string RewardId { get; set; } = string.Empty;
    public string RewardType { get; set; } = string.Empty;
    public string TriggerEventId { get; set; } = string.Empty;
    public DateTimeOffset AwardedAt { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public IReadOnlyDictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
}
