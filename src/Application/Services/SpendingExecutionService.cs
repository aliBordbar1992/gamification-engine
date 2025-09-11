using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Wallet;
using GamificationEngine.Shared;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service responsible for executing rule spendings
/// </summary>
public class SpendingExecutionService : ISpendingExecutionService
{
    private readonly IWalletService _walletService;
    private readonly IPointCategoryRepository _pointCategoryRepository;
    private readonly ILogger<SpendingExecutionService> _logger;

    public SpendingExecutionService(
        IWalletService walletService,
        IPointCategoryRepository pointCategoryRepository,
        ILogger<SpendingExecutionService> logger)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _pointCategoryRepository = pointCategoryRepository ?? throw new ArgumentNullException(nameof(pointCategoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a spending for a user
    /// </summary>
    /// <param name="spending">The spending to execute</param>
    /// <param name="userId">The user ID to execute the spending for</param>
    /// <param name="triggerEvent">The event that triggered the spending</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the execution outcome</returns>
    public async Task<Result<SpendingExecutionResult, DomainError>> ExecuteSpendingAsync(
        RuleSpending spending,
        string userId,
        Event triggerEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing spending {SpendingType} for user {UserId} in category {Category}",
                spending.Type, userId, spending.Category);

            // Validate spending configuration
            if (!spending.IsValid())
            {
                return Result<SpendingExecutionResult, DomainError>.Failure(
                    new SpendingExecutionError($"Invalid spending configuration: {spending.Category}"));
            }

            // Get point category to validate spendability
            var pointCategory = await _pointCategoryRepository.GetByIdAsync(spending.Category);
            if (pointCategory == null)
            {
                return Result<SpendingExecutionResult, DomainError>.Failure(
                    new SpendingExecutionError($"Point category not found: {spending.Category}"));
            }

            if (!pointCategory.IsSpendable)
            {
                return Result<SpendingExecutionResult, DomainError>.Failure(
                    new SpendingExecutionError($"Point category {spending.Category} is not spendable"));
            }

            // Execute based on spending type
            return spending.Type switch
            {
                RuleSpendingType.Transaction => await ExecuteTransactionAsync(spending, userId, triggerEvent, pointCategory, cancellationToken),
                RuleSpendingType.Transfer => await ExecuteTransferAsync(spending, userId, triggerEvent, pointCategory, cancellationToken),
                _ => Result<SpendingExecutionResult, DomainError>.Failure(
                    new SpendingExecutionError($"Unsupported spending type: {spending.Type}"))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing spending {SpendingType} for user {UserId}", spending.Type, userId);
            return Result<SpendingExecutionResult, DomainError>.Failure(
                new SpendingExecutionError($"Failed to execute spending: {ex.Message}"));
        }
    }

    /// <summary>
    /// Executes a transaction spending
    /// </summary>
    private async Task<Result<SpendingExecutionResult, DomainError>> ExecuteTransactionAsync(
        RuleSpending spending,
        string userId,
        Event triggerEvent,
        PointCategory pointCategory,
        CancellationToken cancellationToken)
    {
        try
        {
            // Extract amount from event payload
            var amountStr = spending.GetAmountAttribute();
            if (!long.TryParse(amountStr, out var amount))
            {
                // Try to get amount from event payload
                if (triggerEvent.Attributes?.TryGetValue(amountStr, out var amountValue) == true)
                {
                    if (!long.TryParse(amountValue?.ToString(), out amount))
                    {
                        return Result<SpendingExecutionResult, DomainError>.Failure(
                            new SpendingExecutionError($"Invalid amount value: {amountValue}"));
                    }
                }
                else
                {
                    return Result<SpendingExecutionResult, DomainError>.Failure(
                        new SpendingExecutionError($"Amount attribute not found in event payload: {amountStr}"));
                }
            }

            // Create transaction
            var transactionId = Guid.NewGuid().ToString();
            var transaction = new WalletTransaction(
                transactionId,
                userId,
                spending.Category,
                -amount, // Negative amount for spending
                WalletTransactionType.Spent,
                $"Spending from rule execution - {triggerEvent.EventType}",
                triggerEvent.EventId,
                System.Text.Json.JsonSerializer.Serialize(triggerEvent.Attributes));

            // Execute the transaction
            var result = await _walletService.AddTransactionAsync(transaction, cancellationToken);
            if (!result.IsSuccess)
            {
                return Result<SpendingExecutionResult, DomainError>.Failure(
                    new SpendingExecutionError($"Failed to add transaction: {result.Error}"));
            }

            var executionResult = new SpendingExecutionResult(
                transactionId,
                spending.Category,
                spending.Type.ToString(),
                userId,
                triggerEvent.EventId,
                DateTimeOffset.UtcNow,
                true,
                null,
                amount);

            _logger.LogInformation("Successfully executed transaction spending {TransactionId} for user {UserId}, amount: {Amount}",
                transactionId, userId, amount);

            return Result<SpendingExecutionResult, DomainError>.Success(executionResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing transaction spending for user {UserId}", userId);
            return Result<SpendingExecutionResult, DomainError>.Failure(
                new SpendingExecutionError($"Failed to execute transaction: {ex.Message}"));
        }
    }

    /// <summary>
    /// Executes a transfer spending
    /// </summary>
    private async Task<Result<SpendingExecutionResult, DomainError>> ExecuteTransferAsync(
        RuleSpending spending,
        string userId,
        Event triggerEvent,
        PointCategory pointCategory,
        CancellationToken cancellationToken)
    {
        try
        {
            // Extract transfer details from event payload
            var sourceAttr = spending.GetSourceAttribute();
            var destinationAttr = spending.GetDestinationAttribute();
            var amountStr = spending.GetAmountAttribute();

            // Get source user (should be the current user)
            var sourceUserId = userId;
            if (!string.IsNullOrEmpty(sourceAttr) && sourceAttr != "userId")
            {
                if (triggerEvent.Attributes?.TryGetValue(sourceAttr, out var sourceValue) == true)
                {
                    sourceUserId = sourceValue?.ToString() ?? userId;
                }
            }

            // Get destination user
            string destinationUserId;
            if (triggerEvent.Attributes?.TryGetValue(destinationAttr, out var destinationValue) == true)
            {
                destinationUserId = destinationValue?.ToString() ?? string.Empty;
            }
            else
            {
                return Result<SpendingExecutionResult, DomainError>.Failure(
                    new SpendingExecutionError($"Destination user not found in event payload: {destinationAttr}"));
            }

            // Get amount
            long amount;
            if (!long.TryParse(amountStr, out amount))
            {
                if (triggerEvent.Attributes?.TryGetValue(amountStr, out var amountValue) == true)
                {
                    if (!long.TryParse(amountValue?.ToString(), out amount))
                    {
                        return Result<SpendingExecutionResult, DomainError>.Failure(
                            new SpendingExecutionError($"Invalid amount value: {amountValue}"));
                    }
                }
                else
                {
                    return Result<SpendingExecutionResult, DomainError>.Failure(
                        new SpendingExecutionError($"Amount attribute not found in event payload: {amountStr}"));
                }
            }

            // Create transfer
            var transferId = Guid.NewGuid().ToString();
            var transfer = new WalletTransfer(
                transferId,
                sourceUserId,
                destinationUserId,
                spending.Category,
                amount,
                $"Transfer from rule execution - {triggerEvent.EventType}",
                triggerEvent.EventId,
                System.Text.Json.JsonSerializer.Serialize(triggerEvent.Attributes));

            // Execute the transfer
            var result = await _walletService.TransferAsync(transfer, cancellationToken);
            if (!result.IsSuccess)
            {
                return Result<SpendingExecutionResult, DomainError>.Failure(
                    new SpendingExecutionError($"Failed to execute transfer: {result.Error}"));
            }

            var executionResult = new SpendingExecutionResult(
                transferId,
                spending.Category,
                spending.Type.ToString(),
                userId,
                triggerEvent.EventId,
                DateTimeOffset.UtcNow,
                true,
                null,
                amount,
                destinationUserId);

            _logger.LogInformation("Successfully executed transfer spending {TransferId} from user {SourceUserId} to user {DestinationUserId}, amount: {Amount}",
                transferId, sourceUserId, destinationUserId, amount);

            return Result<SpendingExecutionResult, DomainError>.Success(executionResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing transfer spending for user {UserId}", userId);
            return Result<SpendingExecutionResult, DomainError>.Failure(
                new SpendingExecutionError($"Failed to execute transfer: {ex.Message}"));
        }
    }
}
