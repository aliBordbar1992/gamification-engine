using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Wallet;
using GamificationEngine.Shared;
using WalletEntity = GamificationEngine.Domain.Wallet.Wallet;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for managing user wallets and transactions
/// </summary>
public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IWalletTransferRepository _transferRepository;
    private readonly IPointCategoryRepository _pointCategoryRepository;
    private readonly IUserStateRepository _userStateRepository;

    public WalletService(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IWalletTransferRepository transferRepository,
        IPointCategoryRepository pointCategoryRepository,
        IUserStateRepository userStateRepository)
    {
        _walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
        _pointCategoryRepository = pointCategoryRepository ?? throw new ArgumentNullException(nameof(pointCategoryRepository));
        _userStateRepository = userStateRepository ?? throw new ArgumentNullException(nameof(userStateRepository));
    }

    public async Task<Result<WalletDto, string>> GetWalletAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<WalletDto, string>.Failure("User ID cannot be empty");

            if (string.IsNullOrWhiteSpace(pointCategoryId))
                return Result<WalletDto, string>.Failure("Point category ID cannot be empty");

            var wallet = await _walletRepository.GetByUserAndCategoryAsync(userId, pointCategoryId, cancellationToken);
            if (wallet == null)
                return Result<WalletDto, string>.Failure("Wallet not found");

            var dto = new WalletDto
            {
                UserId = wallet.UserId,
                PointCategoryId = wallet.PointCategoryId,
                Balance = wallet.Balance,
                Transactions = wallet.Transactions.Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    PointCategoryId = t.PointCategoryId,
                    Amount = t.Amount,
                    Type = t.Type.ToString(),
                    Description = t.Description,
                    ReferenceId = t.ReferenceId,
                    Metadata = t.Metadata,
                    Timestamp = t.Timestamp
                }).ToList()
            };

            return Result<WalletDto, string>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<WalletDto, string>.Failure($"Failed to get wallet: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<WalletDto>, string>> GetUserWalletsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<IEnumerable<WalletDto>, string>.Failure("User ID cannot be empty");

            var wallets = await _walletRepository.GetByUserAsync(userId, cancellationToken);
            var dtos = wallets.Select(w => new WalletDto
            {
                UserId = w.UserId,
                PointCategoryId = w.PointCategoryId,
                Balance = w.Balance,
                Transactions = w.Transactions.Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    PointCategoryId = t.PointCategoryId,
                    Amount = t.Amount,
                    Type = t.Type.ToString(),
                    Description = t.Description,
                    ReferenceId = t.ReferenceId,
                    Metadata = t.Metadata,
                    Timestamp = t.Timestamp
                }).ToList()
            });

            return Result<IEnumerable<WalletDto>, string>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<WalletDto>, string>.Failure($"Failed to get user wallets: {ex.Message}");
        }
    }

    public async Task<Result<long, string>> GetBalanceAsync(string userId, string pointCategoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<long, string>.Failure("User ID cannot be empty");

            if (string.IsNullOrWhiteSpace(pointCategoryId))
                return Result<long, string>.Failure("Point category ID cannot be empty");

            var wallet = await _walletRepository.GetByUserAndCategoryAsync(userId, pointCategoryId, cancellationToken);
            if (wallet == null)
                return Result<long, string>.Success(0); // Return 0 balance if wallet doesn't exist

            return Result<long, string>.Success(wallet.Balance);
        }
        catch (Exception ex)
        {
            return Result<long, string>.Failure($"Failed to get balance: {ex.Message}");
        }
    }

    public async Task<Result<WalletTransactionDto, string>> SpendPointsAsync(string userId, string pointCategoryId, long amount, string description, string? referenceId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<WalletTransactionDto, string>.Failure("User ID cannot be empty");

            if (string.IsNullOrWhiteSpace(pointCategoryId))
                return Result<WalletTransactionDto, string>.Failure("Point category ID cannot be empty");

            if (amount <= 0)
                return Result<WalletTransactionDto, string>.Failure("Amount must be positive");

            if (string.IsNullOrWhiteSpace(description))
                return Result<WalletTransactionDto, string>.Failure("Description cannot be empty");

            // Get point category to validate spending rules
            var pointCategory = await _pointCategoryRepository.GetByIdAsync(pointCategoryId, cancellationToken);
            if (pointCategory == null)
                return Result<WalletTransactionDto, string>.Failure("Point category not found");

            if (!pointCategory.IsSpendable)
                return Result<WalletTransactionDto, string>.Failure("Point category is not spendable");

            // Get or create wallet
            var wallet = await _walletRepository.GetByUserAndCategoryAsync(userId, pointCategoryId, cancellationToken);
            if (wallet == null)
            {
                wallet = new Wallet(userId, pointCategoryId);
                await _walletRepository.AddAsync(wallet, cancellationToken);
            }

            // Check if user can afford the spending
            if (!wallet.CanAfford(-amount, pointCategory))
            {
                return Result<WalletTransactionDto, string>.Failure($"Insufficient balance. Current: {wallet.Balance}, Required: {amount}");
            }

            // Create transaction
            var transactionId = Guid.NewGuid().ToString();
            var transaction = new WalletTransaction(
                transactionId,
                userId,
                pointCategoryId,
                -amount, // Negative for spending
                WalletTransactionType.Spent,
                description,
                referenceId,
                metadata);

            // Add transaction to wallet
            wallet.AddTransaction(transaction, pointCategory);

            // Update wallet in repository
            await _walletRepository.UpdateAsync(wallet, cancellationToken);
            await _transactionRepository.AddAsync(transaction, cancellationToken);

            // Update user state
            var userState = await _userStateRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userState != null)
            {
                userState.AddPoints(pointCategoryId, -amount);
                await _userStateRepository.SaveAsync(userState, cancellationToken);
            }

            var dto = new WalletTransactionDto
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                PointCategoryId = transaction.PointCategoryId,
                Amount = transaction.Amount,
                Type = transaction.Type.ToString(),
                Description = transaction.Description,
                ReferenceId = transaction.ReferenceId,
                Metadata = transaction.Metadata,
                Timestamp = transaction.Timestamp
            };

            return Result<WalletTransactionDto, string>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<WalletTransactionDto, string>.Failure($"Failed to spend points: {ex.Message}");
        }
    }

    public async Task<Result<WalletTransferDto, string>> TransferPointsAsync(string fromUserId, string toUserId, string pointCategoryId, long amount, string description, string? referenceId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fromUserId))
                return Result<WalletTransferDto, string>.Failure("From user ID cannot be empty");

            if (string.IsNullOrWhiteSpace(toUserId))
                return Result<WalletTransferDto, string>.Failure("To user ID cannot be empty");

            if (string.IsNullOrWhiteSpace(pointCategoryId))
                return Result<WalletTransferDto, string>.Failure("Point category ID cannot be empty");

            if (amount <= 0)
                return Result<WalletTransferDto, string>.Failure("Amount must be positive");

            if (string.IsNullOrWhiteSpace(description))
                return Result<WalletTransferDto, string>.Failure("Description cannot be empty");

            if (fromUserId == toUserId)
                return Result<WalletTransferDto, string>.Failure("Cannot transfer to the same user");

            // Get point category to validate transfer rules
            var pointCategory = await _pointCategoryRepository.GetByIdAsync(pointCategoryId, cancellationToken);
            if (pointCategory == null)
                return Result<WalletTransferDto, string>.Failure("Point category not found");

            if (!pointCategory.IsSpendable)
                return Result<WalletTransferDto, string>.Failure("Point category is not transferable");

            // Get or create sender wallet
            var fromWallet = await _walletRepository.GetByUserAndCategoryAsync(fromUserId, pointCategoryId, cancellationToken);
            if (fromWallet == null)
            {
                fromWallet = new Wallet(fromUserId, pointCategoryId);
                await _walletRepository.AddAsync(fromWallet, cancellationToken);
            }

            // Check if sender can afford the transfer
            if (!fromWallet.CanAfford(-amount, pointCategory))
            {
                return Result<WalletTransferDto, string>.Failure($"Insufficient balance. Current: {fromWallet.Balance}, Required: {amount}");
            }

            // Get or create receiver wallet
            var toWallet = await _walletRepository.GetByUserAndCategoryAsync(toUserId, pointCategoryId, cancellationToken);
            if (toWallet == null)
            {
                toWallet = new Wallet(toUserId, pointCategoryId);
                await _walletRepository.AddAsync(toWallet, cancellationToken);
            }

            // Create transfer
            var transferId = Guid.NewGuid().ToString();
            var transfer = new WalletTransfer(
                transferId,
                fromUserId,
                toUserId,
                pointCategoryId,
                amount,
                description,
                referenceId,
                metadata);

            // Create transactions for both wallets
            var fromTransaction = new WalletTransaction(
                Guid.NewGuid().ToString(),
                fromUserId,
                pointCategoryId,
                -amount,
                WalletTransactionType.TransferOut,
                $"Transfer to {toUserId}: {description}",
                transferId,
                metadata);

            var toTransaction = new WalletTransaction(
                Guid.NewGuid().ToString(),
                toUserId,
                pointCategoryId,
                amount,
                WalletTransactionType.TransferIn,
                $"Transfer from {fromUserId}: {description}",
                transferId,
                metadata);

            // Add transactions to wallets
            fromWallet.AddTransaction(fromTransaction, pointCategory);
            toWallet.AddTransaction(toTransaction, pointCategory);

            // Mark transfer as completed
            transfer.MarkCompleted();

            // Update repositories
            await _walletRepository.UpdateAsync(fromWallet, cancellationToken);
            await _walletRepository.UpdateAsync(toWallet, cancellationToken);
            await _transferRepository.AddAsync(transfer, cancellationToken);
            await _transactionRepository.AddAsync(fromTransaction, cancellationToken);
            await _transactionRepository.AddAsync(toTransaction, cancellationToken);

            // Update user states
            var fromUserState = await _userStateRepository.GetByUserIdAsync(fromUserId, cancellationToken);
            if (fromUserState != null)
            {
                fromUserState.AddPoints(pointCategoryId, -amount);
                await _userStateRepository.SaveAsync(fromUserState, cancellationToken);
            }

            var toUserState = await _userStateRepository.GetByUserIdAsync(toUserId, cancellationToken);
            if (toUserState != null)
            {
                toUserState.AddPoints(pointCategoryId, amount);
                await _userStateRepository.SaveAsync(toUserState, cancellationToken);
            }

            var dto = new WalletTransferDto
            {
                Id = transfer.Id,
                FromUserId = transfer.FromUserId,
                ToUserId = transfer.ToUserId,
                PointCategoryId = transfer.PointCategoryId,
                Amount = transfer.Amount,
                Description = transfer.Description,
                ReferenceId = transfer.ReferenceId,
                Metadata = transfer.Metadata,
                Status = transfer.Status.ToString(),
                Timestamp = transfer.Timestamp,
                CompletedAt = transfer.CompletedAt,
                FailureReason = transfer.FailureReason
            };

            return Result<WalletTransferDto, string>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<WalletTransferDto, string>.Failure($"Failed to transfer points: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<WalletTransactionDto>, string>> GetTransactionHistoryAsync(string userId, string pointCategoryId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<IEnumerable<WalletTransactionDto>, string>.Failure("User ID cannot be empty");

            if (string.IsNullOrWhiteSpace(pointCategoryId))
                return Result<IEnumerable<WalletTransactionDto>, string>.Failure("Point category ID cannot be empty");

            IEnumerable<WalletTransaction> transactions;

            if (from.HasValue && to.HasValue)
            {
                transactions = await _transactionRepository.GetByDateRangeAsync(userId, pointCategoryId, from.Value, to.Value, cancellationToken);
            }
            else
            {
                transactions = await _transactionRepository.GetByWalletAsync(userId, pointCategoryId, cancellationToken);
            }

            var dtos = transactions.Select(t => new WalletTransactionDto
            {
                Id = t.Id,
                UserId = t.UserId,
                PointCategoryId = t.PointCategoryId,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Description = t.Description,
                ReferenceId = t.ReferenceId,
                Metadata = t.Metadata,
                Timestamp = t.Timestamp
            });

            return Result<IEnumerable<WalletTransactionDto>, string>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<WalletTransactionDto>, string>.Failure($"Failed to get transaction history: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> AddTransactionAsync(WalletTransaction transaction, CancellationToken cancellationToken = default)
    {
        try
        {
            if (transaction == null)
                return Result<bool, string>.Failure("Transaction cannot be null");

            // Get or create wallet
            var wallet = await _walletRepository.GetByUserAndCategoryAsync(transaction.UserId, transaction.PointCategoryId, cancellationToken);
            if (wallet == null)
            {
                wallet = new WalletEntity(transaction.UserId, transaction.PointCategoryId);
                await _walletRepository.AddAsync(wallet, cancellationToken);
            }

            // Get point category for validation
            var pointCategory = await _pointCategoryRepository.GetByIdAsync(transaction.PointCategoryId);
            if (pointCategory == null)
                return Result<bool, string>.Failure($"Point category not found: {transaction.PointCategoryId}");

            // Add transaction to wallet
            wallet.AddTransaction(transaction, pointCategory);

            // Update wallet in repository
            await _walletRepository.UpdateAsync(wallet, cancellationToken);

            // Store transaction separately for history
            await _transactionRepository.AddAsync(transaction, cancellationToken);

            return Result<bool, string>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool, string>.Failure($"Error adding transaction: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> TransferAsync(WalletTransfer transfer, CancellationToken cancellationToken = default)
    {
        try
        {
            if (transfer == null)
                return Result<bool, string>.Failure("Transfer cannot be null");

            // Get or create wallets for both users
            var fromWallet = await _walletRepository.GetByUserAndCategoryAsync(transfer.FromUserId, transfer.PointCategoryId, cancellationToken);
            if (fromWallet == null)
            {
                fromWallet = new WalletEntity(transfer.FromUserId, transfer.PointCategoryId);
                await _walletRepository.AddAsync(fromWallet, cancellationToken);
            }

            var toWallet = await _walletRepository.GetByUserAndCategoryAsync(transfer.ToUserId, transfer.PointCategoryId, cancellationToken);
            if (toWallet == null)
            {
                toWallet = new WalletEntity(transfer.ToUserId, transfer.PointCategoryId);
                await _walletRepository.AddAsync(toWallet, cancellationToken);
            }

            // Get point category for validation
            var pointCategory = await _pointCategoryRepository.GetByIdAsync(transfer.PointCategoryId);
            if (pointCategory == null)
                return Result<bool, string>.Failure($"Point category not found: {transfer.PointCategoryId}");

            // Check if source wallet has sufficient balance
            if (!fromWallet.CanAfford(-transfer.Amount, pointCategory))
            {
                transfer.MarkFailed("Insufficient balance");
                await _transferRepository.AddAsync(transfer, cancellationToken);
                return Result<bool, string>.Failure("Insufficient balance for transfer");
            }

            // Create outgoing transaction for source wallet
            var outgoingTransaction = new WalletTransaction(
                Guid.NewGuid().ToString(),
                transfer.FromUserId,
                transfer.PointCategoryId,
                -transfer.Amount,
                WalletTransactionType.TransferOut,
                transfer.Description,
                transfer.Id,
                transfer.Metadata);

            // Create incoming transaction for destination wallet
            var incomingTransaction = new WalletTransaction(
                Guid.NewGuid().ToString(),
                transfer.ToUserId,
                transfer.PointCategoryId,
                transfer.Amount,
                WalletTransactionType.TransferIn,
                transfer.Description,
                transfer.Id,
                transfer.Metadata);

            // Add transactions to wallets
            fromWallet.AddTransaction(outgoingTransaction, pointCategory);
            toWallet.AddTransaction(incomingTransaction, pointCategory);

            // Mark transfer as completed
            transfer.MarkCompleted();

            // Update wallets and store transfer
            await _walletRepository.UpdateAsync(fromWallet, cancellationToken);
            await _walletRepository.UpdateAsync(toWallet, cancellationToken);
            await _transferRepository.AddAsync(transfer, cancellationToken);
            await _transactionRepository.AddAsync(outgoingTransaction, cancellationToken);
            await _transactionRepository.AddAsync(incomingTransaction, cancellationToken);

            return Result<bool, string>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool, string>.Failure($"Error executing transfer: {ex.Message}");
        }
    }
}
