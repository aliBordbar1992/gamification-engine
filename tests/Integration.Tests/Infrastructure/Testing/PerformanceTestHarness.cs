using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Provides load testing and performance testing utilities for API endpoints
/// </summary>
public class PerformanceTestHarness : IPerformanceTestHarness
{
    private readonly ITestMetricsCollector _metricsCollector;
    private readonly ITestPerformanceMonitor _performanceMonitor;
    private readonly ITestTimingUtilities _timingUtilities;
    private readonly ILogger<PerformanceTestHarness> _logger;

    public PerformanceTestHarness(
        ITestMetricsCollector metricsCollector,
        ITestPerformanceMonitor performanceMonitor,
        ITestTimingUtilities timingUtilities,
        ILogger<PerformanceTestHarness> logger)
    {
        _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
        _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
        _timingUtilities = timingUtilities ?? throw new ArgumentNullException(nameof(timingUtilities));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs a load test with multiple concurrent requests
    /// </summary>
    public async Task<LoadTestResult> RunLoadTestAsync(
        Func<Task> testAction,
        LoadTestOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting load test with {Concurrency} concurrent users for {Duration}",
            options.Concurrency, options.Duration);

        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(options.Duration);
        var results = new List<RequestResult>();
        var semaphore = new SemaphoreSlim(options.Concurrency, options.Concurrency);

        var tasks = new List<Task>();
        var requestId = 0;

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            if (semaphore.Wait(0)) // Try to acquire without blocking
            {
                var currentRequestId = Interlocked.Increment(ref requestId);
                var task = RunSingleRequestAsync(testAction, currentRequestId, semaphore, results);
                tasks.Add(task);
            }
            else
            {
                // Wait a bit before trying again
                await Task.Delay(10, cancellationToken);
            }
        }

