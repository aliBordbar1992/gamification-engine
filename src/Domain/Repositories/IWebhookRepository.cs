using GamificationEngine.Domain.Webhooks;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository interface for webhook data access
/// </summary>
public interface IWebhookRepository
{
    /// <summary>
    /// Gets all webhooks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all webhooks</returns>
    Task<IEnumerable<Webhook>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active webhooks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active webhooks</returns>
    Task<IEnumerable<Webhook>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets webhooks for a specific event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of webhooks for the event type</returns>
    Task<IEnumerable<Webhook>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a webhook by ID
    /// </summary>
    /// <param name="webhookId">The webhook ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The webhook if found</returns>
    Task<Webhook?> GetByIdAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new webhook
    /// </summary>
    /// <param name="webhook">The webhook to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task AddAsync(Webhook webhook, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing webhook
    /// </summary>
    /// <param name="webhook">The webhook to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task UpdateAsync(Webhook webhook, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook by ID
    /// </summary>
    /// <param name="webhookId">The webhook ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task DeleteAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a webhook exists by ID
    /// </summary>
    /// <param name="webhookId">The webhook ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the webhook exists, false otherwise</returns>
    Task<bool> ExistsAsync(string webhookId, CancellationToken cancellationToken = default);
}
