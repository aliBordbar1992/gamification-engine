namespace GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

/// <summary>
/// Interface for tracking individual test execution
/// </summary>
public interface ITestExecutionTracker : IDisposable
{
    /// <summary>
    /// Marks the test as successful
    /// </summary>
    void MarkSuccess();

    /// <summary>
    /// Marks the test as failed
    /// </summary>
    void MarkFailure(Exception? exception = null);
}