using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of ITrophyRepository
/// </summary>
public class InMemoryTrophyRepository : ITrophyRepository
{
    private readonly Dictionary<string, Trophy> _trophies = new();

    public Task<IEnumerable<Trophy>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_trophies.Values.AsEnumerable());
    }

    public Task<IEnumerable<Trophy>> GetVisibleAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_trophies.Values.Where(t => t.Visible).AsEnumerable());
    }

    public Task<Trophy?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _trophies.TryGetValue(id, out var trophy);
        return Task.FromResult(trophy);
    }

    public Task AddAsync(Trophy trophy, CancellationToken cancellationToken = default)
    {
        _trophies[trophy.Id] = trophy;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Trophy trophy, CancellationToken cancellationToken = default)
    {
        _trophies[trophy.Id] = trophy;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _trophies.Remove(id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_trophies.ContainsKey(id));
    }
}
