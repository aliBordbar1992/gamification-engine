namespace GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

/// <summary>
/// Interface for test timing and synchronization utilities
/// </summary>
public interface ITestTimingUtilities
{
    /// <summary>
    /// Waits for a condition to be true with a timeout
    /// </summary>
    Task<bool> WaitForConditionAsync(Func<bool> condition, TimeSpan timeout, TimeSpan? pollInterval = null);

    /// <summary>
    /// Waits for an async condition to be true with a timeout
    /// </summary>
    Task<bool> WaitForConditionAsync(Func<Task<bool>> condition, TimeSpan timeout, TimeSpan? pollInterval = null);

    /// <summary>
    /// Delays execution for a specified amount of time
    /// </summary>
    Task DelayAsync(TimeSpan delay);

    /// <summary>
    /// Measures the execution time of an action
    /// </summary>
    Task<TimeSpan> MeasureExecutionTimeAsync(Func<Task> action);

    /// <summary>
    /// Measures the execution time of a synchronous action
    /// </summary>
    TimeSpan MeasureExecutionTime(Action action);

    /// <summary>
    /// Creates a test timer with configurable behavior
    /// </summary>
    ITestTimer CreateTimer(TimeSpan timeout);

    /// <summary>
    /// Waits for all tasks to complete with a timeout
    /// </summary>
    Task<bool> WaitForAllTasksAsync(IEnumerable<Task> tasks, TimeSpan timeout);

    /// <summary>
    /// Waits for any task to complete with a timeout
    /// </summary>
    Task<Task?> WaitForAnyTaskAsync(IEnumerable<Task> tasks, TimeSpan timeout);
}

/// <summary>
/// Interface for test timers
/// </summary>
public interface ITestTimer
{
    /// <summary>
    /// Gets the remaining time
    /// </summary>
    TimeSpan RemainingTime { get; }

    /// <summary>
    /// Gets whether the timer has expired
    /// </summary>
    bool HasExpired { get; }

    /// <summary>
    /// Waits for the timer to expire
    /// </summary>
    Task WaitForExpirationAsync();

    /// <summary>
    /// Resets the timer
    /// </summary>
    void Reset();

    /// <summary>
    /// Extends the timer by the specified amount
    /// </summary>
    void Extend(TimeSpan extension);
}