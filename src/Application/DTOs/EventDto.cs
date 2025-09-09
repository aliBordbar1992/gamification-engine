using System.Text.Json.Serialization;

namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data Transfer Object for Event entities
/// </summary>
public class EventDto
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    [JsonPropertyName("eventId")]
    public string EventId { get; set; } = Guid.NewGuid().ToString().Replace("-", "");

    /// <summary>
    /// Type of the event (e.g., "USER_COMMENTED", "PRODUCT_PURCHASED")
    /// </summary>
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who performed the action
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// When the event occurred
    /// </summary>
    [JsonPropertyName("occurredAt")]
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>
    /// Additional attributes for the event
    /// </summary>
    [JsonPropertyName("attributes")]
    public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Creates an EventDto from a domain Event
    /// </summary>
    /// <param name="event">The domain event</param>
    /// <returns>EventDto representation</returns>
    public static EventDto FromDomain(Domain.Events.Event @event)
    {
        return new EventDto
        {
            EventId = @event.EventId,
            EventType = @event.EventType,
            UserId = @event.UserId,
            OccurredAt = @event.OccurredAt,
            Attributes = @event.Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }

    /// <summary>
    /// Creates a collection of EventDto from domain Events
    /// </summary>
    /// <param name="events">Collection of domain events</param>
    /// <returns>Collection of EventDto representations</returns>
    public static IEnumerable<EventDto> FromDomain(IEnumerable<Domain.Events.Event> events)
    {
        return events.Select(FromDomain);
    }
}