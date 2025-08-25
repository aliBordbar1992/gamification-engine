using GamificationEngine.Integration.Tests.Infrastructure.Models;

namespace GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

/// <summary>
/// Interface for monitoring test execution
/// </summary>
public interface ITestExecutionMonitor
{
    /// <summary>
    /// Starts monitoring a test execution
    /// </summary>
    ITestExecutionTracker StartTestExecution(string testName, string? testCategory = null);

    /// <summary>
    /// Gets execution statistics for a specific test
    /// </summary>
    TestExecutionStats GetTestStats(string testName);

    /// <summary>
    /// Gets execution statistics for all tests
    /// </summary>
    IReadOnlyDictionary<string, TestExecutionStats> GetAllTestStats();

    /// <summary>
    /// Gets execution statistics grouped by test category
    /// </summary>
    IReadOnlyDictionary<string, TestCategoryStats> GetTestCategoryStats();

    /// <summary>
    /// Gets performance summary for all tests
    /// </summary>
    TestPerformanceSummary GetPerformanceSummary();

    /// <summary>
    /// Clears all test execution statistics
    /// </summary>
    void ClearStats();

    /// <summary>
    /// Exports test execution statistics to a report
    /// </summary>
    TestExecutionReport GenerateReport();
}