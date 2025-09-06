using GamificationEngine.Application.DTOs;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for managing rules
/// </summary>
public interface IRuleManagementService
{
    /// <summary>
    /// Gets all rules
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all rules</returns>
    Task<Result<IEnumerable<RuleDto>, string>> GetAllRulesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active rules
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active rules</returns>
    Task<Result<IEnumerable<RuleDto>, string>> GetActiveRulesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets rules that can be triggered by a specific event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of rules for the event type</returns>
    Task<Result<IEnumerable<RuleDto>, string>> GetRulesByTriggerAsync(string eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a rule by ID
    /// </summary>
    /// <param name="ruleId">The rule ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The rule if found</returns>
    Task<Result<RuleDto, string>> GetRuleByIdAsync(string ruleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new rule
    /// </summary>
    /// <param name="dto">The rule creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created rule</returns>
    Task<Result<RuleDto, string>> CreateRuleAsync(CreateRuleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing rule
    /// </summary>
    /// <param name="ruleId">The rule ID</param>
    /// <param name="dto">The rule update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated rule</returns>
    Task<Result<RuleDto, string>> UpdateRuleAsync(string ruleId, UpdateRuleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a rule
    /// </summary>
    /// <param name="ruleId">The rule ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<Result<bool, string>> DeleteRuleAsync(string ruleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a rule
    /// </summary>
    /// <param name="ruleId">The rule ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if activated successfully</returns>
    Task<Result<bool, string>> ActivateRuleAsync(string ruleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a rule
    /// </summary>
    /// <param name="ruleId">The rule ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deactivated successfully</returns>
    Task<Result<bool, string>> DeactivateRuleAsync(string ruleId, CancellationToken cancellationToken = default);
}
