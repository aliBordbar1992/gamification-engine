using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for executing rule spendings
/// </summary>
public interface ISpendingExecutionService
{
    /// <summary>
    /// Executes a spending for a user
    /// </summary>
    /// <param name="spending">The spending to execute</param>
    /// <param name="userId">The user ID to execute the spending for</param>
    /// <param name="triggerEvent">The event that triggered the spending</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the execution outcome</returns>
    Task<Result<SpendingExecutionResult, DomainError>> ExecuteSpendingAsync(
        RuleSpending spending,
        string userId,
        Event triggerEvent,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a spending execution
/// </summary>
public sealed record SpendingExecutionResult(
    string SpendingId,
    string Category,
    string Type,
    string UserId,
    string EventId,
    DateTimeOffset ExecutedAt,
    bool Success,
    string? ErrorMessage = null,
    long? Amount = null,
    string? DestinationUserId = null);
