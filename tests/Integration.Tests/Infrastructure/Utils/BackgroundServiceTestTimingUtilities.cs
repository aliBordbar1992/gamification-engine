using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Utils;

/// <summary>
/// Enhanced timing and synchronization utilities specifically for testing background services
/// </summary>
public class BackgroundServiceTestTimingUtilities : ITestTimingUtilities
{
    private readonly ILogger<BackgroundServiceTestTimingUtilities> _logger;

    public BackgroundServiceTestTimingUtilities(ILogger<BackgroundServiceTestTimingUtilities> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Waits for a background service to reach a specific status
    /// </summary>
    public async Task<bool> WaitForServiceStatusAsync(
        Func<BackgroundServiceStatus> getStatus,
        BackgroundServiceStatus expectedStatus,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(100);
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);

        while (DateTime.UtcNow < endTime)
        {
            var currentStatus = getStatus();
            if (currentStatus == expectedStatus)
            {
                var elapsed = DateTime.UtcNow - startTime;
                _logger.LogDebug("Service reached expected status {ExpectedStatus} after {Elapsed}ms",
                    expectedStatus, elapsed.TotalMilliseconds);
                return true;
            }

            await Task.Delay(interval);
        }

        var finalStatus = getStatus();
        _logger.LogWarning("Service did not reach expected status {ExpectedStatus} within timeout {Timeout}. Final status: {FinalStatus}",
            expectedStatus, timeout, finalStatus);
        return false;
    }

    /// <summary>
    /// Waits for a background service to start processing
    /// </summary>
    public async Task<bool> WaitForServiceToStartProcessingAsync(
        Func<bool> isProcessing,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        return await WaitForConditionAsync(isProcessing, timeout, pollInterval);
    }

    /// <summary>
    /// Waits for a background service to stop processing
    /// </summary>
    public async Task<bool> WaitForServiceToStopProcessingAsync(
        Func<bool> isProcessing,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        return await WaitForConditionAsync(() => !isProcessing(), timeout, pollInterval);
    }

    /// <summary>
    /// Waits for a background service to complete processing of a specific number of items
    /// </summary>
    public async Task<bool> WaitForProcessingCompletionAsync(
        Func<long> getProcessedCount,
        long expectedCount,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(100);
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);

        while (DateTime.UtcNow < endTime)
        {
            var currentCount = getProcessedCount();
            if (currentCount >= expectedCount)
            {
                var elapsed = DateTime.UtcNow - startTime;
                _logger.LogDebug("Service processed {ExpectedCount} items after {Elapsed}ms",
                    expectedCount, elapsed.TotalMilliseconds);
                return true;
            }

            await Task.Delay(interval);
        }

