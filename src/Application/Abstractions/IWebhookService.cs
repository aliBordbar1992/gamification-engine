using GamificationEngine.Application.DTOs;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for webhook notifications
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Registers a webhook endpoint for reward notifications
    /// </summary>
    /// <param name="webhookDto">The webhook registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The registered webhook</returns>
    Task<Result<WebhookDto, string>> RegisterWebhookAsync(RegisterWebhookDto webhookDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all registered webhooks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of registered webhooks</returns>
    Task<Result<IEnumerable<WebhookDto>, string>> GetAllWebhooksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets webhooks for a specific event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of webhooks for the event type</returns>
    Task<Result<IEnumerable<WebhookDto>, string>> GetWebhooksByEventTypeAsync(string eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a webhook by ID
    /// </summary>
    /// <param name="webhookId">The webhook ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The webhook if found</returns>
    Task<Result<WebhookDto, string>> GetWebhookByIdAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a webhook
    /// </summary>
    /// <param name="webhookId">The webhook ID</param>
    /// <param name="webhookDto">The webhook update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated webhook</returns>
    Task<Result<WebhookDto, string>> UpdateWebhookAsync(string webhookId, UpdateWebhookDto webhookDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook
    /// </summary>
    /// <param name="webhookId">The webhook ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<Result<bool, string>> DeleteWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends webhook notifications for a reward event
    /// </summary>
    /// <param name="rewardEvent">The reward event data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the notification process</returns>
    Task<Result<bool, string>> SendRewardNotificationsAsync(RewardEventDto rewardEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests a webhook endpoint
    /// </summary>
    /// <param name="webhookId">The webhook ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test result</returns>
    Task<Result<WebhookTestResultDto, string>> TestWebhookAsync(string webhookId, CancellationToken cancellationToken = default);
}