        // Wait for all running tasks to complete
        if (tasks.Any())
        {
            await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromMinutes(5), cancellationToken);
        }

        var totalDuration = DateTime.UtcNow - startTime;
        var loadTestResult = new LoadTestResult(results, totalDuration, options);

        _logger.LogInformation("Load test completed. Total requests: {TotalRequests}, Success rate: {SuccessRate:P2}, " +
                             "Average response time: {AverageResponseTime:F2}ms",
            loadTestResult.TotalRequests, loadTestResult.SuccessRate, loadTestResult.AverageResponseTime.TotalMilliseconds);

        return loadTestResult;
    }

    /// <summary>
    /// Runs a stress test with gradually increasing load
    /// </summary>
    public async Task<StressTestResult> RunStressTestAsync(
        Func<Task> testAction,
        StressTestOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting stress test with ramp-up from {InitialConcurrency} to {MaxConcurrency} users",
            options.InitialConcurrency, options.MaxConcurrency);

        var results = new List<LoadTestResult>();
        var currentConcurrency = options.InitialConcurrency;
        var startTime = DateTime.UtcNow;

        while (currentConcurrency <= options.MaxConcurrency && !cancellationToken.IsCancellationRequested)
        {
            var loadTestOptions = new LoadTestOptions
            {
                Concurrency = currentConcurrency,
                Duration = options.StepDuration,
                RequestTimeout = options.RequestTimeout
            };

            var loadTestResult = await RunLoadTestAsync(testAction, loadTestOptions, cancellationToken);
            results.Add(loadTestResult);

            // Check if we've hit the breaking point
            if (loadTestResult.SuccessRate < options.SuccessRateThreshold)
            {
                _logger.LogWarning("Breaking point reached at {Concurrency} concurrent users. Success rate: {SuccessRate:P2}",
                    currentConcurrency, loadTestResult.SuccessRate);
                break;
            }

            currentConcurrency += options.ConcurrencyStep;
            await Task.Delay(options.StepDelay, cancellationToken);
        }

        var totalDuration = DateTime.UtcNow - startTime;
        var stressTestResult = new StressTestResult(results, totalDuration, options);

        _logger.LogInformation("Stress test completed. Breaking point: {BreakingPoint} users, Total duration: {TotalDuration}",
            stressTestResult.BreakingPoint, stressTestResult.TotalDuration);

        return stressTestResult;
    }

    /// <summary>
    /// Runs a performance baseline test to establish performance benchmarks
    /// </summary>
    public async Task<PerformanceBaseline> RunBaselineTestAsync(
        Func<Task> testAction,
        BaselineTestOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Running performance baseline test with {Iterations} iterations",
            options.Iterations);

        var results = new List<TimeSpan>();
        var successCount = 0;

        for (int i = 0; i < options.Iterations && !cancellationToken.IsCancellationRequested; i++)
        {
            try
            {
                var executionTime = await _timingUtilities.MeasureExecutionTimeAsync(testAction);
                results.Add(executionTime);
                successCount++;

                if (options.EnableProgressLogging && (i + 1) % 10 == 0)
                {
                    _logger.LogDebug("Baseline test progress: {Completed}/{Total} iterations completed",
                        i + 1, options.Iterations);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Baseline test iteration {Iteration} failed", i + 1);
            }

            if (options.DelayBetweenIterations > TimeSpan.Zero)
            {
                await Task.Delay(options.DelayBetweenIterations, cancellationToken);
            }
        }

        var baseline = new PerformanceBaseline(results, successCount, options.Iterations);

        _logger.LogInformation("Baseline test completed. Success rate: {SuccessRate:P2}, " +
                             "Average execution time: {AverageExecutionTime:F2}ms, " +
                             "95th percentile: {P95ExecutionTime:F2}ms",
            baseline.SuccessRate, baseline.AverageExecutionTime.TotalMilliseconds,
            baseline.Percentile95.TotalMilliseconds);

        return baseline;
    }

    /// <summary>
    /// Compares current performance against a baseline
    /// </summary>
    public PerformanceComparison CompareAgainstBaseline(
        PerformanceBaseline baseline,
        LoadTestResult currentResult)
    {
        var comparison = new PerformanceComparison
        {
            Baseline = baseline,
            CurrentResult = currentResult,
            ResponseTimeChange = currentResult.AverageResponseTime - baseline.AverageExecutionTime,
            ResponseTimeChangePercentage = baseline.AverageExecutionTime > TimeSpan.Zero
                ? (currentResult.AverageResponseTime - baseline.AverageExecutionTime) / baseline.AverageExecutionTime * 100
                : 0,
            SuccessRateChange = currentResult.SuccessRate - baseline.SuccessRate,
            IsWithinTolerance = IsWithinTolerance(baseline, currentResult)
        };

        _logger.LogInformation("Performance comparison: Response time change: {Change:F2}ms ({ChangePercent:F1}%), " +
                             "Success rate change: {SuccessRateChange:P2}, Within tolerance: {WithinTolerance}",
            comparison.ResponseTimeChange.TotalMilliseconds, comparison.ResponseTimeChangePercentage,
            comparison.SuccessRateChange, comparison.IsWithinTolerance);

        return comparison;
    }

    private async Task RunSingleRequestAsync(
        Func<Task> testAction,
        int requestId,
        SemaphoreSlim semaphore,
        List<RequestResult> results)
    {
        var startTime = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var success = false;
        Exception? exception = null;

        try
        {
            await testAction();
            success = true;
        }
        catch (Exception ex)
        {
            exception = ex;
            success = false;
        }
        finally
        {
            stopwatch.Stop();
            semaphore.Release();

            var result = new RequestResult
            {
                RequestId = requestId,
                StartTime = startTime,
                Duration = stopwatch.Elapsed,
                Success = success,
                Exception = exception
            };

            lock (results)
            {
                results.Add(result);
            }

            // Record metrics
            _metricsCollector.RecordMetric("request_duration", result.Duration.TotalMilliseconds, "ms",
                new Dictionary<string, object> { ["success"] = success, ["request_id"] = requestId });

            if (success)
            {
                _metricsCollector.IncrementCounter("successful_requests");
            }
            else
            {
                _metricsCollector.IncrementCounter("failed_requests");
            }
        }
    }

    private bool IsWithinTolerance(PerformanceBaseline baseline, LoadTestResult currentResult)
    {
        var responseTimeTolerance = baseline.AverageExecutionTime * 0.2; // 20% tolerance
        var successRateTolerance = 0.05; // 5% tolerance

        var responseTimeWithinTolerance = Math.Abs(
            currentResult.AverageResponseTime.TotalMilliseconds - baseline.AverageExecutionTime.TotalMilliseconds) <= responseTimeTolerance.TotalMilliseconds;

        var successRateWithinTolerance = Math.Abs(currentResult.SuccessRate - baseline.SuccessRate) <= successRateTolerance;

        return responseTimeWithinTolerance && successRateWithinTolerance;
    }
}