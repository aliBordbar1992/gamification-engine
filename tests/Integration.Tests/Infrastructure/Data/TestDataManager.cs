using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;
using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Data;

/// <summary>
/// Manages test data creation, validation, and cleanup for integration tests
/// </summary>
public class TestDataManager : ITestDataManager
{
    private readonly ITestDatabase _testDatabase;
    private readonly ILogger<TestDataManager> _logger;
    private readonly Dictionary<string, TestDataFixture> _fixtures;
    private readonly TestDataStatistics _statistics;

    public TestDataManager(ITestDatabase testDatabase, ILogger<TestDataManager> logger)
    {
        _testDatabase = testDatabase ?? throw new ArgumentNullException(nameof(testDatabase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fixtures = new Dictionary<string, TestDataFixture>();
        _statistics = new TestDataStatistics();
    }

    /// <summary>
    /// Creates test events with specified parameters
    /// </summary>
    public Event CreateEvent(
        string userId = "test-user-1",
        string eventType = "TEST_EVENT",
        DateTime? timestamp = null,
        Dictionary<string, object>? attributes = null)
    {
        var @event = new Event(
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

        _statistics.TotalEvents++;
        _logger.LogDebug("Created test event {EventId} of type {EventType} for user {UserId}",
            @event.EventId, eventType, userId);

        return @event;
    }

    /// <summary>
    /// Creates multiple test events for a user
    /// </summary>
    public List<Event> CreateEvents(
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
            events.Add(CreateEvent(userId, eventType, eventTime));
        }

        _logger.LogDebug("Created {Count} test events of type {EventType} for user {UserId}",
            count, eventType, userId);

        return events;
    }

    /// <summary>
    /// Creates test events with different types for a user
    /// </summary>
    public List<Event> CreateMixedEventTypes(
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
                events.Add(CreateEvent(userId, kvp.Key, currentTime));
                currentTime = currentTime.AddMinutes(1);
            }
        }

        _logger.LogDebug("Created {TotalCount} mixed event types for user {UserId}",
            events.Count, userId);

        return events;
    }

    /// <summary>
    /// Creates a test user state with specified parameters
    /// </summary>
    public UserState CreateUserState(
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

        _statistics.TotalUserStates++;
        _logger.LogDebug("Created test user state for user {UserId} with {PointCategories} point categories and {BadgeCount} badges",
            userId, pointsByCategory?.Count ?? 0, badges?.Count ?? 0);

        return userState;
    }

    /// <summary>
    /// Creates test data fixtures for common scenarios
    /// </summary>
    public TestDataFixture CreateFixture(string fixtureName)
    {
        if (_fixtures.ContainsKey(fixtureName))
        {
            return _fixtures[fixtureName];
        }

        var fixture = fixtureName.ToLowerInvariant() switch
        {
            "basic_user" => CreateBasicUserFixture(),
            "power_user" => CreatePowerUserFixture(),
            "new_user" => CreateNewUserFixture(),
            "event_sequence" => CreateEventSequenceFixture(),
            "multiple_users" => CreateMultipleUsersFixture(),
            _ => CreateDefaultFixture(fixtureName)
        };

        _fixtures[fixtureName] = fixture;
        _logger.LogDebug("Created test data fixture '{FixtureName}' with {EventCount} events and {UserStateCount} user states",
            fixtureName, fixture.Events.Count, fixture.UserStates.Count);

        return fixture;
    }

    /// <summary>
    /// Validates that test data meets expected constraints
    /// </summary>
    public bool ValidateTestData<T>(T data, Func<T, bool> validator)
    {
        try
        {
            var isValid = validator(data);
            _logger.LogDebug("Test data validation {Result} for type {DataType}",
                isValid ? "passed" : "failed", typeof(T).Name);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test data validation failed for type {DataType}", typeof(T).Name);
            return false;
        }
    }

