namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data transfer object for wallet information
/// </summary>
public sealed class WalletDto
{
    public string UserId { get; set; } = string.Empty;
    public string PointCategoryId { get; set; } = string.Empty;
    public long Balance { get; set; }
    public List<WalletTransactionDto> Transactions { get; set; } = new();
}

/// <summary>
/// Data transfer object for wallet transactions
/// </summary>
public sealed class WalletTransactionDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string PointCategoryId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? Metadata { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Data transfer object for wallet transfers
/// </summary>
public sealed class WalletTransferDto
{
    public string Id { get; set; } = string.Empty;
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public string PointCategoryId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? Metadata { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }
}

/// <summary>
/// Data transfer object for spending points request
/// </summary>
public sealed class SpendPointsRequestDto
{
    public string PointCategoryId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// Data transfer object for transferring points request
/// </summary>
public sealed class TransferPointsRequestDto
{
    public string ToUserId { get; set; } = string.Empty;
    public string PointCategoryId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? Metadata { get; set; }
}
