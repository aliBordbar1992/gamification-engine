using Microsoft.Extensions.Hosting;

namespace GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

/// <summary>
/// Interface for testing background services in integration tests
/// </summary>
public interface ITestBackgroundService
{
    /// <summary>
    /// Gets whether the background service is currently running
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the current status of the background service
    /// </summary>
    BackgroundServiceStatus Status { get; }

    /// <summary>
    /// Starts the background service for testing
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the background service for testing
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for the background service to start
    /// </summary>
    Task WaitForStartAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for the background service to stop
    /// </summary>
    Task WaitForStopAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the underlying background service instance
    /// </summary>
    IHostedService Service { get; }

    /// <summary>
    /// Gets the service type
    /// </summary>
    Type ServiceType { get; }
}

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