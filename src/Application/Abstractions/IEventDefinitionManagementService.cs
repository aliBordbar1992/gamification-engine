using GamificationEngine.Application.DTOs;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for managing event definitions
/// </summary>
public interface IEventDefinitionManagementService
{
    /// <summary>
    /// Gets all event definitions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of event definitions</returns>
    Task<Result<IEnumerable<EventDefinitionDto>, string>> GetAllEventDefinitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an event definition by ID
    /// </summary>
    /// <param name="id">The event definition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The event definition if found</returns>
    Task<Result<EventDefinitionDto, string>> GetEventDefinitionByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new event definition
    /// </summary>
    /// <param name="dto">The event definition creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created event definition</returns>
    Task<Result<EventDefinitionDto, string>> CreateEventDefinitionAsync(CreateEventDefinitionDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing event definition
    /// </summary>
    /// <param name="id">The event definition ID</param>
    /// <param name="dto">The event definition update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated event definition</returns>
    Task<Result<EventDefinitionDto, string>> UpdateEventDefinitionAsync(string id, UpdateEventDefinitionDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an event definition
    /// </summary>
    /// <param name="id">The event definition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool, string>> DeleteEventDefinitionAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an event definition exists
    /// </summary>
    /// <param name="id">The event definition ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the event definition exists</returns>
    Task<Result<bool, string>> EventDefinitionExistsAsync(string id, CancellationToken cancellationToken = default);
}
