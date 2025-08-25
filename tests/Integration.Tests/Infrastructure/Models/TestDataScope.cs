using GamificationEngine.Integration.Tests.Infrastructure.Data;

namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

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