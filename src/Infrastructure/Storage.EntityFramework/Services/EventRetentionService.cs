using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Infrastructure.Storage.EntityFramework.Services;

/// <summary>
/// Background service that applies event retention policies automatically
/// </summary>
public class EventRetentionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventRetentionService> _logger;
    private readonly EventRetentionOptions _options;

    public EventRetentionService(
        IServiceProvider serviceProvider,
        ILogger<EventRetentionService> logger,
        IOptions<EventRetentionOptions> options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event retention service started with retention period: {RetentionPeriod}", _options.RetentionPeriod);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ApplyRetentionPolicyAsync(stoppingToken);
                await Task.Delay(_options.Interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Event retention service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in event retention service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait before retrying
            }
        }
    }

    private async Task ApplyRetentionPolicyAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        try
        {
            // Use reflection to call the ApplyRetentionPolicyAsync method if it exists
            var method = eventRepository.GetType().GetMethod("ApplyRetentionPolicyAsync");
            if (method != null)
            {
                var result = await (Task<int>)method.Invoke(eventRepository, new object[] { _options.RetentionPeriod, cancellationToken })!;
                _logger.LogInformation("Retention policy applied successfully. Deleted {DeletedCount} events", result);
            }
            else
            {
                _logger.LogWarning("Event repository does not support retention policy");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error applying retention policy");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event retention service stopping");
        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Configuration options for event retention service
/// </summary>
public class EventRetentionOptions
{
    /// <summary>
    /// How long to keep events before applying retention policy
    /// </summary>
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(90);

    /// <summary>
    /// How often to run the retention policy check
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(24);
}