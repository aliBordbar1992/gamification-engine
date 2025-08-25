using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Testing;

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

/// <summary>
/// Represents an isolated test data scope for a single test
/// </summary>
public class TestDataScope : IDisposable
{
    private readonly string _testId;
    private readonly TestDataIsolationManager _manager;
    private readonly HashSet<string> _reservedUserIds;
    private readonly HashSet<string> _reservedEventIds;
    private bool _disposed;

    public TestDataScope(string testId, TestDataIsolationManager manager)
    {
        _testId = testId ?? throw new ArgumentNullException(nameof(testId));
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _reservedUserIds = new HashSet<string>();
        _reservedEventIds = new HashSet<string>();
    }

    /// <summary>
    /// Gets the test ID for this scope
    /// </summary>
    public string TestId => _testId;

    /// <summary>
    /// Reserves a user ID for this test scope
    /// </summary>
    public bool TryReserveUserId(string userId)
    {
        if (_disposed) return false;

        if (_manager.TryReserveUserId(_testId, userId))
        {
            _reservedUserIds.Add(userId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Reserves an event ID for this test scope
    /// </summary>
    public bool TryReserveEventId(string eventId)
    {
        if (_disposed) return false;

        if (_manager.TryReserveEventId(_testId, eventId))
        {
            _reservedEventIds.Add(eventId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Generates a unique user ID for this test scope
    /// </summary>
    public string GenerateUniqueUserId()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(TestDataScope));

        var userId = _manager.GenerateUniqueUserId(_testId);
        _reservedUserIds.Add(userId);
        return userId;
    }

    /// <summary>
    /// Generates a unique event ID for this test scope
    /// </summary>
    public string GenerateUniqueEventId()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(TestDataScope));

        var eventId = _manager.GenerateUniqueEventId(_testId);
        _reservedEventIds.Add(eventId);
        return eventId;
    }

    /// <summary>
    /// Gets all reserved user IDs for this scope
    /// </summary>
    public IReadOnlyCollection<string> ReservedUserIds => _reservedUserIds;

    /// <summary>
    /// Gets all reserved event IDs for this scope
    /// </summary>
    public IReadOnlyCollection<string> ReservedEventIds => _reservedEventIds;

    /// <summary>
    /// Cleans up this test data scope
    /// </summary>
    internal void Cleanup()
    {
        if (_disposed) return;

        // Release all reservations
        foreach (var userId in _reservedUserIds)
        {
            // Note: In a real implementation, we might want to track and release these
            // For now, we'll just clear the local collections
        }

        _reservedUserIds.Clear();
        _reservedEventIds.Clear();
    }

    /// <summary>
    /// Disposes this test data scope
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        Cleanup();
        _manager.ReleaseScope(_testId);
        _disposed = true;
    }
}

/// <summary>
/// Statistics about test data isolation
/// </summary>
public class TestDataIsolationStatistics
{
    /// <summary>
    /// Number of active test scopes
    /// </summary>
    public int ActiveScopeCount { get; set; }

    /// <summary>
    /// Total number of reserved user IDs across all tests
    /// </summary>
    public int TotalUserIdReservations { get; set; }

    /// <summary>
    /// Total number of reserved event IDs across all tests
    /// </summary>
    public int TotalEventIdReservations { get; set; }

    /// <summary>
    /// List of active test IDs
    /// </summary>
    public List<string> ActiveTestIds { get; set; } = new();
}