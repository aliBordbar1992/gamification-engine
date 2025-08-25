using GamificationEngine.Integration.Tests.Infrastructure.Models;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Information about the current test execution state
/// </summary>
public class TestStateInfo
{
    /// <summary>
    /// Unique identifier for the current test
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the current test method
    /// </summary>
    public string TestName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the current test class
    /// </summary>
    public string TestClassName { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the test started
    /// </summary>
    public DateTime TestStartTime { get; set; }

    /// <summary>
    /// Current test execution phase
    /// </summary>
    public TestExecutionPhase CurrentPhase { get; set; }

    /// <summary>
    /// Database connection information
    /// </summary>
    public string? DatabaseConnection { get; set; }

    /// <summary>
    /// Test data that has been created during the test
    /// </summary>
    public Dictionary<string, object> TestData { get; set; } = new();

    /// <summary>
    /// Whether the test is running in isolation mode
    /// </summary>
    public bool IsIsolated { get; set; }

    /// <summary>
    /// Test execution metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}