using GamificationEngine.Integration.Tests.Infrastructure.Models;
using GamificationEngine.Integration.Tests.Infrastructure.Testing;

namespace GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

/// <summary>
/// Interface for performance testing harness that provides load testing and performance testing utilities
/// </summary>
public interface IPerformanceTestHarness
{
    /// <summary>
    /// Runs a load test with multiple concurrent requests
    /// </summary>
    /// <param name="testAction">The test action to execute</param>
    /// <param name="options">Load test configuration options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Load test results</returns>
    Task<LoadTestResult> RunLoadTestAsync(
        Func<Task> testAction,
        LoadTestOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a stress test with gradually increasing load
    /// </summary>
    /// <param name="testAction">The test action to execute</param>
    /// <param name="options">Stress test configuration options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stress test results</returns>
    Task<StressTestResult> RunStressTestAsync(
        Func<Task> testAction,
        StressTestOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a performance baseline test to establish performance benchmarks
    /// </summary>
    /// <param name="testAction">The test action to execute</param>
    /// <param name="options">Baseline test configuration options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance baseline</returns>
    Task<PerformanceBaseline> RunBaselineTestAsync(
        Func<Task> testAction,
        BaselineTestOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares current performance against a baseline
    /// </summary>
    /// <param name="baseline">The performance baseline to compare against</param>
    /// <param name="currentResult">Current test results</param>
    /// <returns>Performance comparison results</returns>
    PerformanceComparison CompareAgainstBaseline(
        PerformanceBaseline baseline,
        LoadTestResult currentResult);
}