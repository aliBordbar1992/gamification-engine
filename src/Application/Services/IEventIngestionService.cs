using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for ingesting and processing user activity events
/// </summary>
public interface IEventIngestionService
{
    /// <summary>
    /// Ingests a new event into the system
    /// </summary>
    /// <param name="event">The event to ingest</param>
    /// <returns>Result indicating success or failure with error details</returns>
    Task<Result<Event, DomainError>> IngestEventAsync(Event @event);

    /// <summary>
    /// Retrieves events for a specific user
    /// </summary>
    /// <param name="userId">The user ID to retrieve events for</param>
    /// <param name="limit">Maximum number of events to return</param>
    /// <param name="offset">Number of events to skip</param>
    /// <returns>Result containing the events or error details</returns>
    Task<Result<IEnumerable<Event>, DomainError>> GetUserEventsAsync(string userId, int limit = 100, int offset = 0);

    /// <summary>
    /// Retrieves events by type
    /// </summary>
    /// <param name="eventType">The event type to filter by</param>
    /// <param name="limit">Maximum number of events to return</param>
    /// <param name="offset">Number of events to skip</param>
    /// <returns>Result containing the events or error details</returns>
    Task<Result<IEnumerable<Event>, DomainError>> GetEventsByTypeAsync(string eventType, int limit = 100, int offset = 0);
}