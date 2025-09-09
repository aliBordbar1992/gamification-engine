using GamificationEngine.Domain.Events;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service for validating events against the event catalog
/// </summary>
public interface IEventValidationService
{
    /// <summary>
    /// Validates an event against the event catalog
    /// </summary>
    /// <param name="event">The event to validate</param>
    /// <returns>True if the event is valid, false otherwise</returns>
    Task<bool> ValidateEventAsync(Event @event);

    /// <summary>
    /// Validates an event type against the event catalog
    /// </summary>
    /// <param name="eventType">The event type to validate</param>
    /// <returns>True if the event type is valid, false otherwise</returns>
    Task<bool> ValidateEventTypeAsync(string eventType);
}
