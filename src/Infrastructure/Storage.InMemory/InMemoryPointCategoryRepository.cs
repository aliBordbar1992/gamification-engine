using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of IPointCategoryRepository
/// </summary>
public class InMemoryPointCategoryRepository : IPointCategoryRepository
{
    private readonly Dictionary<string, PointCategory> _pointCategories = new();

    public Task<IEnumerable<PointCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_pointCategories.Values.AsEnumerable());
    }

    public Task<PointCategory?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _pointCategories.TryGetValue(id, out var pointCategory);
        return Task.FromResult(pointCategory);
    }

    public Task AddAsync(PointCategory pointCategory, CancellationToken cancellationToken = default)
    {
        _pointCategories[pointCategory.Id] = pointCategory;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(PointCategory pointCategory, CancellationToken cancellationToken = default)
    {
        _pointCategories[pointCategory.Id] = pointCategory;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _pointCategories.Remove(id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_pointCategories.ContainsKey(id));
    }
}
