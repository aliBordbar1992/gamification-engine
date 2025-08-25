using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;
using GamificationEngine.Integration.Tests.Infrastructure.Models;

namespace GamificationEngine.Integration.Tests.Infrastructure.Data;

/// <summary>
/// Provides predefined test data fixtures for common testing scenarios
/// </summary>
public static class TestDataFixtures
{
    /// <summary>
    /// Creates a fixture for testing basic user onboarding flow
    /// </summary>
    public static TestDataFixture CreateOnboardingFixture()
    {
        var fixture = new TestDataFixture
        {
            Name = "User Onboarding",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "onboarding" },
                { "userCount", 1 },
                { "eventCount", 5 }
            }
        };

        var userId = "onboarding-user-1";
        var userState = new UserState(userId);
        fixture.UserStates.Add(userState);

        var events = new List<Event>
        {
            new Event(Guid.NewGuid().ToString(), "ACCOUNT_CREATED", userId, DateTimeOffset.UtcNow.AddHours(-4)),
            new Event(Guid.NewGuid().ToString(), "FIRST_LOGIN", userId, DateTimeOffset.UtcNow.AddHours(-3)),
            new Event(Guid.NewGuid().ToString(), "PROFILE_COMPLETED", userId, DateTimeOffset.UtcNow.AddHours(-2)),
            new Event(Guid.NewGuid().ToString(), "FIRST_ACTIVITY", userId, DateTimeOffset.UtcNow.AddHours(-1)),
            new Event(Guid.NewGuid().ToString(), "ONBOARDING_COMPLETED", userId, DateTimeOffset.UtcNow)
        };

        fixture.Events.AddRange(events);
        return fixture;
    }

    /// <summary>
    /// Creates a fixture for testing user engagement patterns
    /// </summary>
    public static TestDataFixture CreateEngagementFixture()
    {
        var fixture = new TestDataFixture
        {
            Name = "User Engagement",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "engagement" },
                { "userCount", 3 },
                { "eventCount", 15 }
            }
        };

        // Create users with different engagement levels
        var users = new[] { "engaged-user-1", "casual-user-2", "inactive-user-3" };

        foreach (var userId in users)
        {
            var userState = new UserState(userId);
            fixture.UserStates.Add(userState);
        }

        // Create engagement events
        var events = new List<Event>();
        var baseTime = DateTimeOffset.UtcNow.AddDays(-7);

        // Engaged user - many activities
        for (int i = 0; i < 8; i++)
        {
            events.Add(new Event(
                Guid.NewGuid().ToString(),
                "PAGE_VIEW",
                "engaged-user-1",
                baseTime.AddHours(i * 2)
            ));
        }

        // Casual user - moderate activities
        for (int i = 0; i < 5; i++)
        {
            events.Add(new Event(
                Guid.NewGuid().ToString(),
                "PAGE_VIEW",
                "casual-user-2",
                baseTime.AddDays(i * 2)
            ));
        }

        // Inactive user - minimal activities
        events.Add(new Event(
            Guid.NewGuid().ToString(),
            "PAGE_VIEW",
            "inactive-user-3",
            baseTime
        ));

        fixture.Events.AddRange(events);
        return fixture;
    }

    /// <summary>
    /// Creates a fixture for testing point accumulation scenarios
    /// </summary>
    public static TestDataFixture CreatePointAccumulationFixture()
    {
        var fixture = new TestDataFixture
        {
            Name = "Point Accumulation",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "points" },
                { "userCount", 2 },
                { "eventCount", 12 }
            }
        };

        // Create users with different point levels
        var user1 = new UserState("points-user-1");
        user1.AddPoints("XP", 100);
        user1.AddPoints("Coins", 50);
        fixture.UserStates.Add(user1);

        var user2 = new UserState("points-user-2");
        user2.AddPoints("XP", 500);
        user2.AddPoints("Coins", 250);
        fixture.UserStates.Add(user2);

        // Create point-earning events
        var events = new List<Event>();
        var baseTime = DateTimeOffset.UtcNow.AddDays(-3);

        // User 1 - earning points
        for (int i = 0; i < 6; i++)
        {
            events.Add(new Event(
                Guid.NewGuid().ToString(),
                "ACTIVITY_COMPLETED",
                "points-user-1",
                baseTime.AddHours(i * 4),
                new Dictionary<string, object> { { "pointsEarned", 10 } }
            ));
        }

        // User 2 - earning points
        for (int i = 0; i < 6; i++)
        {
            events.Add(new Event(
                Guid.NewGuid().ToString(),
                "ACHIEVEMENT_UNLOCKED",
                "points-user-2",
                baseTime.AddHours(i * 4),
                new Dictionary<string, object> { { "pointsEarned", 25 } }
            ));
        }

        fixture.Events.AddRange(events);
        return fixture;
    }

    /// <summary>
    /// Creates a fixture for testing badge progression
    /// </summary>
    public static TestDataFixture CreateBadgeProgressionFixture()
    {
        var fixture = new TestDataFixture
        {
            Name = "Badge Progression",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "badges" },
                { "userCount", 2 },
                { "eventCount", 8 }
            }
        };

        // Create users with different badge progressions
        var user1 = new UserState("badge-user-1");
        user1.GrantBadge("FirstLogin");
        user1.GrantBadge("ActiveUser");
        fixture.UserStates.Add(user1);

        var user2 = new UserState("badge-user-2");
        user2.GrantBadge("FirstLogin");
        user2.GrantBadge("ActiveUser");
        user2.GrantBadge("PowerUser");
        user2.GrantBadge("1000Events");
        fixture.UserStates.Add(user2);

        // Create badge-earning events
        var events = new List<Event>();
        var baseTime = DateTimeOffset.UtcNow.AddDays(-5);

        // User 1 badge events
        events.Add(new Event(Guid.NewGuid().ToString(), "FIRST_LOGIN", "badge-user-1", baseTime.AddDays(1)));
        events.Add(new Event(Guid.NewGuid().ToString(), "ACTIVITY_MILESTONE", "badge-user-1", baseTime.AddDays(3)));

        // User 2 badge events
        events.Add(new Event(Guid.NewGuid().ToString(), "FIRST_LOGIN", "badge-user-2", baseTime));
        events.Add(new Event(Guid.NewGuid().ToString(), "ACTIVITY_MILESTONE", "badge-user-2", baseTime.AddDays(2)));
        events.Add(new Event(Guid.NewGuid().ToString(), "POWER_USER_MILESTONE", "badge-user-2", baseTime.AddDays(4)));
        events.Add(new Event(Guid.NewGuid().ToString(), "1000_EVENTS_MILESTONE", "badge-user-2", baseTime.AddDays(5)));

        fixture.Events.AddRange(events);
        return fixture;
    }

    /// <summary>
    /// Creates a fixture for testing event sequences and temporal conditions
    /// </summary>
    public static TestDataFixture CreateEventSequenceFixture()
    {
        var fixture = new TestDataFixture
        {
            Name = "Event Sequences",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "sequences" },
                { "userCount", 2 },
                { "eventCount", 10 }
            }
        };

        // Create users for sequence testing
        var user1 = new UserState("sequence-user-1");
        var user2 = new UserState("sequence-user-2");
        fixture.UserStates.Add(user1);
        fixture.UserStates.Add(user2);

        // Create sequential events for user 1
        var baseTime = DateTimeOffset.UtcNow.AddHours(-6);
        var sequence1 = new List<Event>
        {
            new Event(Guid.NewGuid().ToString(), "PAGE_VIEW", "sequence-user-1", baseTime),
            new Event(Guid.NewGuid().ToString(), "BUTTON_CLICKED", "sequence-user-1", baseTime.AddMinutes(5)),
            new Event(Guid.NewGuid().ToString(), "FORM_SUBMITTED", "sequence-user-1", baseTime.AddMinutes(10)),
            new Event(Guid.NewGuid().ToString(), "CONFIRMATION_RECEIVED", "sequence-user-1", baseTime.AddMinutes(15))
        };

        // Create sequential events for user 2 (different pattern)
        var sequence2 = new List<Event>
        {
            new Event(Guid.NewGuid().ToString(), "PAGE_VIEW", "sequence-user-2", baseTime.AddHours(1)),
            new Event(Guid.NewGuid().ToString(), "SEARCH_PERFORMED", "sequence-user-2", baseTime.AddHours(1).AddMinutes(2)),
            new Event(Guid.NewGuid().ToString(), "RESULT_SELECTED", "sequence-user-2", baseTime.AddHours(1).AddMinutes(5)),
            new Event(Guid.NewGuid().ToString(), "DETAILS_VIEWED", "sequence-user-2", baseTime.AddHours(1).AddMinutes(8)),
            new Event(Guid.NewGuid().ToString(), "BOOKMARK_ADDED", "sequence-user-2", baseTime.AddHours(1).AddMinutes(12))
        };

        fixture.Events.AddRange(sequence1);
        fixture.Events.AddRange(sequence2);
        return fixture;
    }

    /// <summary>
    /// Creates a fixture for testing multiple users with varied activity patterns
    /// </summary>
    public static TestDataFixture CreateMultiUserActivityFixture()
    {
        var fixture = new TestDataFixture
        {
            Name = "Multi-User Activity",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "multi-user" },
                { "userCount", 5 },
                { "eventCount", 25 }
            }
        };

        // Create users with different activity levels
        for (int i = 1; i <= 5; i++)
        {
            var userId = $"multi-user-{i}";
            var userState = new UserState(userId);

            // Add some initial points based on user number
            userState.AddPoints("XP", i * 100);
            userState.AddPoints("Coins", i * 50);

            fixture.UserStates.Add(userState);
        }

        // Create varied activity events
        var events = new List<Event>();
        var baseTime = DateTimeOffset.UtcNow.AddDays(-2);
        var eventTypes = new[] { "PAGE_VIEW", "COMMENT_POSTED", "LIKE_GIVEN", "SHARE_PERFORMED", "DOWNLOAD_COMPLETED" };

        for (int userIndex = 1; userIndex <= 5; userIndex++)
        {
            var userId = $"multi-user-{userIndex}";
            var eventCount = userIndex; // More active users get more events

            for (int eventIndex = 0; eventIndex < eventCount; eventIndex++)
            {
                var eventType = eventTypes[eventIndex % eventTypes.Length];
                var eventTime = baseTime.AddHours(userIndex * 2 + eventIndex);

                events.Add(new Event(
                    Guid.NewGuid().ToString(),
                    eventType,
                    userId,
                    eventTime,
                    new Dictionary<string, object> { { "userLevel", userIndex } }
                ));
            }
        }

        fixture.Events.AddRange(events);
        return fixture;
    }

    /// <summary>
    /// Creates a fixture for testing error scenarios and edge cases
    /// </summary>
    public static TestDataFixture CreateErrorScenarioFixture()
    {
        var fixture = new TestDataFixture
        {
            Name = "Error Scenarios",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "error-testing" },
                { "userCount", 2 },
                { "eventCount", 6 }
            }
        };

        // Create users for error testing
        var user1 = new UserState("error-user-1");
        var user2 = new UserState("error-user-2");
        fixture.UserStates.Add(user1);
        fixture.UserStates.Add(user2);

        // Create events that might trigger error conditions
        var events = new List<Event>();
        var baseTime = DateTimeOffset.UtcNow.AddHours(-1);

        // Events with potentially problematic attributes
        events.Add(new Event(
            Guid.NewGuid().ToString(),
            "TEST_EVENT",
            "error-user-1",
            baseTime,
            new Dictionary<string, object> { { "nullValue", null! }, { "emptyString", "" } }
        ));

        events.Add(new Event(
            Guid.NewGuid().ToString(),
            "TEST_EVENT",
            "error-user-1",
            baseTime.AddMinutes(5),
            new Dictionary<string, object> { { "specialChars", "!@#$%^&*()" }, { "unicode", "🚀🎯🏆" } }
        ));

        events.Add(new Event(
            Guid.NewGuid().ToString(),
            "TEST_EVENT",
            "error-user-2",
            baseTime.AddMinutes(10),
            new Dictionary<string, object> { { "veryLongString", new string('x', 1000) } }
        ));

        events.Add(new Event(
            Guid.NewGuid().ToString(),
            "TEST_EVENT",
            "error-user-2",
            baseTime.AddMinutes(15),
            new Dictionary<string, object> { { "complexObject", new { nested = "value" } } }
        ));

        fixture.Events.AddRange(events);
        return fixture;
    }
}