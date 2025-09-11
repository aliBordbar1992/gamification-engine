using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Wallet;
using GamificationEngine.Infrastructure.Storage.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GamificationEngine.Infrastructure.Storage.EntityFramework.Repositories;

/// <summary>
/// Entity Framework implementation of wallet transfer repository
/// </summary>
public class WalletTransferRepository : IWalletTransferRepository
{
    private readonly GamificationEngineDbContext _context;

    public WalletTransferRepository(GamificationEngineDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<WalletTransfer?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransfers
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WalletTransfer>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransfers
            .Where(t => t.FromUserId == userId || t.ToUserId == userId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransfer>> GetSentByUserAsync(string fromUserId, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransfers
            .Where(t => t.FromUserId == fromUserId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransfer>> GetReceivedByUserAsync(string toUserId, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransfers
            .Where(t => t.ToUserId == toUserId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransfer>> GetByStatusAsync(WalletTransferStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransfers
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransfer>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransfers
            .Where(t => t.Timestamp >= from && t.Timestamp <= to)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransfer>> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransfers
            .Where(t => t.ReferenceId == referenceId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WalletTransfer transfer, CancellationToken cancellationToken = default)
    {
        _context.WalletTransfers.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WalletTransfer transfer, CancellationToken cancellationToken = default)
    {
        _context.WalletTransfers.Update(transfer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransfer>> GetPendingTransfersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransfers
            .Where(t => t.Status == WalletTransferStatus.Pending)
            .OrderBy(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
