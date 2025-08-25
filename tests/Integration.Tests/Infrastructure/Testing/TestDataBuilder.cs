using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Builder class for creating test data entities used in integration tests
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a test event with default values
    /// </summary>
    public static Event CreateTestEvent(
        string userId = "test-user-1",
        string eventType = "TEST_EVENT",
        DateTime? timestamp = null,
        Dictionary<string, object>? attributes = null)
    {
        return new Event(
            eventId: Guid.NewGuid().ToString(),
            eventType: eventType,
            userId: userId,
            occurredAt: timestamp?.ToUniversalTime() ?? DateTimeOffset.UtcNow,
            attributes: attributes ?? new Dictionary<string, object>
            {
                { "source", "integration-test" },
                { "testId", Guid.NewGuid().ToString() }
            }
        );
    }

    /// <summary>
    /// Creates a test user state with default values
    /// </summary>
    public static UserState CreateTestUserState(
        string userId = "test-user-1",
        Dictionary<string, long>? pointsByCategory = null,
        List<string>? badges = null)
    {
        var userState = new UserState(userId);

        if (pointsByCategory != null)
        {
            foreach (var kvp in pointsByCategory)
            {
                userState.AddPoints(kvp.Key, kvp.Value);
            }
        }

        if (badges != null)
        {
            foreach (var badge in badges)
            {
                userState.GrantBadge(badge);
            }
        }

        return userState;
    }

    /// <summary>
    /// Creates multiple test events for a user
    /// </summary>
    public static List<Event> CreateTestEvents(
        string userId,
        int count,
        string eventType = "TEST_EVENT",
        TimeSpan? interval = null)
    {
        var events = new List<Event>();
        var baseTime = DateTime.UtcNow;
        var timeInterval = interval ?? TimeSpan.FromMinutes(1);

        for (int i = 0; i < count; i++)
        {
            var eventTime = baseTime.Add(timeInterval * i);
            events.Add(CreateTestEvent(userId, eventType, eventTime));
        }

        return events;
    }

    /// <summary>
    /// Creates test events with different types for a user
    /// </summary>
    public static List<Event> CreateMixedEventTypes(
        string userId,
        Dictionary<string, int> eventTypeCounts)
    {
        var events = new List<Event>();
        var baseTime = DateTime.UtcNow;
        var currentTime = baseTime;

        foreach (var kvp in eventTypeCounts)
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                events.Add(CreateTestEvent(userId, kvp.Key, currentTime));
                currentTime = currentTime.AddMinutes(1);
            }
        }

        return events;
    }

    /// <summary>
    /// Creates test events with specific attributes for testing conditions
    /// </summary>
    public static Event CreateEventWithAttributes(
        string userId,
        string eventType,
        Dictionary<string, object> attributes)
    {
        return new Event(
            eventId: Guid.NewGuid().ToString(),
            eventType: eventType,
            userId: userId,
            occurredAt: DateTimeOffset.UtcNow,
            attributes: attributes
        );
    }

    /// <summary>
    /// Creates a sequence of related events for testing temporal conditions
    /// </summary>
    public static List<Event> CreateEventSequence(
        string userId,
        List<(string EventType, TimeSpan Delay)> sequence)
    {
        var events = new List<Event>();
        var baseTime = DateTime.UtcNow;
        var currentTime = baseTime;

        foreach (var (eventType, delay) in sequence)
        {
            events.Add(CreateTestEvent(userId, eventType, currentTime));
            currentTime = currentTime.Add(delay);
        }

        return events;
    }
}