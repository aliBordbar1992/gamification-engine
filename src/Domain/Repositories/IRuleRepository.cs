using GamificationEngine.Domain.Rules;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository for storing and retrieving rules
/// </summary>
public interface IRuleRepository
{
    /// <summary>
    /// Stores a new rule
    /// </summary>
    /// <param name="rule">The rule to store</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task StoreAsync(Rule rule);

    /// <summary>
    /// Retrieves a rule by its ID
    /// </summary>
    /// <param name="ruleId">The rule ID to retrieve</param>
    /// <returns>The rule if found, null otherwise</returns>
    Task<Rule?> GetByIdAsync(string ruleId);

    /// <summary>
    /// Retrieves all active rules that can be triggered by the given event type
    /// </summary>
    /// <param name="eventType">The event type to filter by</param>
    /// <returns>Collection of rules that can be triggered by the event type</returns>
    Task<IEnumerable<Rule>> GetByTriggerAsync(string eventType);

    /// <summary>
    /// Retrieves all active rules
    /// </summary>
    /// <returns>Collection of all active rules</returns>
    Task<IEnumerable<Rule>> GetAllActiveAsync();

    /// <summary>
    /// Retrieves all rules (active and inactive)
    /// </summary>
    /// <returns>Collection of all rules</returns>
    Task<IEnumerable<Rule>> GetAllAsync();

    /// <summary>
    /// Updates an existing rule
    /// </summary>
    /// <param name="rule">The rule to update</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task UpdateAsync(Rule rule);

    /// <summary>
    /// Deletes a rule by its ID
    /// </summary>
    /// <param name="ruleId">The rule ID to delete</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task DeleteAsync(string ruleId);

    /// <summary>
    /// Checks if a rule with the given ID exists
    /// </summary>
    /// <param name="ruleId">The rule ID to check</param>
    /// <returns>True if the rule exists, false otherwise</returns>
    Task<bool> ExistsAsync(string ruleId);
}
