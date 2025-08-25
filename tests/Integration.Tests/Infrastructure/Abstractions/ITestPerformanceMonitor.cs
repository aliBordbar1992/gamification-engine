using GamificationEngine.Integration.Tests.Infrastructure.Models;

namespace GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

/// <summary>
/// Interface for monitoring test performance
/// </summary>
public interface ITestPerformanceMonitor
{
    /// <summary>
    /// Starts monitoring a test operation
    /// </summary>
    ITestOperationMonitor StartOperation(string operationName);

    /// <summary>
    /// Gets performance statistics for an operation
    /// </summary>
    TestPerformanceStats GetPerformanceStats(string operationName);

    /// <summary>
    /// Gets all performance statistics
    /// </summary>
    IReadOnlyDictionary<string, TestPerformanceStats> GetAllPerformanceStats();

    /// <summary>
    /// Clears all performance data
    /// </summary>
    void ClearPerformanceData();
}