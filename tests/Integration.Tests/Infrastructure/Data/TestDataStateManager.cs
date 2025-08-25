using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

namespace GamificationEngine.Integration.Tests.Infrastructure.Data;

/// <summary>
/// Manages test database state, including resets, snapshots, and cleanup operations
/// </summary>
public class TestDataStateManager
{
    private readonly ITestDatabase _testDatabase;
    private readonly ILogger<TestDataStateManager> _logger;
    private readonly Dictionary<string, DatabaseSnapshot> _snapshots;
    private readonly object _snapshotLock = new object();

    public TestDataStateManager(ITestDatabase testDatabase, ILogger<TestDataStateManager> logger)
    {
        _testDatabase = testDatabase ?? throw new ArgumentNullException(nameof(testDatabase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _snapshots = new Dictionary<string, DatabaseSnapshot>();
    }

    /// <summary>
    /// Creates a snapshot of the current database state
    /// </summary>
    public async Task<DatabaseSnapshot> CreateSnapshotAsync(string snapshotName)
    {
        if (string.IsNullOrWhiteSpace(snapshotName))
            throw new ArgumentException("Snapshot name cannot be null or empty", nameof(snapshotName));

        try
        {
            var snapshot = await CaptureDatabaseStateAsync(snapshotName);

            lock (_snapshotLock)
            {
                _snapshots[snapshotName] = snapshot;
            }

            _logger.LogDebug("Created database snapshot '{SnapshotName}' with {EventCount} events and {UserStateCount} user states",
                snapshotName, snapshot.EventCount, snapshot.UserStateCount);

            return snapshot;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database snapshot '{SnapshotName}'", snapshotName);
            throw;
        }
    }

    /// <summary>
    /// Restores the database to a previously created snapshot
    /// </summary>
    public async Task RestoreSnapshotAsync(string snapshotName)
    {
        if (string.IsNullOrWhiteSpace(snapshotName))
            throw new ArgumentException("Snapshot name cannot be null or empty", nameof(snapshotName));

        DatabaseSnapshot? snapshot;
        lock (_snapshotLock)
        {
            if (!_snapshots.TryGetValue(snapshotName, out snapshot))
            {
                throw new InvalidOperationException($"Snapshot '{snapshotName}' not found");
            }
        }

        try
        {
            await RestoreDatabaseStateAsync(snapshot);
            _logger.LogDebug("Restored database to snapshot '{SnapshotName}'", snapshotName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore database to snapshot '{SnapshotName}'", snapshotName);
            throw;
        }
    }

    /// <summary>
    /// Resets the database to a clean state
    /// </summary>
    public async Task ResetToCleanStateAsync()
    {
        try
        {
            await _testDatabase.CleanupAsync();
            _logger.LogDebug("Reset database to clean state");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset database to clean state");
            throw;
        }
    }

    /// <summary>
    /// Resets the database to a known baseline state
    /// </summary>
    public async Task ResetToBaselineAsync()
    {
        try
        {
            await _testDatabase.CleanupAsync();
            await _testDatabase.SeedAsync();
            _logger.LogDebug("Reset database to baseline state");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset database to baseline state");
            throw;
        }
    }

    /// <summary>
    /// Resets the database to a specific fixture state
    /// </summary>
    public async Task ResetToFixtureAsync(TestDataFixture fixture)
    {
        if (fixture == null)
            throw new ArgumentNullException(nameof(fixture));

        try
        {
            await _testDatabase.CleanupAsync();
            await SeedFixtureAsync(fixture);
            _logger.LogDebug("Reset database to fixture '{FixtureName}'", fixture.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset database to fixture '{FixtureName}'", fixture.Name);
            throw;
        }
    }

    /// <summary>
    /// Gets a list of available snapshots
    /// </summary>
    public IEnumerable<string> GetAvailableSnapshots()
    {
        lock (_snapshotLock)
        {
            return _snapshots.Keys.ToList();
        }
    }

    /// <summary>
    /// Deletes a specific snapshot
    /// </summary>
    public void DeleteSnapshot(string snapshotName)
    {
        if (string.IsNullOrWhiteSpace(snapshotName)) return;

        lock (_snapshotLock)
        {
            if (_snapshots.Remove(snapshotName))
            {
                _logger.LogDebug("Deleted snapshot '{SnapshotName}'", snapshotName);
            }
        }
    }

    /// <summary>
    /// Cleans up all snapshots
    /// </summary>
    public void CleanupAllSnapshots()
    {
        lock (_snapshotLock)
        {
            _snapshots.Clear();
            _logger.LogDebug("Cleaned up all snapshots");
        }
    }

    /// <summary>
    /// Gets the current database state information
    /// </summary>
    public async Task<DatabaseStateInfo> GetCurrentStateAsync()
    {
        try
        {
            var context = _testDatabase.Context;
            var eventCount = await context.Events.CountAsync();
            var userStateCount = await context.UserStates.CountAsync();

            return new DatabaseStateInfo
            {
                EventCount = eventCount,
                UserStateCount = userStateCount,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current database state");
            throw;
        }
    }

    /// <summary>
    /// Verifies that the database is in the expected state
    /// </summary>
    public async Task<bool> VerifyStateAsync(DatabaseStateInfo expectedState)
    {
        if (expectedState == null) return false;

        try
        {
            var currentState = await GetCurrentStateAsync();

            var isCorrect = currentState.EventCount == expectedState.EventCount &&
                           currentState.UserStateCount == expectedState.UserStateCount;

            if (!isCorrect)
            {
                _logger.LogWarning("Database state verification failed. Expected: {ExpectedEvents} events, {ExpectedUsers} users. Actual: {ActualEvents} events, {ActualUsers} users",
                    expectedState.EventCount, expectedState.UserStateCount, currentState.EventCount, currentState.UserStateCount);
            }

            return isCorrect;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify database state");
            return false;
        }
    }

    /// <summary>
    /// Captures the current database state for snapshot creation
    /// </summary>
    private async Task<DatabaseSnapshot> CaptureDatabaseStateAsync(string snapshotName)
    {
        var context = _testDatabase.Context;
        var events = await context.Events.ToListAsync();
        var userStates = await context.UserStates.ToListAsync();

        return new DatabaseSnapshot
        {
            Name = snapshotName,
            EventCount = events.Count,
            UserStateCount = userStates.Count,
            Timestamp = DateTimeOffset.UtcNow,
            Events = events,
            UserStates = userStates
        };
    }

    /// <summary>
    /// Restores the database to a specific snapshot state
    /// </summary>
    private async Task RestoreDatabaseStateAsync(DatabaseSnapshot snapshot)
    {
        var context = _testDatabase.Context;

        // Clear current data
        context.Events.RemoveRange(context.Events);
        context.UserStates.RemoveRange(context.UserStates);

        // Restore snapshot data
        if (snapshot.Events?.Any() == true)
        {
            context.Events.AddRange(snapshot.Events);
        }

        if (snapshot.UserStates?.Any() == true)
        {
            context.UserStates.AddRange(snapshot.UserStates);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the database with fixture data
    /// </summary>
    private async Task SeedFixtureAsync(TestDataFixture fixture)
    {
        var context = _testDatabase.Context;

        if (fixture.UserStates?.Any() == true)
        {
            context.UserStates.AddRange(fixture.UserStates);
        }

        if (fixture.Events?.Any() == true)
        {
            context.Events.AddRange(fixture.Events);
        }

        await context.SaveChangesAsync();
    }
}

/// <summary>
/// Represents a snapshot of the database state at a specific point in time
/// </summary>
public class DatabaseSnapshot
{
    /// <summary>
    /// Name of the snapshot
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of events in the snapshot
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// Number of user states in the snapshot
    /// </summary>
    public int UserStateCount { get; set; }

    /// <summary>
    /// When the snapshot was created
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// The actual events data (optional, for full state restoration)
    /// </summary>
    public List<Domain.Events.Event>? Events { get; set; }

    /// <summary>
    /// The actual user states data (optional, for full state restoration)
    /// </summary>
    public List<Domain.Users.UserState>? UserStates { get; set; }
}

/// <summary>
/// Information about the current database state
/// </summary>
public class DatabaseStateInfo
{
    /// <summary>
    /// Number of events in the database
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// Number of user states in the database
    /// </summary>
    public int UserStateCount { get; set; }

    /// <summary>
    /// When this state info was captured
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}