    /// <summary>
    /// Cleans up test data and resets to initial state
    /// </summary>
    public async Task CleanupTestDataAsync()
    {
        try
        {
            await _testDatabase.CleanupAsync();
            _fixtures.Clear();
            ResetStatistics();
            _logger.LogInformation("Test data cleanup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup test data");
            throw;
        }
    }

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        try
        {
            await _testDatabase.SeedAsync();
            _logger.LogInformation("Test data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed test data");
            throw;
        }
    }

    /// <summary>
    /// Gets test data statistics
    /// </summary>
    public TestDataStatistics GetTestDataStatistics()
    {
        return new TestDataStatistics
        {
            TotalEvents = _statistics.TotalEvents,
            TotalUserStates = _statistics.TotalUserStates,
            UniqueEventTypes = _statistics.UniqueEventTypes,
            UniqueUsers = _statistics.UniqueUsers,
            DataTimeRange = _statistics.DataTimeRange
        };
    }

    private TestDataFixture CreateBasicUserFixture()
    {
        var fixture = new TestDataFixture { Name = "Basic User" };

        // Create a basic user with some events
        var userId = "basic-user-1";
        fixture.UserStates.Add(CreateUserState(userId, new Dictionary<string, long> { { "XP", 100 } }));
        fixture.Events.AddRange(CreateEvents(userId, 5, "PAGE_VIEW"));
        fixture.Events.Add(CreateEvent(userId, "FIRST_LOGIN"));

        return fixture;
    }

    private TestDataFixture CreatePowerUserFixture()
    {
        var fixture = new TestDataFixture { Name = "Power User" };

        // Create a power user with many events and achievements
        var userId = "power-user-1";
        fixture.UserStates.Add(CreateUserState(userId,
            new Dictionary<string, long> { { "XP", 10000 }, { "Coins", 5000 } },
            new List<string> { "Early Adopter", "Power User", "1000 Events" }));

        fixture.Events.AddRange(CreateEvents(userId, 100, "PAGE_VIEW"));
        fixture.Events.AddRange(CreateEvents(userId, 50, "COMMENT_POSTED"));
        fixture.Events.AddRange(CreateEvents(userId, 25, "PRODUCT_PURCHASED"));

        return fixture;
    }

    private TestDataFixture CreateNewUserFixture()
    {
        var fixture = new TestDataFixture { Name = "New User" };

        // Create a new user with minimal activity
        var userId = "new-user-1";
        fixture.UserStates.Add(CreateUserState(userId));
        fixture.Events.Add(CreateEvent(userId, "ACCOUNT_CREATED"));

        return fixture;
    }

    private TestDataFixture CreateEventSequenceFixture()
    {
        var fixture = new TestDataFixture { Name = "Event Sequence" };

        // Create events that form a specific sequence
        var userId = "sequence-user-1";
        fixture.UserStates.Add(CreateUserState(userId));

        var sequence = new List<(string EventType, TimeSpan Delay)>
        {
            ("PAGE_VIEW", TimeSpan.Zero),
            ("BUTTON_CLICKED", TimeSpan.FromMinutes(1)),
            ("FORM_SUBMITTED", TimeSpan.FromMinutes(2)),
            ("CONFIRMATION_RECEIVED", TimeSpan.FromMinutes(3))
        };

        fixture.Events.AddRange(CreateEventSequence(userId, sequence));

        return fixture;
    }

    private TestDataFixture CreateMultipleUsersFixture()
    {
        var fixture = new TestDataFixture { Name = "Multiple Users" };

        // Create multiple users with different activity levels
        for (int i = 1; i <= 5; i++)
        {
            var userId = $"multi-user-{i}";
            var eventCount = i * 10;
            var points = i * 100;

            fixture.UserStates.Add(CreateUserState(userId, new Dictionary<string, long> { { "XP", points } }));
            fixture.Events.AddRange(CreateEvents(userId, eventCount, "ACTIVITY"));
        }

        return fixture;
    }

    private TestDataFixture CreateDefaultFixture(string fixtureName)
    {
        var fixture = new TestDataFixture { Name = fixtureName };

        // Create a default user with basic events
        var userId = $"default-user-{Guid.NewGuid():N}";
        fixture.UserStates.Add(CreateUserState(userId));
        fixture.Events.Add(CreateEvent(userId, "DEFAULT_EVENT"));

        return fixture;
    }

    private List<Event> CreateEventSequence(string userId, List<(string EventType, TimeSpan Delay)> sequence)
    {
        var events = new List<Event>();
        var baseTime = DateTime.UtcNow;
        var currentTime = baseTime;

        foreach (var (eventType, delay) in sequence)
        {
            events.Add(CreateEvent(userId, eventType, currentTime));
            currentTime = currentTime.Add(delay);
        }

        return events;
    }

    private void ResetStatistics()
    {
        _statistics.TotalEvents = 0;
        _statistics.TotalUserStates = 0;
        _statistics.UniqueEventTypes = 0;
        _statistics.UniqueUsers = 0;
        _statistics.DataTimeRange = TimeSpan.Zero;
    }
}