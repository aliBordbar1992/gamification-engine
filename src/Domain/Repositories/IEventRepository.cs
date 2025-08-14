using GamificationEngine.Domain.Events;

namespace GamificationEngine.Domain.Repositories;

/// <summary>
/// Repository for storing and retrieving events
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Stores a new event
    /// </summary>
    /// <param name="event">The event to store</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task StoreAsync(Event @event);

    /// <summary>
    /// Retrieves events for a specific user
    /// </summary>
    /// <param name="userId">The user ID to retrieve events for</param>
    /// <param name="limit">Maximum number of events to return</param>
    /// <param name="offset">Number of events to skip</param>
    /// <returns>Collection of events for the user</returns>
    Task<IEnumerable<Event>> GetByUserIdAsync(string userId, int limit, int offset);

    /// <summary>
    /// Retrieves events by type
    /// </summary>
    /// <param name="eventType">The event type to filter by</param>
    /// <param name="limit">Maximum number of events to return</param>
    /// <param name="offset">Number of events to skip</param>
    /// <returns>Collection of events of the specified type</returns>
    Task<IEnumerable<Event>> GetByTypeAsync(string eventType, int limit, int offset);

    /// <summary>
    /// Retrieves an event by its ID
    /// </summary>
    /// <param name="eventId">The event ID to retrieve</param>
    /// <returns>The event if found, null otherwise</returns>
    Task<Event?> GetByIdAsync(string eventId);
}