using System.Collections.ObjectModel;

namespace GamificationEngine.Domain.Events;

/// <summary>
/// Platform-agnostic user activity event normalized by ingestion layer.
/// </summary>
public sealed class Event
{
    public string EventId { get; }
    public string EventType { get; }
    public string UserId { get; }
    public DateTimeOffset OccurredAt { get; }
    public IReadOnlyDictionary<string, object> Attributes { get; }

    public Event(string eventId, string eventType, string userId, DateTimeOffset occurredAt, IDictionary<string, object>? attributes = null)
    {
        if (string.IsNullOrWhiteSpace(eventId)) throw new ArgumentException("eventId cannot be empty", nameof(eventId));
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("eventType cannot be empty", nameof(eventType));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId cannot be empty", nameof(userId));

        EventId = eventId;
        EventType = eventType;
        UserId = userId;
        OccurredAt = occurredAt;
        Attributes = new ReadOnlyDictionary<string, object>(attributes ?? new Dictionary<string, object>());
    }
}