namespace GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

/// <summary>
/// Interface for monitoring individual test operations
/// </summary>
public interface ITestOperationMonitor : IDisposable
{
    /// <summary>
    /// Marks the operation as successful
    /// </summary>
    void MarkSuccess();

    /// <summary>
    /// Marks the operation as failed
    /// </summary>
    void MarkFailure();
}