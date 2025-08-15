using System.Collections.ObjectModel;
using System.Text.Json;

namespace GamificationEngine.Domain.Events;

/// <summary>
/// Represents an event that occurred in the system
/// </summary>
public class Event
{
    // EF Core requires a parameterless constructor
    protected Event() { }

    public Event(string eventId, string eventType, string userId, DateTimeOffset occurredAt, IDictionary<string, object>? attributes = null)
    {
        if (string.IsNullOrWhiteSpace(eventId)) throw new ArgumentException("eventId cannot be empty", nameof(eventId));
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("eventType cannot be empty", nameof(eventType));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId cannot be empty", nameof(userId));

        EventId = eventId;
        EventType = eventType;
        UserId = userId;
        OccurredAt = occurredAt;
        Attributes = attributes?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();
    }

    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
}