        var finalCount = getProcessedCount();
        _logger.LogWarning("Service did not process {ExpectedCount} items within timeout {Timeout}. Final count: {FinalCount}",
            expectedCount, timeout, finalCount);
        return false;
    }

    /// <summary>
    /// Waits for a background service to become idle (no processing for a specified duration)
    /// </summary>
    public async Task<bool> WaitForServiceIdleAsync(
        Func<bool> isProcessing,
        TimeSpan idleDuration,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(100);
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);
        var lastProcessingTime = DateTime.UtcNow;

        while (DateTime.UtcNow < endTime)
        {
            if (isProcessing())
            {
                lastProcessingTime = DateTime.UtcNow;
            }
            else if (DateTime.UtcNow - lastProcessingTime >= idleDuration)
            {
                var elapsed = DateTime.UtcNow - startTime;
                _logger.LogDebug("Service became idle after {Elapsed}ms (idle for {IdleDuration})",
                    elapsed.TotalMilliseconds, idleDuration);
                return true;
            }

            await Task.Delay(interval);
        }

        _logger.LogWarning("Service did not become idle within timeout {Timeout}", timeout);
        return false;
    }

    /// <summary>
    /// Waits for a condition to be true with a timeout
    /// </summary>
    public async Task<bool> WaitForConditionAsync(Func<bool> condition, TimeSpan timeout, TimeSpan? pollInterval = null)
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(100);
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);

        while (DateTime.UtcNow < endTime)
        {
            if (condition())
            {
                var elapsed = DateTime.UtcNow - startTime;
                _logger.LogDebug("Condition met after {Elapsed}ms", elapsed.TotalMilliseconds);
                return true;
            }

            await Task.Delay(interval);
        }

        _logger.LogWarning("Condition not met within timeout {Timeout}", timeout);
        return false;
    }

    /// <summary>
    /// Waits for an async condition to be true with a timeout
    /// </summary>
    public async Task<bool> WaitForConditionAsync(Func<Task<bool>> condition, TimeSpan timeout, TimeSpan? pollInterval = null)
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(100);
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);

        while (DateTime.UtcNow < endTime)
        {
            if (await condition())
            {
                var elapsed = DateTime.UtcNow - startTime;
                _logger.LogDebug("Async condition met after {Elapsed}ms", elapsed.TotalMilliseconds);
                return true;
            }

            await Task.Delay(interval);
        }

        _logger.LogWarning("Async condition not met within timeout {Timeout}", timeout);
        return false;
    }

    /// <summary>
    /// Delays execution for a specified amount of time
    /// </summary>
    public async Task DelayAsync(TimeSpan delay)
    {
        _logger.LogDebug("Delaying execution for {Delay}", delay);
        await Task.Delay(delay);
    }

    /// <summary>
    /// Measures the execution time of an action
    /// </summary>
    public async Task<TimeSpan> MeasureExecutionTimeAsync(Func<Task> action)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await action();
        }
        finally
        {
            stopwatch.Stop();
        }

        var executionTime = stopwatch.Elapsed;
        _logger.LogDebug("Async action executed in {ExecutionTime}", executionTime);
        return executionTime;
    }

    /// <summary>
    /// Measures the execution time of a synchronous action
    /// </summary>
    public TimeSpan MeasureExecutionTime(Action action)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            action();
        }
        finally
        {
            stopwatch.Stop();
        }

        var executionTime = stopwatch.Elapsed;
        _logger.LogDebug("Action executed in {ExecutionTime}", executionTime);
        return executionTime;
    }

    /// <summary>
    /// Creates a test timer with configurable behavior
    /// </summary>
    public ITestTimer CreateTimer(TimeSpan timeout)
    {
        // Create a simple timer implementation
        return new SimpleTestTimer(timeout, _logger);
    }

    /// <summary>
    /// Waits for all tasks to complete with a timeout
    /// </summary>
    public async Task<bool> WaitForAllTasksAsync(IEnumerable<Task> tasks, TimeSpan timeout)
    {
        try
        {
            await Task.WhenAll(tasks).WaitAsync(timeout);
            _logger.LogDebug("All tasks completed within timeout {Timeout}", timeout);
            return true;
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Not all tasks completed within timeout {Timeout}", timeout);
            return false;
        }
    }

    /// <summary>
    /// Waits for any task to complete with a timeout
    /// </summary>
    public async Task<Task?> WaitForAnyTaskAsync(IEnumerable<Task> tasks, TimeSpan timeout)
    {
        try
        {
            var completedTask = await Task.WhenAny(tasks).WaitAsync(timeout);
            _logger.LogDebug("Task completed within timeout {Timeout}", timeout);
            return completedTask;
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("No tasks completed within timeout {Timeout}", timeout);
            return null;
        }
    }

    /// <summary>
    /// Waits for a background service to stabilize (no state changes for a specified duration)
    /// </summary>
    public async Task<bool> WaitForServiceStabilizationAsync(
        Func<object> getCurrentState,
        TimeSpan stabilizationDuration,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(100);
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);
        var lastStateChangeTime = DateTime.UtcNow;
        var lastState = getCurrentState();

        while (DateTime.UtcNow < endTime)
        {
            var currentState = getCurrentState();
            if (!currentState.Equals(lastState))
            {
                lastState = currentState;
                lastStateChangeTime = DateTime.UtcNow;
            }
            else if (DateTime.UtcNow - lastStateChangeTime >= stabilizationDuration)
            {
                var elapsed = DateTime.UtcNow - startTime;
                _logger.LogDebug("Service stabilized after {Elapsed}ms (stable for {StabilizationDuration})",
                    elapsed.TotalMilliseconds, stabilizationDuration);
                return true;
            }

            await Task.Delay(interval);
        }

        _logger.LogWarning("Service did not stabilize within timeout {Timeout}", timeout);
        return false;
    }
}