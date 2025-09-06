using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Webhooks;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of IWebhookRepository
/// </summary>
public class InMemoryWebhookRepository : IWebhookRepository
{
    private readonly Dictionary<string, Webhook> _webhooks = new();

    public Task<IEnumerable<Webhook>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_webhooks.Values.AsEnumerable());
    }

    public Task<IEnumerable<Webhook>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_webhooks.Values.Where(w => w.IsActive).AsEnumerable());
    }

    public Task<IEnumerable<Webhook>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_webhooks.Values
            .Where(w => w.IsActive && w.EventTypes.Contains(eventType, StringComparer.OrdinalIgnoreCase))
            .AsEnumerable());
    }

    public Task<Webhook?> GetByIdAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        _webhooks.TryGetValue(webhookId, out var webhook);
        return Task.FromResult(webhook);
    }

    public Task AddAsync(Webhook webhook, CancellationToken cancellationToken = default)
    {
        _webhooks[webhook.Id] = webhook;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Webhook webhook, CancellationToken cancellationToken = default)
    {
        _webhooks[webhook.Id] = webhook;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        _webhooks.Remove(webhookId);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_webhooks.ContainsKey(webhookId));
    }
}
