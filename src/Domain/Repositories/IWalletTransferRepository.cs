using GamificationEngine.Domain.Wallet;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository interface for managing wallet transfers
/// </summary>
public interface IWalletTransferRepository
{
    /// <summary>
    /// Gets a transfer by ID
    /// </summary>
    /// <param name="id">The transfer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transfer or null if not found</returns>
    Task<WalletTransfer?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transfers for a specific user (as sender or receiver)
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transfers involving the user</returns>
    Task<IEnumerable<WalletTransfer>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transfers sent by a specific user
    /// </summary>
    /// <param name="fromUserId">The sender user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transfers sent by the user</returns>
    Task<IEnumerable<WalletTransfer>> GetSentByUserAsync(string fromUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transfers received by a specific user
    /// </summary>
    /// <param name="toUserId">The receiver user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transfers received by the user</returns>
    Task<IEnumerable<WalletTransfer>> GetReceivedByUserAsync(string toUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transfers by status
    /// </summary>
    /// <param name="status">The transfer status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transfers with the specified status</returns>
    Task<IEnumerable<WalletTransfer>> GetByStatusAsync(WalletTransferStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transfers within a date range
    /// </summary>
    /// <param name="from">Start date (inclusive)</param>
    /// <param name="to">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transfers in the date range</returns>
    Task<IEnumerable<WalletTransfer>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transfers by reference ID
    /// </summary>
    /// <param name="referenceId">The reference ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transfers with the reference ID</returns>
    Task<IEnumerable<WalletTransfer>> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new transfer
    /// </summary>
    /// <param name="transfer">The transfer to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task AddAsync(WalletTransfer transfer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing transfer
    /// </summary>
    /// <param name="transfer">The transfer to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task UpdateAsync(WalletTransfer transfer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending transfers that need processing
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of pending transfers</returns>
    Task<IEnumerable<WalletTransfer>> GetPendingTransfersAsync(CancellationToken cancellationToken = default);
}
