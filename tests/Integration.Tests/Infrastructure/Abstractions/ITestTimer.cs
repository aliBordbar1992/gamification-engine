namespace GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

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