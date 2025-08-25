using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;

namespace GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

/// <summary>
/// Interface for managing test data creation, validation, and cleanup
/// </summary>
public interface ITestDataManager
{
    /// <summary>
    /// Creates test events with specified parameters
    /// </summary>
    Event CreateEvent(
        string userId = "test-user-1",
        string eventType = "TEST_EVENT",
        DateTime? timestamp = null,
        Dictionary<string, object>? attributes = null);

    /// <summary>
    /// Creates multiple test events for a user
    /// </summary>
    List<Event> CreateEvents(
        string userId,
        int count,
        string eventType = "TEST_EVENT",
        TimeSpan? interval = null);

    /// <summary>
    /// Creates test events with different types for a user
    /// </summary>
    List<Event> CreateMixedEventTypes(
        string userId,
        Dictionary<string, int> eventTypeCounts);

    /// <summary>
    /// Creates a test user state with specified parameters
    /// </summary>
    UserState CreateUserState(
        string userId = "test-user-1",
        Dictionary<string, long>? pointsByCategory = null,
        List<string>? badges = null);

    /// <summary>
    /// Creates test data fixtures for common scenarios
    /// </summary>
    TestDataFixture CreateFixture(string fixtureName);

    /// <summary>
    /// Validates that test data meets expected constraints
    /// </summary>
    bool ValidateTestData<T>(T data, Func<T, bool> validator);

    /// <summary>
    /// Cleans up test data and resets to initial state
    /// </summary>
    Task CleanupTestDataAsync();

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    Task SeedTestDataAsync();

    /// <summary>
    /// Gets test data statistics
    /// </summary>
    TestDataStatistics GetTestDataStatistics();
}

/// <summary>
/// Represents a collection of test data for a specific scenario
/// </summary>
public class TestDataFixture
{
    /// <summary>
    /// Name of the fixture
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Events in the fixture
    /// </summary>
    public List<Event> Events { get; set; } = new();

    /// <summary>
    /// User states in the fixture
    /// </summary>
    public List<UserState> UserStates { get; set; } = new();

    /// <summary>
    /// Metadata for the fixture
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Statistics about test data usage
/// </summary>
public class TestDataStatistics
{
    /// <summary>
    /// Total number of events created
    /// </summary>
    public int TotalEvents { get; set; }

    /// <summary>
    /// Total number of user states created
    /// </summary>
    public int TotalUserStates { get; set; }

    /// <summary>
    /// Number of unique event types
    /// </summary>
    public int UniqueEventTypes { get; set; }

    /// <summary>
    /// Number of unique users
    /// </summary>
    public int UniqueUsers { get; set; }

    /// <summary>
    /// Time range of test data
    /// </summary>
    public TimeSpan DataTimeRange { get; set; }
}