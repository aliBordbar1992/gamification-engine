using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Api;

/// <summary>
/// Background service for processing events from the event queue
/// </summary>
public class EventQueueBackgroundService : BackgroundService
{
    private readonly IEventQueue _eventQueue;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<EventQueueBackgroundService> _logger;
    private long _processedEventCount;

    public EventQueueBackgroundService(
        IEventQueue eventQueue,
        IEventRepository eventRepository,
        ILogger<EventQueueBackgroundService> logger)
    {
        _eventQueue = eventQueue ?? throw new ArgumentNullException(nameof(eventQueue));
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public long ProcessedEventCount => _processedEventCount;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event queue background service started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var @event = await _eventQueue.DequeueAsync(stoppingToken);

                    if (@event != null)
                    {
                        await _eventRepository.StoreAsync(@event);
                        Interlocked.Increment(ref _processedEventCount);

                        _logger.LogDebug("Processed event {EventId} of type {EventType} for user {UserId}",
                            @event.EventId, @event.EventType, @event.UserId);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Service is stopping, break out of the loop
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event from queue");
                    // Continue processing other events
                }
            }
        }
        finally
        {
            _logger.LogInformation("Event queue background service stopped. Processed {Count} events", _processedEventCount);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping event queue background service");
        await base.StopAsync(cancellationToken);
    }
}