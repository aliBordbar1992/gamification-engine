using GamificationEngine.Domain.Wallet;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository interface for managing wallet transactions
/// </summary>
public interface IWalletTransactionRepository
{
    /// <summary>
    /// Gets a transaction by ID
    /// </summary>
    /// <param name="id">The transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transaction or null if not found</returns>
    Task<WalletTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transactions for a specific wallet
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions for the wallet</returns>
    Task<IEnumerable<WalletTransaction>> GetByWalletAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions for a user across all point categories
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all user transactions</returns>
    Task<IEnumerable<WalletTransaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions within a date range
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="from">Start date (inclusive)</param>
    /// <param name="to">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions in the date range</returns>
    Task<IEnumerable<WalletTransaction>> GetByDateRangeAsync(string userId, string pointCategoryId, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by type
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="type">The transaction type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions of the specified type</returns>
    Task<IEnumerable<WalletTransaction>> GetByTypeAsync(string userId, string pointCategoryId, WalletTransactionType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new transaction
    /// </summary>
    /// <param name="transaction">The transaction to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task AddAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing transaction
    /// </summary>
    /// <param name="transaction">The transaction to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task UpdateAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by reference ID
    /// </summary>
    /// <param name="referenceId">The reference ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions with the reference ID</returns>
    Task<IEnumerable<WalletTransaction>> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default);
}
