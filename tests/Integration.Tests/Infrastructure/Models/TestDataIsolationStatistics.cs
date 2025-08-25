namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

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