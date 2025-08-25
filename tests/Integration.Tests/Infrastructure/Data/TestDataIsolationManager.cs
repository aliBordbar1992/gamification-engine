using GamificationEngine.Integration.Tests.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace GamificationEngine.Integration.Tests.Infrastructure.Data;

/// <summary>
/// Manages test data isolation to prevent test interference during parallel execution
/// </summary>
public class TestDataIsolationManager
{
    private readonly ILogger<TestDataIsolationManager> _logger;
    private readonly ConcurrentDictionary<string, TestDataScope> _activeScopes;
    private readonly ConcurrentDictionary<string, HashSet<string>> _userIdReservations;
    private readonly ConcurrentDictionary<string, HashSet<string>> _eventIdReservations;

    public TestDataIsolationManager(ILogger<TestDataIsolationManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activeScopes = new ConcurrentDictionary<string, TestDataScope>();
        _userIdReservations = new ConcurrentDictionary<string, HashSet<string>>();
        _eventIdReservations = new ConcurrentDictionary<string, HashSet<string>>();
    }

    /// <summary>
    /// Creates an isolated test data scope for a test
    /// </summary>
    public TestDataScope CreateIsolatedScope(string testId)
    {
        if (string.IsNullOrWhiteSpace(testId))
            throw new ArgumentException("Test ID cannot be null or empty", nameof(testId));

        var scope = new TestDataScope(testId, this);
        _activeScopes.TryAdd(testId, scope);

        _logger.LogDebug("Created isolated test data scope for test {TestId}", testId);
        return scope;
    }

    /// <summary>
    /// Releases an isolated test data scope
    /// </summary>
    public void ReleaseScope(string testId)
    {
        if (string.IsNullOrWhiteSpace(testId)) return;

        if (_activeScopes.TryRemove(testId, out var scope))
        {
            scope.Cleanup();
            _logger.LogDebug("Released isolated test data scope for test {TestId}", testId);
        }
    }

    /// <summary>
    /// Reserves a user ID for exclusive use by a test
    /// </summary>
    public bool TryReserveUserId(string testId, string userId)
    {
        if (string.IsNullOrWhiteSpace(testId) || string.IsNullOrWhiteSpace(userId))
            return false;

        var reservations = _userIdReservations.GetOrAdd(testId, _ => new HashSet<string>());

        lock (reservations)
        {
            // Check if user ID is already reserved by another test
            foreach (var otherTestId in _userIdReservations.Keys)
            {
                if (otherTestId != testId && _userIdReservations[otherTestId].Contains(userId))
                {
                    return false;
                }
            }

            reservations.Add(userId);
            _logger.LogDebug("Reserved user ID {UserId} for test {TestId}", userId, testId);
            return true;
        }
    }

    /// <summary>
    /// Reserves an event ID for exclusive use by a test
    /// </summary>
    public bool TryReserveEventId(string testId, string eventId)
    {
        if (string.IsNullOrWhiteSpace(testId) || string.IsNullOrWhiteSpace(eventId))
            return false;

        var reservations = _eventIdReservations.GetOrAdd(testId, _ => new HashSet<string>());

        lock (reservations)
        {
            // Check if event ID is already reserved by another test
            foreach (var otherTestId in _eventIdReservations.Keys)
            {
                if (otherTestId != testId && _eventIdReservations[otherTestId].Contains(eventId))
                {
                    return false;
                }
            }

            reservations.Add(eventId);
            _logger.LogDebug("Reserved event ID {EventId} for test {TestId}", eventId, testId);
            return true;
        }
    }

    /// <summary>
    /// Generates a unique user ID for a test
    /// </summary>
    public string GenerateUniqueUserId(string testId)
    {
        var baseUserId = $"test-user-{testId}";
        var counter = 0;
        var userId = baseUserId;

        while (!TryReserveUserId(testId, userId))
        {
            counter++;
            userId = $"{baseUserId}-{counter}";
        }

        return userId;
    }

    /// <summary>
    /// Generates a unique event ID for a test
    /// </summary>
    public string GenerateUniqueEventId(string testId)
    {
        var baseEventId = $"test-event-{testId}";
        var counter = 0;
        var eventId = baseEventId;

        while (!TryReserveEventId(testId, eventId))
        {
            counter++;
            eventId = $"{baseEventId}-{counter}";
        }

        return eventId;
    }

    /// <summary>
    /// Gets all active test scopes
    /// </summary>
    public IEnumerable<string> GetActiveTestIds()
    {
        return _activeScopes.Keys.ToList();
    }

    /// <summary>
    /// Gets the count of active test scopes
    /// </summary>
    public int GetActiveScopeCount()
    {
        return _activeScopes.Count;
    }

    /// <summary>
    /// Cleans up all test data scopes (useful for cleanup after test runs)
    /// </summary>
    public void CleanupAllScopes()
    {
        var testIds = _activeScopes.Keys.ToList();
        foreach (var testId in testIds)
        {
            ReleaseScope(testId);
        }

        _userIdReservations.Clear();
        _eventIdReservations.Clear();

        _logger.LogInformation("Cleaned up all test data scopes");
    }

    /// <summary>
    /// Gets isolation statistics for monitoring
    /// </summary>
    public TestDataIsolationStatistics GetIsolationStatistics()
    {
        return new TestDataIsolationStatistics
        {
            ActiveScopeCount = _activeScopes.Count,
            TotalUserIdReservations = _userIdReservations.Values.Sum(h => h.Count),
            TotalEventIdReservations = _eventIdReservations.Values.Sum(h => h.Count),
            ActiveTestIds = _activeScopes.Keys.ToList()
        };
    }
}