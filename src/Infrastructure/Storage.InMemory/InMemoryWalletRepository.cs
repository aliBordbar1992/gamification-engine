using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Wallet;
using WalletEntity = GamificationEngine.Domain.Wallet.Wallet;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of wallet repository for testing
/// </summary>
public class InMemoryWalletRepository : IWalletRepository
{
    private readonly List<WalletEntity> _wallets = new();
    private readonly object _lock = new();

    public Task<WalletEntity?> GetByUserAndCategoryAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var wallet = _wallets.FirstOrDefault(w => w.UserId == userId && w.PointCategoryId == pointCategoryId);
            return Task.FromResult(wallet);
        }
    }

    public Task<IEnumerable<WalletEntity>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var wallets = _wallets.Where(w => w.UserId == userId).ToList();
            return Task.FromResult<IEnumerable<WalletEntity>>(wallets);
        }
    }

    public Task<IEnumerable<WalletEntity>> GetByPointCategoryAsync(string pointCategoryId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var wallets = _wallets.Where(w => w.PointCategoryId == pointCategoryId).ToList();
            return Task.FromResult<IEnumerable<WalletEntity>>(wallets);
        }
    }

    public Task AddAsync(WalletEntity wallet, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _wallets.Add(wallet);
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(WalletEntity wallet, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var index = _wallets.FindIndex(w => w.UserId == wallet.UserId && w.PointCategoryId == wallet.PointCategoryId);
            if (index >= 0)
            {
                _wallets[index] = wallet;
            }
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var exists = _wallets.Any(w => w.UserId == userId && w.PointCategoryId == pointCategoryId);
            return Task.FromResult(exists);
        }
    }

    public Task<IEnumerable<WalletEntity>> GetWithBalanceAboveAsync(string pointCategoryId, long minBalance, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var wallets = _wallets.Where(w => w.PointCategoryId == pointCategoryId && w.Balance >= minBalance).ToList();
            return Task.FromResult<IEnumerable<WalletEntity>>(wallets);
        }
    }
}
