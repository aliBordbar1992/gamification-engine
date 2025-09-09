using GamificationEngine.Domain.Entities;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository interface for managing event definitions
/// </summary>
public interface IEventDefinitionRepository
{
    /// <summary>
    /// Gets all event definitions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of event definitions</returns>
    Task<IEnumerable<EventDefinition>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an event definition by ID
    /// </summary>
    /// <param name="id">The event definition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The event definition if found, null otherwise</returns>
    Task<EventDefinition?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new event definition
    /// </summary>
    /// <param name="eventDefinition">The event definition to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task AddAsync(EventDefinition eventDefinition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing event definition
    /// </summary>
    /// <param name="eventDefinition">The event definition to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task UpdateAsync(EventDefinition eventDefinition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an event definition by ID
    /// </summary>
    /// <param name="id">The event definition ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an event definition exists by ID
    /// </summary>
    /// <param name="id">The event definition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the event definition exists, false otherwise</returns>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
