using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Wallet;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of wallet transfer repository for testing
/// </summary>
public class InMemoryWalletTransferRepository : IWalletTransferRepository
{
    private readonly List<WalletTransfer> _transfers = new();
    private readonly object _lock = new();

    public Task<WalletTransfer?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transfer = _transfers.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(transfer);
        }
    }

    public Task<IEnumerable<WalletTransfer>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transfers = _transfers
                .Where(t => t.FromUserId == userId || t.ToUserId == userId)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransfer>>(transfers);
        }
    }

    public Task<IEnumerable<WalletTransfer>> GetSentByUserAsync(string fromUserId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transfers = _transfers
                .Where(t => t.FromUserId == fromUserId)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransfer>>(transfers);
        }
    }

    public Task<IEnumerable<WalletTransfer>> GetReceivedByUserAsync(string toUserId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transfers = _transfers
                .Where(t => t.ToUserId == toUserId)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransfer>>(transfers);
        }
    }

    public Task<IEnumerable<WalletTransfer>> GetByStatusAsync(WalletTransferStatus status, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transfers = _transfers
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransfer>>(transfers);
        }
    }

    public Task<IEnumerable<WalletTransfer>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transfers = _transfers
                .Where(t => t.Timestamp >= from && t.Timestamp <= to)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransfer>>(transfers);
        }
    }

    public Task<IEnumerable<WalletTransfer>> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transfers = _transfers
                .Where(t => t.ReferenceId == referenceId)
                .OrderByDescending(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransfer>>(transfers);
        }
    }

    public Task AddAsync(WalletTransfer transfer, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _transfers.Add(transfer);
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(WalletTransfer transfer, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var index = _transfers.FindIndex(t => t.Id == transfer.Id);
            if (index >= 0)
            {
                _transfers[index] = transfer;
            }
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<WalletTransfer>> GetPendingTransfersAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var transfers = _transfers
                .Where(t => t.Status == WalletTransferStatus.Pending)
                .OrderBy(t => t.Timestamp)
                .ToList();
            return Task.FromResult<IEnumerable<WalletTransfer>>(transfers);
        }
    }
}
