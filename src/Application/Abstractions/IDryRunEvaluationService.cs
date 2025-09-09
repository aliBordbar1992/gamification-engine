using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for performing dry-run evaluations of rules without executing rewards
/// </summary>
public interface IDryRunEvaluationService
{
    /// <summary>
    /// Performs a dry-run evaluation of rules for the given event without executing rewards
    /// </summary>
    /// <param name="triggerEvent">The event to evaluate rules against</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing detailed evaluation trace</returns>
    Task<Result<DryRunResponseDto, DomainError>> DryRunRulesAsync(
        Event triggerEvent,
        CancellationToken cancellationToken = default);
}
