using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Base test harness for testing background services in integration tests
/// </summary>
/// <typeparam name="TService">The type of background service to test</typeparam>
public abstract class BackgroundServiceTestHarness<TService> : ITestBackgroundService, IAsyncDisposable
    where TService : class, IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundServiceTestHarness<TService>> _logger;
    private TService? _service;
    private BackgroundServiceStatus _status;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly object _statusLock = new object();

    protected BackgroundServiceTestHarness(IServiceProvider serviceProvider, ILogger<BackgroundServiceTestHarness<TService>> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cancellationTokenSource = new CancellationTokenSource();
        _status = BackgroundServiceStatus.Stopped;
    }

    /// <summary>
    /// Gets whether the background service is currently running
    /// </summary>
    public bool IsRunning => Status == BackgroundServiceStatus.Running;

    /// <summary>
    /// Gets the current status of the background service
    /// </summary>
    public BackgroundServiceStatus Status
    {
        get
        {
            lock (_statusLock)
            {
                return _status;
            }
        }
        private set
        {
            lock (_statusLock)
            {
                _status = value;
            }
        }
    }

    /// <summary>
    /// Gets the underlying background service instance
    /// </summary>
    public IHostedService Service => _service ?? throw new InvalidOperationException("Service not initialized. Call StartAsync first.");

    /// <summary>
    /// Gets the service type
    /// </summary>
    public Type ServiceType => typeof(TService);

    /// <summary>
    /// Gets the typed service instance
    /// </summary>
    protected TService TypedService => _service ?? throw new InvalidOperationException("Service not initialized. Call StartAsync first.");

    /// <summary>
    /// Starts the background service for testing
    /// </summary>
    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_service != null)
        {
            _logger.LogWarning("Service is already running");
            return;
        }

        try
        {
            _logger.LogInformation("Starting background service {ServiceType}", typeof(TService).Name);
            Status = BackgroundServiceStatus.Starting;

            // Get the service from the service provider
            _service = _serviceProvider.GetRequiredService<TService>();

            // Start the service
            await _service.StartAsync(cancellationToken);

            Status = BackgroundServiceStatus.Running;
            _logger.LogInformation("Background service {ServiceType} started successfully", typeof(TService).Name);
        }
        catch (Exception ex)
        {
            Status = BackgroundServiceStatus.Error;
            _logger.LogError(ex, "Failed to start background service {ServiceType}", typeof(TService).Name);
            throw;
        }
    }

    /// <summary>
    /// Stops the background service for testing
    /// </summary>
    public virtual async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_service == null)
        {
            _logger.LogWarning("Service is not running");
            return;
        }

        try
        {
            _logger.LogInformation("Stopping background service {ServiceType}", typeof(TService).Name);
            Status = BackgroundServiceStatus.Stopping;

            // Stop the service
            await _service.StopAsync(cancellationToken);

            Status = BackgroundServiceStatus.Stopped;
            _service = null;
            _logger.LogInformation("Background service {ServiceType} stopped successfully", typeof(TService).Name);
        }
        catch (Exception ex)
        {
            Status = BackgroundServiceStatus.Error;
            _logger.LogError(ex, "Failed to stop background service {ServiceType}", typeof(TService).Name);
            throw;
        }
    }

    /// <summary>
    /// Waits for the background service to start
    /// </summary>
    public virtual async Task WaitForStartAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            if (Status == BackgroundServiceStatus.Running)
            {
                _logger.LogDebug("Background service started after {Elapsed}ms",
                    (DateTime.UtcNow - startTime).TotalMilliseconds);
                return;
            }

            if (Status == BackgroundServiceStatus.Error)
            {
                throw new InvalidOperationException($"Background service failed to start: {Status}");
            }

            await Task.Delay(50, cancellationToken);
        }

        throw new TimeoutException($"Background service did not start within {timeout}");
    }

    /// <summary>
    /// Waits for the background service to stop
    /// </summary>
    public virtual async Task WaitForStopAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            if (Status == BackgroundServiceStatus.Stopped)
            {
                _logger.LogDebug("Background service stopped after {Elapsed}ms",
                    (DateTime.UtcNow - startTime).TotalMilliseconds);
                return;
            }

            if (Status == BackgroundServiceStatus.Error)
            {
                throw new InvalidOperationException($"Background service failed to stop: {Status}");
            }

            await Task.Delay(50, cancellationToken);
        }

        throw new TimeoutException($"Background service did not stop within {timeout}");
    }

    /// <summary>
    /// Waits for a specific condition to be met by the background service
    /// </summary>
    protected async Task<bool> WaitForConditionAsync(Func<TService, bool> condition, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            if (_service != null && condition(_service))
            {
                _logger.LogDebug("Condition met after {Elapsed}ms",
                    (DateTime.UtcNow - startTime).TotalMilliseconds);
                return true;
            }

            await Task.Delay(50, cancellationToken);
        }

        _logger.LogWarning("Condition not met within timeout {Timeout}", timeout);
        return false;
    }

    /// <summary>
    /// Waits for an async condition to be met by the background service
    /// </summary>
    protected async Task<bool> WaitForConditionAsync(Func<TService, Task<bool>> condition, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            if (_service != null && await condition(_service))
            {
                _logger.LogDebug("Async condition met after {Elapsed}ms",
                    (DateTime.UtcNow - startTime).TotalMilliseconds);
                return true;
            }

            await Task.Delay(50, cancellationToken);
        }

        _logger.LogWarning("Async condition not met within timeout {Timeout}", timeout);
        return false;
    }

    /// <summary>
    /// Performs cleanup operations
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        try
        {
            if (_service != null)
            {
                await StopAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup of background service test harness");
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
        }

        await ValueTask.CompletedTask;
    }
}