using Microsoft.EntityFrameworkCore;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;
using GamificationEngine.Infrastructure.Storage.EntityFramework;
using GamificationEngine.Integration.Tests.Infrastructure;
using GamificationEngine.Integration.Tests.Infrastructure.Testing;

namespace GamificationEngine.Integration.Tests.Database;

/// <summary>
/// Utilities for managing test database state, seeding, and cleanup
/// </summary>
public static class TestDatabaseUtilities
{
    /// <summary>
    /// Seeds the database with common test data
    /// </summary>
    public static async Task SeedCommonTestDataAsync(GamificationEngineDbContext context)
    {
        // Seed test users
        var testUsers = new[]
        {
            new UserState("test-user-1"),
            new UserState("test-user-2"),
            new UserState("test-user-3")
        };

        // Add some initial points to users
        testUsers[0].AddPoints("XP", 100);
        testUsers[0].AddPoints("Coins", 50);
        testUsers[0].GrantBadge("FirstLogin");

        testUsers[1].AddPoints("XP", 250);
        testUsers[1].AddPoints("Coins", 125);
        testUsers[1].GrantBadge("ActiveUser");

        testUsers[2].AddPoints("XP", 500);
        testUsers[2].AddPoints("Coins", 250);
        testUsers[2].GrantBadge("PowerUser");
        // Note: GrantTrophy method doesn't exist yet in UserState

        context.UserStates.AddRange(testUsers);

        // Seed test events
        var testEvents = new[]
        {
            TestDataBuilder.CreateTestEvent("test-user-1", "USER_LOGIN", DateTime.UtcNow.AddHours(-2)),
            TestDataBuilder.CreateTestEvent("test-user-1", "PAGE_VIEW", DateTime.UtcNow.AddHours(-1)),
            TestDataBuilder.CreateTestEvent("test-user-2", "USER_LOGIN", DateTime.UtcNow.AddHours(-3)),
            TestDataBuilder.CreateTestEvent("test-user-2", "COMMENT_POSTED", DateTime.UtcNow.AddHours(-2)),
            TestDataBuilder.CreateTestEvent("test-user-3", "USER_LOGIN", DateTime.UtcNow.AddHours(-4)),
            TestDataBuilder.CreateTestEvent("test-user-3", "ACHIEVEMENT_UNLOCKED", DateTime.UtcNow.AddHours(-3))
        };

        context.Events.AddRange(testEvents);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the database with events for testing specific scenarios
    /// </summary>
    public static async Task SeedEventTestDataAsync(GamificationEngineDbContext context, string userId, int eventCount = 10)
    {
        var events = TestDataBuilder.CreateTestEvents(userId, eventCount, "TEST_EVENT");
        context.Events.AddRange(events);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the database with user states for testing point calculations
    /// </summary>
    public static async Task SeedUserStateTestDataAsync(GamificationEngineDbContext context, Dictionary<string, Dictionary<string, long>> userPoints)
    {
        foreach (var kvp in userPoints)
        {
            var userId = kvp.Key;
            var pointsByCategory = kvp.Value;

            var userState = new UserState(userId);
            foreach (var categoryKvp in pointsByCategory)
            {
                userState.AddPoints(categoryKvp.Key, categoryKvp.Value);
            }

            context.UserStates.Add(userState);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Cleans up all test data from the database
    /// </summary>
    public static async Task CleanupAllTestDataAsync(GamificationEngineDbContext context)
    {
        context.Events.RemoveRange(context.Events);
        context.UserStates.RemoveRange(context.UserStates);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Verifies that the database is in a clean state
    /// </summary>
    public static async Task<bool> IsDatabaseCleanAsync(GamificationEngineDbContext context)
    {
        var eventCount = await context.Events.CountAsync();
        var userStateCount = await context.UserStates.CountAsync();

        return eventCount == 0 && userStateCount == 0;
    }

    /// <summary>
    /// Gets the current count of entities in the database
    /// </summary>
    public static async Task<(int EventCount, int UserStateCount)> GetEntityCountsAsync(GamificationEngineDbContext context)
    {
        var eventCount = await context.Events.CountAsync();
        var userStateCount = await context.UserStates.CountAsync();

        return (eventCount, userStateCount);
    }

    /// <summary>
    /// Resets the database to a known state for testing
    /// </summary>
    public static async Task ResetDatabaseAsync(GamificationEngineDbContext context)
    {
        await CleanupAllTestDataAsync(context);
        await SeedCommonTestDataAsync(context);
    }
}