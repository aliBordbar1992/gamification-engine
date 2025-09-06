using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Infrastructure.Storage.InMemory;
using System.Diagnostics;

namespace GamificationEngine.Performance.Tests;

/// <summary>
/// Stress tests for high event volume scenarios
/// </summary>
public class EventIngestionStressTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IEventIngestionService _eventIngestionService;
    private readonly IEventRepository _eventRepository;
    private readonly IEventQueue _eventQueue;
    private readonly ILogger<EventIngestionStressTests> _logger;
    private bool _disposed = false;

    public EventIngestionStressTests()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Add repositories
        services.AddSingleton<IEventRepository, EventRepository>();
        services.AddSingleton<IEventQueue, InMemoryEventQueue>();
        services.AddSingleton<IUserStateRepository, InMemoryUserStateRepository>();

        // Add services
        services.AddScoped<IEventIngestionService, EventIngestionService>();

        // Add background service
        services.AddHostedService<EventQueueBackgroundService>();

        _serviceProvider = services.BuildServiceProvider();
        _eventIngestionService = _serviceProvider.GetRequiredService<IEventIngestionService>();
        _eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        _eventQueue = _serviceProvider.GetRequiredService<IEventQueue>();
        _logger = _serviceProvider.GetRequiredService<ILogger<EventIngestionStressTests>>();
    }

    [Fact]
    public async Task IngestHighVolumeEvents_ShouldHandle1000EventsPerSecond()
    {
        // Arrange
        const int totalEvents = 10000;
        const int targetEventsPerSecond = 1000;
        var events = GenerateTestEvents(totalEvents);

        var stopwatch = Stopwatch.StartNew();
        var successfulIngestions = 0;
        var failedIngestions = 0;

        // Act - Ingest events in batches to simulate high volume
        var batchSize = targetEventsPerSecond;
        var batches = events.Chunk(batchSize);

        foreach (var batch in batches)
        {
            var batchTasks = batch.Select(async @event =>
            {
                try
                {
                    var result = await _eventIngestionService.IngestEventAsync(@event);
                    if (result.IsSuccess)
                        Interlocked.Increment(ref successfulIngestions);
                    else
                        Interlocked.Increment(ref failedIngestions);
                }
                catch
                {
                    Interlocked.Increment(ref failedIngestions);
                }
            });

            await Task.WhenAll(batchTasks);

            // Wait to maintain target rate
            await Task.Delay(1000); // 1 second between batches
        }

        stopwatch.Stop();

        // Wait for background processing to complete
        await Task.Delay(5000);

        // Assert
        var actualEventsPerSecond = totalEvents / stopwatch.Elapsed.TotalSeconds;

        _logger.LogInformation("Stress Test Results:");
        _logger.LogInformation($"Total Events: {totalEvents}");
        _logger.LogInformation($"Successful Ingestions: {successfulIngestions}");
        _logger.LogInformation($"Failed Ingestions: {failedIngestions}");
        _logger.LogInformation($"Actual Events/Second: {actualEventsPerSecond:F2}");
        _logger.LogInformation($"Total Time: {stopwatch.Elapsed}");

        Assert.True(successfulIngestions >= totalEvents * 0.95, $"Expected at least 95% success rate, got {(double)successfulIngestions / totalEvents:P2}");
        Assert.True(actualEventsPerSecond >= targetEventsPerSecond * 0.8, $"Expected at least 80% of target rate, got {actualEventsPerSecond:F2} events/second");
    }

    [Fact]
    public async Task ConcurrentEventIngestion_ShouldHandleMultipleUsers()
    {
        // Arrange
        const int users = 100;
        const int eventsPerUser = 50;
        var totalEvents = users * eventsPerUser;

        var stopwatch = Stopwatch.StartNew();
        var successfulIngestions = 0;
        var failedIngestions = 0;

        // Act - Simulate concurrent users
        var userTasks = Enumerable.Range(1, users).Select(async userId =>
        {
            var userEvents = GenerateTestEventsForUser(userId.ToString(), eventsPerUser);

            foreach (var @event in userEvents)
            {
                try
                {
                    var result = await _eventIngestionService.IngestEventAsync(@event);
                    if (result.IsSuccess)
                        Interlocked.Increment(ref successfulIngestions);
                    else
                        Interlocked.Increment(ref failedIngestions);
                }
                catch
                {
                    Interlocked.Increment(ref failedIngestions);
                }
            }
        });

        await Task.WhenAll(userTasks);
        stopwatch.Stop();

        // Wait for background processing to complete
        await Task.Delay(5000);

        // Assert
        _logger.LogInformation("Concurrent Test Results:");
        _logger.LogInformation($"Users: {users}");
        _logger.LogInformation($"Events per User: {eventsPerUser}");
        _logger.LogInformation($"Total Events: {totalEvents}");
        _logger.LogInformation($"Successful Ingestions: {successfulIngestions}");
        _logger.LogInformation($"Failed Ingestions: {failedIngestions}");
        _logger.LogInformation($"Total Time: {stopwatch.Elapsed}");

        Assert.True(successfulIngestions >= totalEvents * 0.95, $"Expected at least 95% success rate, got {(double)successfulIngestions / totalEvents:P2}");
    }

    [Fact]
    public async Task MemoryUsage_ShouldRemainStableUnderLoad()
    {
        // Arrange
        const int totalEvents = 50000;
        var initialMemory = GC.GetTotalMemory(true);

        var events = GenerateTestEvents(totalEvents);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var successfulIngestions = 0;
        var batchSize = 1000;

        foreach (var batch in events.Chunk(batchSize))
        {
            var batchTasks = batch.Select(async @event =>
            {
                try
                {
                    var result = await _eventIngestionService.IngestEventAsync(@event);
                    if (result.IsSuccess)
                        Interlocked.Increment(ref successfulIngestions);
                }
                catch
                {
                    // Ignore individual failures for this test
                }
            });

            await Task.WhenAll(batchTasks);

            // Force garbage collection every 10 batches
            if (successfulIngestions % (batchSize * 10) == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        stopwatch.Stop();

        // Wait for background processing to complete
        await Task.Delay(10000);

        var finalMemory = GC.GetTotalMemory(false);
        var memoryIncrease = finalMemory - initialMemory;
        var memoryIncreaseMB = memoryIncrease / (1024.0 * 1024.0);

        // Assert
        _logger.LogInformation("Memory Usage Test Results:");
        _logger.LogInformation($"Total Events Processed: {successfulIngestions}");
        _logger.LogInformation($"Initial Memory: {initialMemory / (1024.0 * 1024.0):F2} MB");
        _logger.LogInformation($"Final Memory: {finalMemory / (1024.0 * 1024.0):F2} MB");
        _logger.LogInformation($"Memory Increase: {memoryIncreaseMB:F2} MB");
        _logger.LogInformation($"Memory per Event: {memoryIncreaseMB / successfulIngestions * 1024:F2} KB");

        // Memory should not increase by more than 1MB per 1000 events
        var expectedMaxMemoryIncrease = (successfulIngestions / 1000.0) * 1024 * 1024; // 1MB per 1000 events
        Assert.True(memoryIncrease <= expectedMaxMemoryIncrease,
            $"Memory increase ({memoryIncreaseMB:F2} MB) exceeds expected limit ({expectedMaxMemoryIncrease / (1024.0 * 1024.0):F2} MB)");
    }

    private static IEnumerable<Event> GenerateTestEvents(int count)
    {
        var random = new Random();
        var eventTypes = new[] { "USER_LOGIN", "USER_ACTION", "PURCHASE", "VIEW_PAGE", "COMMENT", "LIKE", "SHARE" };

        return Enumerable.Range(1, count).Select(i =>
        {
            var userId = $"user_{random.Next(1, 1000)}";
            var eventType = eventTypes[random.Next(eventTypes.Length)];

            return new Event(
                eventId: $"event_{i}",
                eventType: eventType,
                userId: userId,
                timestamp: DateTimeOffset.UtcNow.AddMilliseconds(-random.Next(0, 3600000)), // Random time within last hour
                attributes: new Dictionary<string, object>
                {
                    { "value", random.Next(1, 100) },
                    { "category", $"category_{random.Next(1, 10)}" },
                    { "source", $"source_{random.Next(1, 5)}" }
                }
            );
        });
    }

    private static IEnumerable<Event> GenerateTestEventsForUser(string userId, int count)
    {
        var random = new Random();
        var eventTypes = new[] { "USER_LOGIN", "USER_ACTION", "PURCHASE", "VIEW_PAGE", "COMMENT", "LIKE", "SHARE" };

        return Enumerable.Range(1, count).Select(i =>
        {
            var eventType = eventTypes[random.Next(eventTypes.Length)];

            return new Event(
                eventId: $"event_{userId}_{i}",
                eventType: eventType,
                userId: userId,
                timestamp: DateTimeOffset.UtcNow.AddMilliseconds(-random.Next(0, 3600000)),
                attributes: new Dictionary<string, object>
                {
                    { "value", random.Next(1, 100) },
                    { "category", $"category_{random.Next(1, 10)}" },
                    { "source", $"source_{random.Next(1, 5)}" }
                }
            );
        });
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _serviceProvider?.Dispose();
            _disposed = true;
        }
    }
}
