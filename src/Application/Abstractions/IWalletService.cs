using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Wallet;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for managing user wallets and transactions
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Gets a wallet for a specific user and point category
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The wallet information</returns>
    Task<Result<WalletDto, string>> GetWalletAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all wallets for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of user wallets</returns>
    Task<Result<IEnumerable<WalletDto>, string>> GetUserWalletsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the balance for a specific user and point category
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The current balance</returns>
    Task<Result<long, string>> GetBalanceAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Spends points from a user's wallet
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="amount">The amount to spend</param>
    /// <param name="description">Description of the spending</param>
    /// <param name="referenceId">Optional reference ID for tracking</param>
    /// <param name="metadata">Optional metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created transaction</returns>
    Task<Result<WalletTransactionDto, string>> SpendPointsAsync(string userId, string pointCategoryId, long amount, string description, string? referenceId = null, string? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfers points from one user to another
    /// </summary>
    /// <param name="fromUserId">The sender user ID</param>
    /// <param name="toUserId">The receiver user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="amount">The amount to transfer</param>
    /// <param name="description">Description of the transfer</param>
    /// <param name="referenceId">Optional reference ID for tracking</param>
    /// <param name="metadata">Optional metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created transfer</returns>
    Task<Result<WalletTransferDto, string>> TransferPointsAsync(string fromUserId, string toUserId, string pointCategoryId, long amount, string description, string? referenceId = null, string? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transaction history for a user's wallet
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="from">Optional start date filter</param>
    /// <param name="to">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions</returns>
    Task<Result<IEnumerable<WalletTransactionDto>, string>> GetTransactionHistoryAsync(string userId, string pointCategoryId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a transaction to a user's wallet
    /// </summary>
    /// <param name="transaction">The transaction to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool, string>> AddTransactionAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a wallet transfer
    /// </summary>
    /// <param name="transfer">The transfer to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool, string>> TransferAsync(WalletTransfer transfer, CancellationToken cancellationToken = default);
}
