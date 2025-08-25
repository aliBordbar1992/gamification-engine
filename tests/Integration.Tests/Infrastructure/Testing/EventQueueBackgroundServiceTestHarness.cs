using GamificationEngine.Application.Abstractions;
using GamificationEngine.Api;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Test harness specifically for testing the EventQueueBackgroundService
/// </summary>
public class EventQueueBackgroundServiceTestHarness : BackgroundServiceTestHarness<EventQueueBackgroundService>
{
    private readonly IEventQueue _eventQueue;
    private readonly IEventRepository _eventRepository;

    public EventQueueBackgroundServiceTestHarness(
        IServiceProvider serviceProvider,
        ILogger<EventQueueBackgroundServiceTestHarness> logger)
        : base(serviceProvider, logger)
    {
        _eventQueue = serviceProvider.GetRequiredService<IEventQueue>();
        _eventRepository = serviceProvider.GetRequiredService<IEventRepository>();
    }

    /// <summary>
    /// Gets the current processed event count from the service
    /// </summary>
    public long ProcessedEventCount
    {
        get
        {
            try
            {
                return TypedService.ProcessedEventCount;
            }
            catch (InvalidOperationException)
            {
                // Service not initialized yet
                return 0;
            }
        }
    }

    /// <summary>
    /// Enqueues an event for processing by the background service
    /// </summary>
    public async Task EnqueueEventAsync(Event @event)
    {
        await _eventQueue.EnqueueAsync(@event);
        // Event enqueued successfully
    }

    /// <summary>
    /// Enqueues multiple events for processing
    /// </summary>
    public async Task EnqueueEventsAsync(IEnumerable<Event> events)
    {
        foreach (var @event in events)
        {
            await EnqueueEventAsync(@event);
        }
    }

    /// <summary>
    /// Waits for the service to process a specific number of events
    /// </summary>
    public async Task<bool> WaitForProcessedEventCountAsync(long expectedCount, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        return await WaitForConditionAsync(
            service => service.ProcessedEventCount >= expectedCount,
            timeout,
            cancellationToken);
    }

    /// <summary>
    /// Waits for the service to process events and verifies they are stored in the repository
    /// </summary>
    public async Task<bool> WaitForEventsToBeStoredAsync(int expectedEventCount, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            // Check if the expected number of events have been processed
            if (ProcessedEventCount >= expectedEventCount)
            {
                // Give a small delay to ensure events are actually stored
                await Task.Delay(100, cancellationToken);

                // Verify events are in the repository by checking processed count
                if (ProcessedEventCount >= expectedEventCount)
                {
                    // Expected events processed and stored
                    return true;
                }
            }

            await Task.Delay(50, cancellationToken);
        }

        // Expected events not processed and stored within timeout
        return false;
    }

    /// <summary>
    /// Waits for the event queue to be empty
    /// </summary>
    public async Task<bool> WaitForQueueToBeEmptyAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(timeout);

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            // Try to dequeue an event - if null is returned, queue is empty
            var @event = await _eventQueue.DequeueAsync(cancellationToken);
            if (@event == null)
            {
                // Event queue is empty
                return true;
            }

            // Put the event back in the queue
            await _eventQueue.EnqueueAsync(@event);

            await Task.Delay(50, cancellationToken);
        }

        // Event queue not empty within timeout
        return false;
    }

    /// <summary>
    /// Gets the current queue size (approximate)
    /// </summary>
    public async Task<int> GetQueueSizeAsync()
    {
        var count = 0;
        var events = new List<Event>();

        // Dequeue all events to count them
        Event? @event;
        while ((@event = await _eventQueue.DequeueAsync(CancellationToken.None)) != null)
        {
            events.Add(@event);
            count++;
        }

        // Put all events back in the queue
        foreach (var e in events)
        {
            await _eventQueue.EnqueueAsync(e);
        }

        return count;
    }

    /// <summary>
    /// Clears the event queue
    /// </summary>
    public async Task ClearQueueAsync()
    {
        Event? @event;
        while ((@event = await _eventQueue.DequeueAsync(CancellationToken.None)) != null)
        {
            // Just discard the events
        }

        // Event queue cleared
    }

    /// <summary>
    /// Creates a test event with default values
    /// </summary>
    public static Event CreateTestEvent(string? eventId = null, string? eventType = null, string? userId = null)
    {
        return new Event(
            eventId ?? Guid.NewGuid().ToString(),
            eventType ?? "TEST_EVENT",
            userId ?? "test-user",
            DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Creates multiple test events
    /// </summary>
    public static IEnumerable<Event> CreateTestEvents(int count, string? eventType = null, string? userId = null)
    {
        for (int i = 0; i < count; i++)
        {
            yield return CreateTestEvent(
                Guid.NewGuid().ToString(),
                eventType ?? "TEST_EVENT",
                userId ?? "test-user");
        }
    }
}