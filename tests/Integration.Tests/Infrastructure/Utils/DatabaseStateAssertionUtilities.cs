using GamificationEngine.Infrastructure.Storage.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GamificationEngine.Integration.Tests.Infrastructure.Utils;

/// <summary>
/// Utility class providing database state assertion helpers for integration tests
/// </summary>
public static class DatabaseStateAssertionUtilities
{
    /// <summary>
    /// Asserts that an event exists in the database with the expected properties
    /// </summary>
    public static async Task AssertEventExistsInDatabase(
        GamificationEngineDbContext dbContext,
        string expectedEventId,
        string expectedUserId,
        string expectedEventType)
    {
        dbContext.ShouldNotBeNull();

        var @event = await dbContext.Events
            .FirstOrDefaultAsync(e => e.EventId == expectedEventId);

        @event.ShouldNotBeNull($"Event with ID '{expectedEventId}' should exist in database");
        @event.UserId.ShouldBe(expectedUserId);
        @event.EventType.ShouldBe(expectedEventType);
        @event.OccurredAt.ShouldBeGreaterThan(DateTimeOffset.MinValue);
    }

    /// <summary>
    /// Asserts that an event does not exist in the database
    /// </summary>
    public static async Task AssertEventDoesNotExistInDatabase(
        GamificationEngineDbContext dbContext,
        string eventId)
    {
        dbContext.ShouldNotBeNull();

        var @event = await dbContext.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        @event.ShouldBeNull($"Event with ID '{eventId}' should not exist in database");
    }

    /// <summary>
    /// Asserts that the expected number of events exist for a user
    /// </summary>
    public static async Task AssertUserEventCount(
        GamificationEngineDbContext dbContext,
        string userId,
        int expectedCount)
    {
        dbContext.ShouldNotBeNull();

        var actualCount = await dbContext.Events
            .CountAsync(e => e.UserId == userId);

        actualCount.ShouldBe(expectedCount, $"User '{userId}' should have {expectedCount} events in database");
    }

    /// <summary>
    /// Asserts that the expected number of events exist for a specific event type
    /// </summary>
    public static async Task AssertEventTypeCount(
        GamificationEngineDbContext dbContext,
        string eventType,
        int expectedCount)
    {
        dbContext.ShouldNotBeNull();

        var actualCount = await dbContext.Events
            .CountAsync(e => e.EventType == eventType);

        actualCount.ShouldBe(expectedCount, $"Event type '{eventType}' should have {expectedCount} occurrences in database");
    }

    /// <summary>
    /// Asserts that a user state exists in the database with the expected properties
    /// </summary>
    public static async Task AssertUserStateExistsInDatabase(
        GamificationEngineDbContext dbContext,
        string expectedUserId,
        Dictionary<string, long> expectedPointsByCategory)
    {
        dbContext.ShouldNotBeNull();

        var userState = await dbContext.UserStates
            .FirstOrDefaultAsync(u => u.UserId == expectedUserId);

        userState.ShouldNotBeNull($"User state for user '{expectedUserId}' should exist in database");
        userState.PointsByCategory.ShouldBe(expectedPointsByCategory);
        userState.Badges.ShouldNotBeNull();
        userState.Trophies.ShouldNotBeNull();
    }

    /// <summary>
    /// Asserts that a user state does not exist in the database
    /// </summary>
    public static async Task AssertUserStateDoesNotExistInDatabase(
        GamificationEngineDbContext dbContext,
        string userId)
    {
        dbContext.ShouldNotBeNull();

        var userState = await dbContext.UserStates
            .FirstOrDefaultAsync(u => u.UserId == userId);

        userState.ShouldBeNull($"User state for user '{userId}' should not exist in database");
    }

