using GamificationEngine.Domain.Entities;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository interface for managing levels
/// </summary>
public interface ILevelRepository
{
    /// <summary>
    /// Gets all levels
    /// </summary>
    Task<IEnumerable<Level>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all levels for a specific category
    /// </summary>
    Task<IEnumerable<Level>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets levels ordered by minimum points for a specific category
    /// </summary>
    Task<IEnumerable<Level>> GetByCategoryOrderedAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a level by ID
    /// </summary>
    Task<Level?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the appropriate level for a given point amount in a category
    /// </summary>
    Task<Level?> GetLevelForPointsAsync(string category, long points, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new level
    /// </summary>
    Task AddAsync(Level level, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing level
    /// </summary>
    Task UpdateAsync(Level level, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a level by ID
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a level exists by ID
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
