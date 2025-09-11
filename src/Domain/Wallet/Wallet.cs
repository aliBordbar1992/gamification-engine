using GamificationEngine.Domain.Entities;

namespace GamificationEngine.Domain.Wallet;

/// <summary>
/// Represents a user's wallet for spendable point categories
/// </summary>
public sealed class Wallet
{
    // EF Core requires a parameterless constructor
    private Wallet() { }

    public Wallet(string userId, string pointCategoryId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(pointCategoryId)) throw new ArgumentException("Point category ID cannot be empty", nameof(pointCategoryId));

        UserId = userId;
        PointCategoryId = pointCategoryId;
        Balance = 0;
        _transactions = new List<WalletTransaction>();
    }

    private readonly List<WalletTransaction> _transactions = new();

    public string UserId { get; private set; } = string.Empty;
    public string PointCategoryId { get; private set; } = string.Empty;
    public long Balance { get; private set; } = 0;
    public IReadOnlyList<WalletTransaction> Transactions => _transactions.AsReadOnly();

    /// <summary>
    /// Adds a transaction to the wallet and updates the balance
    /// </summary>
    /// <param name="transaction">The transaction to add</param>
    /// <param name="pointCategory">The point category to validate against</param>
    public void AddTransaction(WalletTransaction transaction, PointCategory pointCategory)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        if (pointCategory == null) throw new ArgumentNullException(nameof(pointCategory));
        if (transaction.PointCategoryId != PointCategoryId) throw new ArgumentException("Transaction point category must match wallet point category", nameof(transaction));

        // Validate transaction against point category rules
        if (transaction.Amount < 0 && !pointCategory.NegativeBalanceAllowed)
        {
            var newBalance = Balance + transaction.Amount;
            if (newBalance < 0)
            {
                throw new InvalidOperationException($"Insufficient balance. Current: {Balance}, Required: {Math.Abs(transaction.Amount)}");
            }
        }

        _transactions.Add(transaction);
        Balance += transaction.Amount;
    }

    /// <summary>
    /// Checks if the user has sufficient balance for a transaction
    /// </summary>
    /// <param name="amount">The amount to check (negative for spending)</param>
    /// <param name="pointCategory">The point category to validate against</param>
    /// <returns>True if the transaction would be valid</returns>
    public bool CanAfford(long amount, PointCategory pointCategory)
    {
        if (pointCategory == null) throw new ArgumentNullException(nameof(pointCategory));
        if (amount >= 0) return true; // Earning is always allowed

        var newBalance = Balance + amount;
        return pointCategory.NegativeBalanceAllowed || newBalance >= 0;
    }

    /// <summary>
    /// Gets the transaction history for a specific time range
    /// </summary>
    /// <param name="from">Start date (inclusive)</param>
    /// <param name="to">End date (inclusive)</param>
    /// <returns>Filtered transactions</returns>
    public IEnumerable<WalletTransaction> GetTransactions(DateTime from, DateTime to)
    {
        return _transactions.Where(t => t.Timestamp >= from && t.Timestamp <= to);
    }

    /// <summary>
    /// Gets the transaction history for a specific transaction type
    /// </summary>
    /// <param name="type">The transaction type to filter by</param>
    /// <returns>Filtered transactions</returns>
    public IEnumerable<WalletTransaction> GetTransactions(WalletTransactionType type)
    {
        return _transactions.Where(t => t.Type == type);
    }
}
