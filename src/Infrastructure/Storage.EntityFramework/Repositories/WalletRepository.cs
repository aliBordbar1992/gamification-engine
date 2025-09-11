
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Wallet;
using GamificationEngine.Infrastructure.Storage.EntityFramework;
using Microsoft.EntityFrameworkCore;
using WalletEntity = GamificationEngine.Domain.Wallet.Wallet;

namespace GamificationEngine.Infrastructure.Storage.EntityFramework.Repositories;

/// <summary>
/// Entity Framework implementation of wallet repository
/// </summary>
public class WalletRepository : IWalletRepository
{
    private readonly GamificationEngineDbContext _context;

    public WalletRepository(GamificationEngineDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<WalletEntity?> GetByUserAndCategoryAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.PointCategoryId == pointCategoryId, cancellationToken);
    }

    public async Task<IEnumerable<WalletEntity>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .Include(w => w.Transactions)
            .Where(w => w.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletEntity>> GetByPointCategoryAsync(string pointCategoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .Include(w => w.Transactions)
            .Where(w => w.PointCategoryId == pointCategoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WalletEntity wallet, CancellationToken cancellationToken = default)
    {
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WalletEntity wallet, CancellationToken cancellationToken = default)
    {
        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .AnyAsync(w => w.UserId == userId && w.PointCategoryId == pointCategoryId, cancellationToken);
    }

    public async Task<IEnumerable<WalletEntity>> GetWithBalanceAboveAsync(string pointCategoryId, long minBalance, CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .Include(w => w.Transactions)
            .Where(w => w.PointCategoryId == pointCategoryId && w.Balance >= minBalance)
            .ToListAsync(cancellationToken);
    }
}
