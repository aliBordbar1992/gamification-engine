namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Represents the current phase of test execution
/// </summary>
public enum TestExecutionPhase
{
    /// <summary>
    /// Test has not started yet
    /// </summary>
    NotStarted,

    /// <summary>
    /// Test setup is in progress
    /// </summary>
    SettingUp,

    /// <summary>
    /// Test is running
    /// </summary>
    Running,

    /// <summary>
    /// Test teardown is in progress
    /// </summary>
    TearingDown,

    /// <summary>
    /// Test has completed
    /// </summary>
    Completed,

    /// <summary>
    /// Test has failed
    /// </summary>
    Failed
}