    /// <summary>
    /// Asserts that the expected number of user states exist in the database
    /// </summary>
    public static async Task AssertUserStateCount(
        GamificationEngineDbContext dbContext,
        int expectedCount)
    {
        dbContext.ShouldNotBeNull();

        var actualCount = await dbContext.UserStates.CountAsync();
        actualCount.ShouldBe(expectedCount, $"Database should contain {expectedCount} user states");
    }

    /// <summary>
    /// Asserts that the expected number of events exist in the database
    /// </summary>
    public static async Task AssertTotalEventCount(
        GamificationEngineDbContext dbContext,
        int expectedCount)
    {
        dbContext.ShouldNotBeNull();

        var actualCount = await dbContext.Events.CountAsync();
        actualCount.ShouldBe(expectedCount, $"Database should contain {expectedCount} events");
    }

    /// <summary>
    /// Asserts that an event has the expected attributes in the database
    /// </summary>
    public static async Task AssertEventAttributes(
        GamificationEngineDbContext dbContext,
        string eventId,
        Dictionary<string, object> expectedAttributes)
    {
        dbContext.ShouldNotBeNull();

        var @event = await dbContext.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        @event.ShouldNotBeNull($"Event with ID '{eventId}' should exist in database");

        foreach (var expectedAttribute in expectedAttributes)
        {
            @event.Attributes.ShouldContainKey(expectedAttribute.Key);
            @event.Attributes[expectedAttribute.Key].ShouldBe(expectedAttribute.Value);
        }
    }

    /// <summary>
    /// Asserts that a user has the expected badges in the database
    /// </summary>
    public static async Task AssertUserBadges(
        GamificationEngineDbContext dbContext,
        string userId,
        HashSet<string> expectedBadges)
    {
        dbContext.ShouldNotBeNull();

        var userState = await dbContext.UserStates
            .FirstOrDefaultAsync(u => u.UserId == userId);

        userState.ShouldNotBeNull($"User state for user '{userId}' should exist in database");
        userState.Badges.ShouldBe(expectedBadges);
    }

    /// <summary>
    /// Asserts that a user has the expected trophies in the database
    /// </summary>
    public static async Task AssertUserTrophies(
        GamificationEngineDbContext dbContext,
        string userId,
        HashSet<string> expectedTrophies)
    {
        dbContext.ShouldNotBeNull();

        var userState = await dbContext.UserStates
            .FirstOrDefaultAsync(u => u.UserId == userId);

        userState.ShouldNotBeNull($"User state for user '{userId}' should exist in database");
        userState.Trophies.ShouldBe(expectedTrophies);
    }

    /// <summary>
    /// Asserts that the database is in a clean state (no events or user states)
    /// </summary>
    public static async Task AssertDatabaseIsClean(GamificationEngineDbContext dbContext)
    {
        dbContext.ShouldNotBeNull();

        var eventCount = await dbContext.Events.CountAsync();
        var userStateCount = await dbContext.UserStates.CountAsync();

        eventCount.ShouldBe(0, "Database should contain no events");
        userStateCount.ShouldBe(0, "Database should contain no user states");
    }

    /// <summary>
    /// Asserts that events are stored in chronological order for a specific user
    /// </summary>
    public static async Task AssertUserEventsChronologicalOrder(
        GamificationEngineDbContext dbContext,
        string userId)
    {
        dbContext.ShouldNotBeNull();

        var userEvents = await dbContext.Events
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.OccurredAt)
            .ToListAsync();

        for (int i = 1; i < userEvents.Count; i++)
        {
            userEvents[i].OccurredAt.ShouldBeGreaterThanOrEqualTo(
                userEvents[i - 1].OccurredAt,
                $"Event {i} should occur after or at the same time as event {i - 1}");
        }
    }

    /// <summary>
    /// Asserts that the database transaction can be committed successfully
    /// </summary>
    public static async Task AssertDatabaseTransactionCanCommit(
        GamificationEngineDbContext dbContext,
        Func<Task> action)
    {
        dbContext.ShouldNotBeNull();

        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            await action();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}