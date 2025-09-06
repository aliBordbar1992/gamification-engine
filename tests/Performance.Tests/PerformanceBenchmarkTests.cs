using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Infrastructure.Storage.InMemory;
using GamificationEngine.Infrastructure.Caching;
using GamificationEngine.Infrastructure.Metrics;
using System.Diagnostics;

namespace GamificationEngine.Performance.Tests;

/// <summary>
/// Performance benchmark tests for various components
/// </summary>
public class PerformanceBenchmarkTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ILogger<PerformanceBenchmarkTests> _logger;
    private bool _disposed = false;

    public PerformanceBenchmarkTests()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Add repositories
        services.AddSingleton<IEventRepository, EventRepository>();
        services.AddSingleton<IEventQueue, InMemoryEventQueue>();
        services.AddSingleton<IUserStateRepository, InMemoryUserStateRepository>();

        // Add caching
        services.AddSingleton<ICacheService, InMemoryCacheService>();

        // Add metrics
        services.AddSingleton<IPerformanceMetrics, InMemoryPerformanceMetrics>();

        // Add services
        services.AddScoped<IEventIngestionService, EventIngestionService>();

        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<PerformanceBenchmarkTests>>();
    }

    [Fact]
    public async Task EventRepository_PerformanceBenchmark()
    {
        // Arrange
        var repository = _serviceProvider.GetRequiredService<IEventRepository>();
        var metrics = _serviceProvider.GetRequiredService<IPerformanceMetrics>();
        const int iterations = 1000;
        var events = GenerateTestEvents(iterations);

        // Act
        using (metrics.StartTimer("event_repository.store"))
        {
            foreach (var @event in events)
            {
                await repository.StoreAsync(@event);
            }
        }

        // Measure retrieval performance
        var random = new Random();
        using (metrics.StartTimer("event_repository.retrieve"))
        {
            for (int i = 0; i < 100; i++)
            {
                var userId = $"user_{random.Next(1, 100)}";
                await repository.GetByUserIdAsync(userId, limit: 10);
            }
        }

        // Assert
        var stats = metrics.GetStatistics();
        var storeTiming = stats.Timings["event_repository.store"];
        var retrieveTiming = stats.Timings["event_repository.retrieve"];

        _logger.LogInformation("Event Repository Benchmark:");
        _logger.LogInformation($"Store {iterations} events: {storeTiming.AverageDuration.TotalMilliseconds:F2}ms average");
        _logger.LogInformation($"Retrieve 100 queries: {retrieveTiming.AverageDuration.TotalMilliseconds:F2}ms average");

        Assert.True(storeTiming.AverageDuration.TotalMilliseconds < 10, "Event storage should be fast");
        Assert.True(retrieveTiming.AverageDuration.TotalMilliseconds < 50, "Event retrieval should be fast");
    }

    [Fact]
    public async Task CacheService_PerformanceBenchmark()
    {
        // Arrange
        var cacheService = _serviceProvider.GetRequiredService<ICacheService>();
        var metrics = _serviceProvider.GetRequiredService<IPerformanceMetrics>();
        const int iterations = 10000;
        var testData = GenerateTestData(iterations);

        // Act - Test cache set performance
        using (metrics.StartTimer("cache.set"))
        {
            foreach (var item in testData)
            {
                await cacheService.SetAsync($"key_{item.Key}", item.Value);
            }
        }

        // Test cache get performance
        using (metrics.StartTimer("cache.get"))
        {
            foreach (var item in testData.Take(1000))
            {
                await cacheService.GetAsync<TestData>($"key_{item.Key}");
            }
        }

        // Test cache get or set performance
        using (metrics.StartTimer("cache.get_or_set"))
        {
            for (int i = 0; i < 1000; i++)
            {
                await cacheService.GetOrSetAsync($"key_{i}", () => Task.FromResult(new TestData { Key = i, Value = $"value_{i}" }));
            }
        }

        // Assert
        var stats = metrics.GetStatistics();
        var setTiming = stats.Timings["cache.set"];
        var getTiming = stats.Timings["cache.get"];
        var getOrSetTiming = stats.Timings["cache.get_or_set"];

        _logger.LogInformation("Cache Service Benchmark:");
        _logger.LogInformation($"Set {iterations} items: {setTiming.AverageDuration.TotalMicroseconds:F2}μs average");
        _logger.LogInformation($"Get 1000 items: {getTiming.AverageDuration.TotalMicroseconds:F2}μs average");
        _logger.LogInformation($"GetOrSet 1000 items: {getOrSetTiming.AverageDuration.TotalMicroseconds:F2}μs average");

        Assert.True(setTiming.AverageDuration.TotalMicroseconds < 100, "Cache set should be very fast");
        Assert.True(getTiming.AverageDuration.TotalMicroseconds < 50, "Cache get should be very fast");
        Assert.True(getOrSetTiming.AverageDuration.TotalMicroseconds < 200, "Cache get or set should be fast");
    }

    [Fact]
    public async Task EventQueue_PerformanceBenchmark()
    {
        // Arrange
        var eventQueue = _serviceProvider.GetRequiredService<IEventQueue>();
        var metrics = _serviceProvider.GetRequiredService<IPerformanceMetrics>();
        const int iterations = 10000;
        var events = GenerateTestEvents(iterations);

        // Act - Test enqueue performance
        using (metrics.StartTimer("event_queue.enqueue"))
        {
            foreach (var @event in events)
            {
                await eventQueue.EnqueueAsync(@event);
            }
        }

        // Test dequeue performance
        var dequeuedEvents = 0;
        using (metrics.StartTimer("event_queue.dequeue"))
        {
            while (dequeuedEvents < iterations)
            {
                var @event = await eventQueue.DequeueAsync(CancellationToken.None);
                if (@event != null)
                {
                    dequeuedEvents++;
                }
                else
                {
                    break; // Queue is empty
                }
            }
        }

        // Assert
        var stats = metrics.GetStatistics();
        var enqueueTiming = stats.Timings["event_queue.enqueue"];
        var dequeueTiming = stats.Timings["event_queue.dequeue"];

        _logger.LogInformation("Event Queue Benchmark:");
        _logger.LogInformation($"Enqueue {iterations} events: {enqueueTiming.AverageDuration.TotalMicroseconds:F2}μs average");
        _logger.LogInformation($"Dequeue {dequeuedEvents} events: {dequeueTiming.AverageDuration.TotalMicroseconds:F2}μs average");

        Assert.True(enqueueTiming.AverageDuration.TotalMicroseconds < 50, "Event enqueue should be very fast");
        Assert.True(dequeueTiming.AverageDuration.TotalMicroseconds < 100, "Event dequeue should be very fast");
    }

    [Fact]
    public async Task ConcurrentOperations_PerformanceBenchmark()
    {
        // Arrange
        var repository = _serviceProvider.GetRequiredService<IEventRepository>();
        var cacheService = _serviceProvider.GetRequiredService<ICacheService>();
        var metrics = _serviceProvider.GetRequiredService<IPerformanceMetrics>();
        const int concurrentTasks = 100;
        const int operationsPerTask = 100;

        // Act
        var tasks = Enumerable.Range(1, concurrentTasks).Select(async taskId =>
        {
            using (metrics.StartTimer("concurrent_operations", new Dictionary<string, string> { { "taskId", taskId.ToString() } }))
            {
                for (int i = 0; i < operationsPerTask; i++)
                {
                    var @event = new Event(
                        eventId: $"event_{taskId}_{i}",
                        eventType: "TEST_EVENT",
                        userId: $"user_{taskId}",
                        timestamp: DateTimeOffset.UtcNow,
                        attributes: new Dictionary<string, object> { { "value", i } }
                    );

                    await repository.StoreAsync(@event);
                    await cacheService.SetAsync($"cache_{taskId}_{i}", @event);
                }
            }
        });

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var stats = metrics.GetStatistics();
        var totalOperations = concurrentTasks * operationsPerTask * 2; // Store + Cache
        var operationsPerSecond = totalOperations / stopwatch.Elapsed.TotalSeconds;

        _logger.LogInformation("Concurrent Operations Benchmark:");
        _logger.LogInformation($"Concurrent Tasks: {concurrentTasks}");
        _logger.LogInformation($"Operations per Task: {operationsPerTask}");
        _logger.LogInformation($"Total Operations: {totalOperations}");
        _logger.LogInformation($"Total Time: {stopwatch.Elapsed}");
        _logger.LogInformation($"Operations per Second: {operationsPerSecond:F2}");

        Assert.True(operationsPerSecond > 1000, $"Expected at least 1000 operations/second, got {operationsPerSecond:F2}");
    }

    private static IEnumerable<Event> GenerateTestEvents(int count)
    {
        var random = new Random();
        var eventTypes = new[] { "USER_LOGIN", "USER_ACTION", "PURCHASE", "VIEW_PAGE", "COMMENT" };

        return Enumerable.Range(1, count).Select(i =>
        {
            var userId = $"user_{random.Next(1, 100)}";
            var eventType = eventTypes[random.Next(eventTypes.Length)];

            return new Event(
                eventId: $"event_{i}",
                eventType: eventType,
                userId: userId,
                timestamp: DateTimeOffset.UtcNow,
                attributes: new Dictionary<string, object>
                {
                    { "value", random.Next(1, 100) },
                    { "category", $"category_{random.Next(1, 10)}" }
                }
            );
        });
    }

    private static IEnumerable<TestData> GenerateTestData(int count)
    {
        return Enumerable.Range(1, count).Select(i => new TestData
        {
            Key = i,
            Value = $"value_{i}",
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    private class TestData
    {
        public int Key { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
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
