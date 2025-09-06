using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of IBadgeRepository
/// </summary>
public class InMemoryBadgeRepository : IBadgeRepository
{
    private readonly Dictionary<string, Badge> _badges = new();

    public Task<IEnumerable<Badge>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_badges.Values.AsEnumerable());
    }

    public Task<IEnumerable<Badge>> GetVisibleAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_badges.Values.Where(b => b.Visible).AsEnumerable());
    }

    public Task<Badge?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _badges.TryGetValue(id, out var badge);
        return Task.FromResult(badge);
    }

    public Task AddAsync(Badge badge, CancellationToken cancellationToken = default)
    {
        _badges[badge.Id] = badge;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Badge badge, CancellationToken cancellationToken = default)
    {
        _badges[badge.Id] = badge;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _badges.Remove(id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_badges.ContainsKey(id));
    }
}
