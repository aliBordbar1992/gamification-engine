using WalletEntity = GamificationEngine.Domain.Wallet.Wallet;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository interface for managing user wallets
/// </summary>
public interface IWalletRepository
{
    /// <summary>
    /// Gets a wallet for a specific user and point category
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The wallet or null if not found</returns>
    Task<WalletEntity?> GetByUserAndCategoryAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all wallets for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of user wallets</returns>
    Task<IEnumerable<WalletEntity>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all wallets for a specific point category
    /// </summary>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of wallets for the point category</returns>
    Task<IEnumerable<WalletEntity>> GetByPointCategoryAsync(string pointCategoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new wallet
    /// </summary>
    /// <param name="wallet">The wallet to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task AddAsync(WalletEntity wallet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing wallet
    /// </summary>
    /// <param name="wallet">The wallet to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task UpdateAsync(WalletEntity wallet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a wallet exists for a user and point category
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the wallet exists</returns>
    Task<bool> ExistsAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets wallets with balance above a threshold
    /// </summary>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="minBalance">Minimum balance threshold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of wallets meeting the criteria</returns>
    Task<IEnumerable<WalletEntity>> GetWithBalanceAboveAsync(string pointCategoryId, long minBalance, CancellationToken cancellationToken = default);
}
