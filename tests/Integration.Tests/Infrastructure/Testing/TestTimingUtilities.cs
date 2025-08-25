using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Provides timing and synchronization utilities for integration tests
/// </summary>
public class TestTimingUtilities : ITestTimingUtilities
{
    private readonly ILogger<TestTimingUtilities> _logger;

    public TestTimingUtilities(ILogger<TestTimingUtilities> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        return new TestTimer(timeout, _logger);
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
}

/// <summary>
/// Implementation of a test timer
/// </summary>
public class TestTimer : ITestTimer
{
    private readonly TimeSpan _timeout;
    private readonly ILogger _logger;
    private DateTime _expirationTime;
    private bool _isExpired;

    public TestTimer(TimeSpan timeout, ILogger logger)
    {
        _timeout = timeout;
        _logger = logger;
        Reset();
    }

    /// <summary>
    /// Gets the remaining time
    /// </summary>
    public TimeSpan RemainingTime
    {
        get
        {
            if (_isExpired)
                return TimeSpan.Zero;

            var remaining = _expirationTime - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Gets whether the timer has expired
    /// </summary>
    public bool HasExpired
    {
        get
        {
            if (_isExpired)
                return true;

            _isExpired = DateTime.UtcNow >= _expirationTime;
            return _isExpired;
        }
    }

    /// <summary>
    /// Waits for the timer to expire
    /// </summary>
    public async Task WaitForExpirationAsync()
    {
        if (HasExpired)
            return;

        var remaining = RemainingTime;
        if (remaining > TimeSpan.Zero)
        {
            _logger.LogDebug("Waiting for timer to expire in {RemainingTime}", remaining);
            await Task.Delay(remaining);
        }

        _isExpired = true;
        _logger.LogDebug("Timer expired");
    }

    /// <summary>
    /// Resets the timer
    /// </summary>
    public void Reset()
    {
        _expirationTime = DateTime.UtcNow.Add(_timeout);
        _isExpired = false;
        _logger.LogDebug("Timer reset with timeout {Timeout}", _timeout);
    }

    /// <summary>
    /// Extends the timer by the specified amount
    /// </summary>
    public void Extend(TimeSpan extension)
    {
        _expirationTime = _expirationTime.Add(extension);
        _isExpired = false;
        _logger.LogDebug("Timer extended by {Extension}, new expiration: {ExpirationTime}",
            extension, _expirationTime);
    }
}