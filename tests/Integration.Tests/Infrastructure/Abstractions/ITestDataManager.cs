using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;
using GamificationEngine.Integration.Tests.Infrastructure.Models;

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