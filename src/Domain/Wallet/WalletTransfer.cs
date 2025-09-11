namespace GamificationEngine.Domain.Wallet;

/// <summary>
/// Represents a transfer of points between users
/// </summary>
public sealed class WalletTransfer
{
    // EF Core requires a parameterless constructor
    private WalletTransfer() { }

    public WalletTransfer(
        string id,
        string fromUserId,
        string toUserId,
        string pointCategoryId,
        long amount,
        string description,
        string? referenceId = null,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("ID cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(fromUserId)) throw new ArgumentException("From user ID cannot be empty", nameof(fromUserId));
        if (string.IsNullOrWhiteSpace(toUserId)) throw new ArgumentException("To user ID cannot be empty", nameof(toUserId));
        if (string.IsNullOrWhiteSpace(pointCategoryId)) throw new ArgumentException("Point category ID cannot be empty", nameof(pointCategoryId));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty", nameof(description));
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));

        Id = id;
        FromUserId = fromUserId;
        ToUserId = toUserId;
        PointCategoryId = pointCategoryId;
        Amount = amount;
        Description = description;
        ReferenceId = referenceId;
        Metadata = metadata;
        Status = WalletTransferStatus.Pending;
        Timestamp = DateTime.UtcNow;
    }

    public string Id { get; private set; } = string.Empty;
    public string FromUserId { get; private set; } = string.Empty;
    public string ToUserId { get; private set; } = string.Empty;
    public string PointCategoryId { get; private set; } = string.Empty;
    public long Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ReferenceId { get; private set; }
    public string? Metadata { get; private set; }
    public WalletTransferStatus Status { get; private set; }
    public DateTime Timestamp { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; }

    /// <summary>
    /// Marks the transfer as completed
    /// </summary>
    public void MarkCompleted()
    {
        if (Status != WalletTransferStatus.Pending)
            throw new InvalidOperationException($"Cannot complete transfer in status: {Status}");

        Status = WalletTransferStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the transfer as failed
    /// </summary>
    /// <param name="reason">Reason for failure</param>
    public void MarkFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Failure reason cannot be empty", nameof(reason));
        if (Status != WalletTransferStatus.Pending)
            throw new InvalidOperationException($"Cannot fail transfer in status: {Status}");

        Status = WalletTransferStatus.Failed;
        FailureReason = reason;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the transfer
    /// </summary>
    public void Cancel()
    {
        if (Status != WalletTransferStatus.Pending)
            throw new InvalidOperationException($"Cannot cancel transfer in status: {Status}");

        Status = WalletTransferStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Status of a wallet transfer
/// </summary>
public enum WalletTransferStatus
{
    /// <summary>
    /// Transfer is pending processing
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Transfer completed successfully
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Transfer failed
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Transfer was cancelled
    /// </summary>
    Cancelled = 3
}
