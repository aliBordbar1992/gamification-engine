using GamificationEngine.Domain.Entities;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository interface for managing badges
/// </summary>
public interface IBadgeRepository
{
    /// <summary>
    /// Gets all badges
    /// </summary>
    Task<IEnumerable<Badge>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all visible badges
    /// </summary>
    Task<IEnumerable<Badge>> GetVisibleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a badge by ID
    /// </summary>
    Task<Badge?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new badge
    /// </summary>
    Task AddAsync(Badge badge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing badge
    /// </summary>
    Task UpdateAsync(Badge badge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a badge by ID
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a badge exists by ID
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
