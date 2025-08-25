using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;
using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

namespace GamificationEngine.Integration.Tests.Infrastructure.Data;

/// <summary>
/// Factory class for creating common test data scenarios and patterns
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// Creates a test scenario for user progression testing
    /// </summary>
    public static TestDataFixture CreateUserProgressionScenario(
        int userCount = 3,
        int eventsPerUser = 10,
        bool includePoints = true,
        bool includeBadges = true)
    {
        var fixture = new TestDataFixture
        {
            Name = "User Progression",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "progression" },
                { "userCount", userCount },
                { "eventsPerUser", eventsPerUser },
                { "includePoints", includePoints },
                { "includeBadges", includeBadges }
            }
        };

        for (int i = 1; i <= userCount; i++)
        {
            var userId = $"progression-user-{i}";
            var userState = new UserState(userId);

            if (includePoints)
            {
                userState.AddPoints("XP", i * 100);
                userState.AddPoints("Coins", i * 50);
            }

            if (includeBadges)
            {
                if (i >= 1) userState.GrantBadge("FirstLogin");
                if (i >= 2) userState.GrantBadge("ActiveUser");
                if (i >= 3) userState.GrantBadge("PowerUser");
            }

            fixture.UserStates.Add(userState);

            // Create progression events
            var baseTime = DateTimeOffset.UtcNow.AddDays(-i);
            for (int j = 0; j < eventsPerUser; j++)
            {
                var eventTime = baseTime.AddHours(j * 2);
                var eventType = GetProgressionEventType(j);

                fixture.Events.Add(new Event(
                    Guid.NewGuid().ToString(),
                    eventType,
                    userId,
                    eventTime,
                    new Dictionary<string, object> { { "progressionLevel", i }, { "eventIndex", j } }
                ));
            }
        }

        return fixture;
    }

    /// <summary>
    /// Creates a test scenario for event correlation testing
    /// </summary>
    public static TestDataFixture CreateEventCorrelationScenario(
        int userCount = 2,
        int correlationGroups = 3)
    {
        var fixture = new TestDataFixture
        {
            Name = "Event Correlation",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "correlation" },
                { "userCount", userCount },
                { "correlationGroups", correlationGroups }
            }
        };

        for (int i = 1; i <= userCount; i++)
        {
            var userId = $"correlation-user-{i}";
            var userState = new UserState(userId);
            fixture.UserStates.Add(userState);

            // Create correlated event sequences
            for (int group = 0; group < correlationGroups; group++)
            {
                var baseTime = DateTimeOffset.UtcNow.AddHours(-group * 4);
                var correlationId = $"correlation-{i}-{group}";

                // First event in sequence
                fixture.Events.Add(new Event(
                    Guid.NewGuid().ToString(),
                    "SEARCH_STARTED",
                    userId,
                    baseTime,
                    new Dictionary<string, object> { { "correlationId", correlationId }, { "sequence", 1 } }
                ));

                // Second event in sequence
                fixture.Events.Add(new Event(
                    Guid.NewGuid().ToString(),
                    "RESULT_SELECTED",
                    userId,
                    baseTime.AddMinutes(2),
                    new Dictionary<string, object> { { "correlationId", correlationId }, { "sequence", 2 } }
                ));

                // Third event in sequence
                fixture.Events.Add(new Event(
                    Guid.NewGuid().ToString(),
                    "DETAILS_VIEWED",
                    userId,
                    baseTime.AddMinutes(5),
                    new Dictionary<string, object> { { "correlationId", correlationId }, { "sequence", 3 } }
                ));
            }
        }

        return fixture;
    }

    /// <summary>
    /// Creates a test scenario for time-based condition testing
    /// </summary>
    public static TestDataFixture CreateTimeBasedConditionScenario(
        int userCount = 2,
        int timeWindows = 3)
    {
        var fixture = new TestDataFixture
        {
            Name = "Time-Based Conditions",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "time-based" },
                { "userCount", userCount },
                { "timeWindows", timeWindows }
            }
        };

        for (int i = 1; i <= userCount; i++)
        {
            var userId = $"time-user-{i}";
            var userState = new UserState(userId);
            fixture.UserStates.Add(userState);

            // Create events across different time windows
            for (int window = 0; window < timeWindows; window++)
            {
                var windowStart = DateTimeOffset.UtcNow.AddDays(-window * 7);

                // Daily activity pattern
                for (int day = 0; day < 7; day++)
                {
                    var dayStart = windowStart.AddDays(day);

                    // Morning activity
                    fixture.Events.Add(new Event(
                        Guid.NewGuid().ToString(),
                        "MORNING_LOGIN",
                        userId,
                        dayStart.AddHours(8),
                        new Dictionary<string, object> { { "timeWindow", window }, { "dayOfWeek", dayStart.DayOfWeek } }
                    ));

                    // Afternoon activity
                    fixture.Events.Add(new Event(
                        Guid.NewGuid().ToString(),
                        "AFTERNOON_ACTIVITY",
                        userId,
                        dayStart.AddHours(14),
                        new Dictionary<string, object> { { "timeWindow", window }, { "dayOfWeek", dayStart.DayOfWeek } }
                    ));

                    // Evening activity
                    fixture.Events.Add(new Event(
                        Guid.NewGuid().ToString(),
                        "EVENING_ACTIVITY",
                        userId,
                        dayStart.AddHours(20),
                        new Dictionary<string, object> { { "timeWindow", window }, { "dayOfWeek", dayStart.DayOfWeek } }
                    ));
                }
            }
        }

        return fixture;
    }

    /// <summary>
    /// Creates a test scenario for attribute-based condition testing
    /// </summary>
    public static TestDataFixture CreateAttributeBasedConditionScenario(
        int userCount = 3,
        int attributeVariations = 5)
    {
        var fixture = new TestDataFixture
        {
            Name = "Attribute-Based Conditions",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "attribute-based" },
                { "userCount", userCount },
                { "attributeVariations", attributeVariations }
            }
        };

        var attributeSets = new[]
        {
            new Dictionary<string, object> { { "device", "mobile" }, { "browser", "chrome" }, { "location", "US" } },
            new Dictionary<string, object> { { "device", "desktop" }, { "browser", "firefox" }, { "location", "EU" } },
            new Dictionary<string, object> { { "device", "tablet" }, { "browser", "safari" }, { "location", "ASIA" } },
            new Dictionary<string, object> { { "device", "mobile" }, { "browser", "edge" }, { "location", "US" } },
            new Dictionary<string, object> { { "device", "desktop" }, { "browser", "chrome" }, { "location", "EU" } }
        };

        for (int i = 1; i <= userCount; i++)
        {
            var userId = $"attribute-user-{i}";
            var userState = new UserState(userId);
            fixture.UserStates.Add(userState);

            // Create events with different attribute combinations
            for (int j = 0; j < attributeVariations; j++)
            {
                var attributes = attributeSets[j % attributeSets.Length].ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value
                );

                // Add user-specific attributes
                attributes["userId"] = userId;
                attributes["eventIndex"] = j;
                attributes["userLevel"] = i;

                fixture.Events.Add(new Event(
                    Guid.NewGuid().ToString(),
                    "ATTRIBUTE_TEST_EVENT",
                    userId,
                    DateTimeOffset.UtcNow.AddHours(-j),
                    attributes
                ));
            }
        }

        return fixture;
    }

    /// <summary>
    /// Creates a test scenario for performance testing with large datasets
    /// </summary>
    public static TestDataFixture CreatePerformanceTestScenario(
        int userCount = 10,
        int eventsPerUser = 100)
    {
        var fixture = new TestDataFixture
        {
            Name = "Performance Test",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "performance" },
                { "userCount", userCount },
                { "eventsPerUser", eventsPerUser },
                { "totalEvents", userCount * eventsPerUser }
            }
        };

        for (int i = 1; i <= userCount; i++)
        {
            var userId = $"perf-user-{i}";
            var userState = new UserState(userId);

            // Add some initial state
            userState.AddPoints("XP", i * 1000);
            userState.AddPoints("Coins", i * 500);

            fixture.UserStates.Add(userState);

            // Create high-volume events
            var baseTime = DateTimeOffset.UtcNow.AddDays(-30);
            for (int j = 0; j < eventsPerUser; j++)
            {
                var eventTime = baseTime.AddMinutes(j * 43); // Spread events over time
                var eventType = GetPerformanceEventType(j);

                fixture.Events.Add(new Event(
                    Guid.NewGuid().ToString(),
                    eventType,
                    userId,
                    eventTime,
                    new Dictionary<string, object>
                    {
                        { "userIndex", i },
                        { "eventIndex", j },
                        { "timestamp", eventTime.ToUnixTimeSeconds() }
                    }
                ));
            }
        }

        return fixture;
    }

    /// <summary>
    /// Creates a test scenario for edge case testing
    /// </summary>
    public static TestDataFixture CreateEdgeCaseScenario()
    {
        var fixture = new TestDataFixture
        {
            Name = "Edge Cases",
            Metadata = new Dictionary<string, object>
            {
                { "scenario", "edge-cases" },
                { "userCount", 2 },
                { "eventCount", 8 }
            }
        };

        // User with very long user ID
        var longUserId = new string('x', 100);
        var userState1 = new UserState(longUserId);
        fixture.UserStates.Add(userState1);

        // User with special characters in ID
        var specialUserId = "user-!@#$%^&*()_+-=[]{}|;':\",./<>?";
        var userState2 = new UserState(specialUserId);
        fixture.UserStates.Add(userState2);

        // Events with edge case attributes
        var edgeCaseEvents = new[]
        {
            new Event(
                Guid.NewGuid().ToString(),
                "EDGE_CASE_1",
                longUserId,
                DateTimeOffset.UtcNow.AddYears(-5), // Very old event
                new Dictionary<string, object> { { "nullValue", null! }, { "emptyString", "" } }
            ),
            new Event(
                Guid.NewGuid().ToString(),
                "EDGE_CASE_2",
                longUserId,
                DateTimeOffset.UtcNow.AddDays(1), // Future event
                new Dictionary<string, object> { { "veryLongString", new string('y', 5000) } }
            ),
            new Event(
                Guid.NewGuid().ToString(),
                "EDGE_CASE_3",
                specialUserId,
                DateTimeOffset.UtcNow,
                new Dictionary<string, object> { { "unicode", "🚀🎯🏆💎🔥" }, { "specialChars", "!@#$%^&*()" } }
            ),
            new Event(
                Guid.NewGuid().ToString(),
                "EDGE_CASE_4",
                specialUserId,
                DateTimeOffset.UtcNow.AddMilliseconds(1),
                new Dictionary<string, object> { { "complexObject", new { nested = new { deep = "value" } } } }
            )
        };

        fixture.Events.AddRange(edgeCaseEvents);

        return fixture;
    }

    /// <summary>
    /// Gets the appropriate event type for progression testing based on index
    /// </summary>
    private static string GetProgressionEventType(int index)
    {
        return index switch
        {
            0 => "ACCOUNT_CREATED",
            1 => "FIRST_LOGIN",
            2 => "PROFILE_COMPLETED",
            3 => "FIRST_ACTIVITY",
            4 => "MILESTONE_REACHED",
            5 => "ACHIEVEMENT_UNLOCKED",
            6 => "LEVEL_UP",
            7 => "BADGE_EARNED",
            8 => "TROPHY_UNLOCKED",
            _ => "ACTIVITY_COMPLETED"
        };
    }

    /// <summary>
    /// Gets the appropriate event type for performance testing based on index
    /// </summary>
    private static string GetPerformanceEventType(int index)
    {
        var eventTypes = new[]
        {
            "PAGE_VIEW", "BUTTON_CLICK", "FORM_SUBMIT", "API_CALL", "DATA_ACCESS",
            "CACHE_HIT", "CACHE_MISS", "DATABASE_QUERY", "FILE_UPLOAD", "NOTIFICATION_SENT"
        };

        return eventTypes[index % eventTypes.Length];
    }
}