namespace GamificationEngine.Domain.Wallet;

/// <summary>
/// Represents a transaction in a user's wallet
/// </summary>
public sealed class WalletTransaction
{
    // EF Core requires a parameterless constructor
    private WalletTransaction() { }

    public WalletTransaction(
        string id,
        string userId,
        string pointCategoryId,
        long amount,
        WalletTransactionType type,
        string description,
        string? referenceId = null,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("ID cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(pointCategoryId)) throw new ArgumentException("Point category ID cannot be empty", nameof(pointCategoryId));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty", nameof(description));

        Id = id;
        UserId = userId;
        PointCategoryId = pointCategoryId;
        Amount = amount;
        Type = type;
        Description = description;
        ReferenceId = referenceId;
        Metadata = metadata;
        Timestamp = DateTime.UtcNow;
    }

    public string Id { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public string PointCategoryId { get; private set; } = string.Empty;
    public long Amount { get; private set; }
    public WalletTransactionType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ReferenceId { get; private set; }
    public string? Metadata { get; private set; }
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Updates the transaction metadata
    /// </summary>
    /// <param name="metadata">New metadata</param>
    public void UpdateMetadata(string? metadata)
    {
        Metadata = metadata;
    }
}

/// <summary>
/// Types of wallet transactions
/// </summary>
public enum WalletTransactionType
{
    /// <summary>
    /// Points earned from rewards
    /// </summary>
    Earned = 0,

    /// <summary>
    /// Points spent on purchases or services
    /// </summary>
    Spent = 1,

    /// <summary>
    /// Points transferred to another user
    /// </summary>
    TransferOut = 2,

    /// <summary>
    /// Points received from another user
    /// </summary>
    TransferIn = 3,

    /// <summary>
    /// Points refunded from a cancelled transaction
    /// </summary>
    Refund = 4,

    /// <summary>
    /// Points deducted as a penalty
    /// </summary>
    Penalty = 5,

    /// <summary>
    /// Points adjusted by admin
    /// </summary>
    Adjustment = 6
}
