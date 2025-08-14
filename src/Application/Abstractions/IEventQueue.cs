using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Interface for event queue operations
/// </summary>
public interface IEventQueue
{
    /// <summary>
    /// Enqueues an event for processing
    /// </summary>
    /// <param name="event">The event to enqueue</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool, DomainError>> EnqueueAsync(Event @event);

    /// <summary>
    /// Dequeues the next event for processing
    /// </summary>
    /// <returns>The next event if available, null if queue is empty</returns>
    Task<Event?> DequeueAsync();

    /// <summary>
    /// Gets the current queue size
    /// </summary>
    /// <returns>Number of events currently in the queue</returns>
    int GetQueueSize();

    /// <summary>
    /// Checks if the queue is empty
    /// </summary>
    /// <returns>True if queue is empty, false otherwise</returns>
    bool IsEmpty();
}