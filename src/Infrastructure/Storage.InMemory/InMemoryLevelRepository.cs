using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of ILevelRepository
/// </summary>
public class InMemoryLevelRepository : ILevelRepository
{
    private readonly Dictionary<string, Level> _levels = new();

    public Task<IEnumerable<Level>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_levels.Values.AsEnumerable());
    }

    public Task<IEnumerable<Level>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_levels.Values.Where(l => l.Category == category).AsEnumerable());
    }

    public Task<IEnumerable<Level>> GetByCategoryOrderedAsync(string category, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_levels.Values
            .Where(l => l.Category == category)
            .OrderBy(l => l.MinPoints)
            .AsEnumerable());
    }

    public Task<Level?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _levels.TryGetValue(id, out var level);
        return Task.FromResult(level);
    }

    public Task<Level?> GetLevelForPointsAsync(string category, long points, CancellationToken cancellationToken = default)
    {
        var level = _levels.Values
            .Where(l => l.Category == category && l.MinPoints <= points)
            .OrderByDescending(l => l.MinPoints)
            .FirstOrDefault();

        return Task.FromResult(level);
    }

    public Task AddAsync(Level level, CancellationToken cancellationToken = default)
    {
        _levels[level.Id] = level;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Level level, CancellationToken cancellationToken = default)
    {
        _levels[level.Id] = level;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _levels.Remove(id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_levels.ContainsKey(id));
    }
}
