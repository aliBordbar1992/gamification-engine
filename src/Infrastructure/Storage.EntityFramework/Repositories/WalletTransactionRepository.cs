using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Wallet;
using GamificationEngine.Infrastructure.Storage.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GamificationEngine.Infrastructure.Storage.EntityFramework.Repositories;

/// <summary>
/// Entity Framework implementation of wallet transaction repository
/// </summary>
public class WalletTransactionRepository : IWalletTransactionRepository
{
    private readonly GamificationEngineDbContext _context;

    public WalletTransactionRepository(GamificationEngineDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<WalletTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetByWalletAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.UserId == userId && t.PointCategoryId == pointCategoryId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetByDateRangeAsync(string userId, string pointCategoryId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.UserId == userId &&
                       t.PointCategoryId == pointCategoryId &&
                       t.Timestamp >= from &&
                       t.Timestamp <= to)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetByTypeAsync(string userId, string pointCategoryId, WalletTransactionType type, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.UserId == userId &&
                       t.PointCategoryId == pointCategoryId &&
                       t.Type == type)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WalletTransaction transaction, CancellationToken cancellationToken = default)
    {
        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WalletTransaction transaction, CancellationToken cancellationToken = default)
    {
        _context.WalletTransactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletTransaction>> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .Where(t => t.ReferenceId == referenceId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
