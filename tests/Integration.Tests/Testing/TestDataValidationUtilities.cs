using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;
using GamificationEngine.Integration.Tests.Infrastructure.Testing;

namespace GamificationEngine.Integration.Tests.Testing;

/// <summary>
/// Utilities for validating test data and ensuring data integrity
/// </summary>
public static class TestDataValidationUtilities
{
    /// <summary>
    /// Validates that a test event meets basic requirements
    /// </summary>
    public static bool ValidateEvent(Event @event)
    {
        if (@event == null) return false;

        return !string.IsNullOrWhiteSpace(@event.EventId) &&
               !string.IsNullOrWhiteSpace(@event.EventType) &&
               !string.IsNullOrWhiteSpace(@event.UserId) &&
               @event.OccurredAt != default;
    }

    /// <summary>
    /// Validates that a test user state meets basic requirements
    /// </summary>
    public static bool ValidateUserState(UserState userState)
    {
        if (userState == null) return false;

        return !string.IsNullOrWhiteSpace(userState.UserId);
    }

    /// <summary>
    /// Validates that a test data fixture is properly structured
    /// </summary>
    public static bool ValidateFixture(TestDataFixture fixture)
    {
        if (fixture == null) return false;

        if (string.IsNullOrWhiteSpace(fixture.Name))
            return false;

        // Validate all events in the fixture
        if (fixture.Events.Any(e => !ValidateEvent(e)))
            return false;

        // Validate all user states in the fixture
        if (fixture.UserStates.Any(u => !ValidateUserState(u)))
            return false;

        return true;
    }

    /// <summary>
    /// Validates that events for a specific user are properly ordered by time
    /// </summary>
    public static bool ValidateEventTimeOrdering(string userId, IEnumerable<Event> events)
    {
        var userEvents = events.Where(e => e.UserId == userId).OrderBy(e => e.OccurredAt).ToList();

        for (int i = 1; i < userEvents.Count; i++)
        {
            if (userEvents[i].OccurredAt <= userEvents[i - 1].OccurredAt)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that user states have consistent point totals
    /// </summary>
    public static bool ValidateUserStatePointConsistency(UserState userState, IEnumerable<Event> events)
    {
        if (userState == null || events == null) return false;

        // This is a placeholder for future validation logic
        // When the rules engine is implemented, we can validate that
        // user state points match what would be calculated from events
        return true;
    }

    /// <summary>
    /// Validates that event attributes are within acceptable ranges
    /// </summary>
    public static bool ValidateEventAttributes(Event @event)
    {
        if (@event?.Attributes == null) return true; // No attributes is valid

        foreach (var kvp in @event.Attributes)
        {
            // Check for null keys
            if (string.IsNullOrWhiteSpace(kvp.Key))
                return false;

            // Check for extremely long values
            if (kvp.Value is string strValue && strValue.Length > 10000)
                return false;

            // Check for extremely long keys
            if (kvp.Key.Length > 1000)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a collection of events has no duplicate event IDs
    /// </summary>
    public static bool ValidateNoDuplicateEventIds(IEnumerable<Event> events)
    {
        if (events == null) return true;

        var eventIds = events.Select(e => e.EventId).ToList();
        var uniqueIds = eventIds.Distinct().ToList();

        return eventIds.Count == uniqueIds.Count;
    }

    /// <summary>
    /// Validates that user IDs are properly formatted
    /// </summary>
    public static bool ValidateUserIdFormat(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return false;

        // Check for reasonable length
        if (userId.Length > 100) return false;

        // Check for valid characters (basic validation)
        if (userId.Any(c => char.IsControl(c))) return false;

        return true;
    }

    /// <summary>
    /// Validates that event types are properly formatted
    /// </summary>
    public static bool ValidateEventTypeFormat(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType)) return false;

        // Check for reasonable length
        if (eventType.Length > 100) return false;

        // Check for valid characters (basic validation)
        if (eventType.Any(c => char.IsControl(c))) return false;

        return true;
    }

    /// <summary>
    /// Validates that timestamps are within reasonable bounds
    /// </summary>
    public static bool ValidateTimestamp(DateTimeOffset timestamp)
    {
        var now = DateTimeOffset.UtcNow;
        var minAllowed = now.AddYears(-10); // Events shouldn't be older than 10 years
        var maxAllowed = now.AddDays(1);    // Events shouldn't be more than 1 day in the future

        return timestamp >= minAllowed && timestamp <= maxAllowed;
    }

    /// <summary>
    /// Comprehensive validation of test data fixture
    /// </summary>
    public static (bool IsValid, List<string> Errors) ValidateFixtureComprehensive(TestDataFixture fixture)
    {
        var errors = new List<string>();

        if (fixture == null)
        {
            errors.Add("Fixture is null");
            return (false, errors);
        }

        // Basic fixture validation
        if (!ValidateFixture(fixture))
            errors.Add("Basic fixture validation failed");

        // Check for duplicate event IDs
        if (!ValidateNoDuplicateEventIds(fixture.Events))
            errors.Add("Duplicate event IDs found");

        // Validate individual events
        foreach (var @event in fixture.Events)
        {
            if (!ValidateEvent(@event))
                errors.Add($"Invalid event: {@event.EventId}");

            if (!ValidateEventAttributes(@event))
                errors.Add($"Invalid event attributes: {@event.EventId}");

            if (!ValidateUserIdFormat(@event.UserId))
                errors.Add($"Invalid user ID format: {@event.UserId}");

            if (!ValidateEventTypeFormat(@event.EventType))
                errors.Add($"Invalid event type format: {@event.EventType}");

            if (!ValidateTimestamp(@event.OccurredAt))
                errors.Add($"Invalid timestamp: {@event.OccurredAt}");
        }

        // Validate individual user states
        foreach (var userState in fixture.UserStates)
        {
            if (!ValidateUserState(userState))
                errors.Add($"Invalid user state: {userState.UserId}");

            if (!ValidateUserIdFormat(userState.UserId))
                errors.Add($"Invalid user ID format in user state: {userState.UserId}");
        }

        // Validate time ordering for each user
        var userGroups = fixture.Events.GroupBy(e => e.UserId);
        foreach (var userGroup in userGroups)
        {
            if (!ValidateEventTimeOrdering(userGroup.Key, userGroup))
                errors.Add($"Invalid time ordering for user: {userGroup.Key}");
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Validates that test data statistics are consistent with actual data
    /// </summary>
    public static bool ValidateStatisticsConsistency(TestDataFixture fixture, TestDataStatistics statistics)
    {
        if (fixture == null || statistics == null) return false;

        var actualEventCount = fixture.Events.Count;
        var actualUserStateCount = fixture.UserStates.Count;
        var actualUniqueEventTypes = fixture.Events.Select(e => e.EventType).Distinct().Count();
        var actualUniqueUsers = fixture.Events.Select(e => e.UserId).Distinct().Count();

        if (statistics.TotalEvents != actualEventCount) return false;
        if (statistics.TotalUserStates != actualUserStateCount) return false;
        if (statistics.UniqueEventTypes != actualUniqueEventTypes) return false;
        if (statistics.UniqueUsers != actualUniqueUsers) return false;

        return true;
    }
}