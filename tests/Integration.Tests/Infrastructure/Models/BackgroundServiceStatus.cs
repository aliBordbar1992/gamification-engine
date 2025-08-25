namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Status of a background service during testing
/// </summary>
public enum BackgroundServiceStatus
{
    /// <summary>
    /// Service is not running
    /// </summary>
    Stopped,

    /// <summary>
    /// Service is starting
    /// </summary>
    Starting,

    /// <summary>
    /// Service is running
    /// </summary>
    Running,

    /// <summary>
    /// Service is stopping
    /// </summary>
    Stopping,

    /// <summary>
    /// Service has encountered an error
    /// </summary>
    Error
}