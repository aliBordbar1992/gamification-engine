using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of the event queue processor
/// </summary>
public class EventQueueProcessor : IEventQueueProcessor, IDisposable
{
    private readonly IEventQueue _eventQueue;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<EventQueueProcessor>? _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _processingTask;
    private volatile bool _isProcessing;
    private long _processedEventCount;

    public bool IsProcessing => _isProcessing;
    public long ProcessedEventCount => _processedEventCount;

    public EventQueueProcessor(
        IEventQueue eventQueue,
        IEventRepository eventRepository,
        ILogger<EventQueueProcessor>? logger = null)
    {
        _eventQueue = eventQueue ?? throw new ArgumentNullException(nameof(eventQueue));
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _logger = logger;
    }

    public async Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {
        if (_isProcessing)
        {
            _logger?.LogWarning("Event queue processor is already running");
            return;
        }

        _isProcessing = true;
        _cancellationTokenSource.CancelAfter(Timeout.Infinite); // Reset cancellation

        _logger?.LogInformation("Starting event queue processor");

        _processingTask = ProcessEventsAsync(cancellationToken);
        await Task.CompletedTask; // Return immediately, processing runs in background
    }

    public async Task StopProcessingAsync()
    {
        if (!_isProcessing)
        {
            _logger?.LogWarning("Event queue processor is not running");
            return;
        }

        _logger?.LogInformation("Stopping event queue processor");
        _isProcessing = false;
        _cancellationTokenSource.Cancel();

        if (_processingTask != null)
        {
            try
            {
                await _processingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
        }

        _logger?.LogInformation("Event queue processor stopped. Processed {Count} events", _processedEventCount);
    }

    private async Task ProcessEventsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_isProcessing && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var @event = await _eventQueue.DequeueAsync();

                    if (@event != null)
                    {
                        await _eventRepository.StoreAsync(@event);
                        Interlocked.Increment(ref _processedEventCount);

                        _logger?.LogDebug("Processed event {EventId} of type {EventType} for user {UserId}",
                            @event.EventId, @event.EventType, @event.UserId);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Processing was cancelled
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error processing event from queue");
                    // Continue processing other events
                }
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        if (_processingTask != null && !_processingTask.IsCompleted)
        {
            try
            {
                _processingTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch (AggregateException)
            {
                // Expected when cancelling
            }
        }
    }
}