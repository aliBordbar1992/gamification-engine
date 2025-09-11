using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Wallet;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of wallet transaction repository for testing
/// </summary>
public class InMemoryWalletTransactionRepository : IWalletTransactionRepository
{
    private readonly List<WalletTransaction> _transactions = new();
    private readonly object _lock = new();

    public Task<WalletTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(transaction);
        }
    }

    public Task<IEnumerable<WalletTransaction>> GetByWalletAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transactions = _transactions
                .Where(t => t.UserId == userId && t.PointCategoryId == pointCategoryId)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransaction>>(transactions);
        }
    }

    public Task<IEnumerable<WalletTransaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transactions = _transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransaction>>(transactions);
        }
    }

    public Task<IEnumerable<WalletTransaction>> GetByDateRangeAsync(string userId, string pointCategoryId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transactions = _transactions
                .Where(t => t.UserId == userId &&
                           t.PointCategoryId == pointCategoryId &&
                           t.Timestamp >= from &&
                           t.Timestamp <= to)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransaction>>(transactions);
        }
    }

    public Task<IEnumerable<WalletTransaction>> GetByTypeAsync(string userId, string pointCategoryId, WalletTransactionType type, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transactions = _transactions
                .Where(t => t.UserId == userId &&
                           t.PointCategoryId == pointCategoryId &&
                           t.Type == type)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransaction>>(transactions);
        }
    }

    public Task AddAsync(WalletTransaction transaction, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _transactions.Add(transaction);
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(WalletTransaction transaction, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var index = _transactions.FindIndex(t => t.Id == transaction.Id);
            if (index >= 0)
            {
                _transactions[index] = transaction;
            }
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<WalletTransaction>> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transactions = _transactions
                .Where(t => t.ReferenceId == referenceId)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransaction>>(transactions);
        }
    }
}
