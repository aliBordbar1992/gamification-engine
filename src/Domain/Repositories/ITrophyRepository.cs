using GamificationEngine.Domain.Entities;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository interface for managing trophies
/// </summary>
public interface ITrophyRepository
{
    /// <summary>
    /// Gets all trophies
    /// </summary>
    Task<IEnumerable<Trophy>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all visible trophies
    /// </summary>
    Task<IEnumerable<Trophy>> GetVisibleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a trophy by ID
    /// </summary>
    Task<Trophy?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new trophy
    /// </summary>
    Task AddAsync(Trophy trophy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing trophy
    /// </summary>
    Task UpdateAsync(Trophy trophy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a trophy by ID
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a trophy exists by ID
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
