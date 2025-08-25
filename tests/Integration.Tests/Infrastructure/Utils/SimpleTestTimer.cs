using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Utils;

/// <summary>
/// Simple implementation of a test timer for background service testing
/// </summary>
public class SimpleTestTimer : ITestTimer
{
    private readonly TimeSpan _timeout;
    private readonly ILogger _logger;
    private DateTime _expirationTime;
    private bool _isExpired;

    public SimpleTestTimer(TimeSpan timeout, ILogger logger)
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