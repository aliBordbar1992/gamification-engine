using GamificationEngine.Domain.Entities;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository interface for managing point categories
/// </summary>
public interface IPointCategoryRepository
{
    /// <summary>
    /// Gets all point categories
    /// </summary>
    Task<IEnumerable<PointCategory>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a point category by ID
    /// </summary>
    Task<PointCategory?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new point category
    /// </summary>
    Task AddAsync(PointCategory pointCategory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing point category
    /// </summary>
    Task UpdateAsync(PointCategory pointCategory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a point category by ID
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a point category exists by ID
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
