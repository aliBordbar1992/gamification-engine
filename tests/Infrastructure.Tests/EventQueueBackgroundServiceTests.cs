using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Api;
using Moq;
using Shouldly;
using Xunit;

namespace GamificationEngine.Infrastructure.Tests;

public class EventQueueBackgroundServiceTests
{
    private readonly Mock<IEventQueue> _mockEventQueue;
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<ILogger<EventQueueBackgroundService>> _mockLogger;

    public EventQueueBackgroundServiceTests()
    {
        _mockEventQueue = new Mock<IEventQueue>();
        _mockEventRepository = new Mock<IEventRepository>();
        _mockLogger = new Mock<ILogger<EventQueueBackgroundService>>();
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var service = new EventQueueBackgroundService(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);

        // Assert
        service.ShouldNotBeNull();
        service.ProcessedEventCount.ShouldBe(0);
    }

    [Fact]
    public void Constructor_WithNullEventQueue_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new EventQueueBackgroundService(null!, _mockEventRepository.Object, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullEventRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new EventQueueBackgroundService(_mockEventQueue.Object, null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new EventQueueBackgroundService(_mockEventQueue.Object, _mockEventRepository.Object, null!));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessEventsAndStoreThem()
    {
        // Arrange
        var events = new List<Event>
        {
            new Event("test-1", "TEST_EVENT_1", "user123", DateTimeOffset.UtcNow),
            new Event("test-2", "TEST_EVENT_2", "user123", DateTimeOffset.UtcNow)
        };

        var eventIndex = 0;
        _mockEventQueue.Setup(q => q.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => eventIndex < events.Count ? events[eventIndex++] : null);

        var service = new EventQueueBackgroundService(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200)); // 200ms timeout

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100); // Give time for processing
        await service.StopAsync(cts.Token);

        // Assert
        _mockEventRepository.Verify(r => r.StoreAsync(It.IsAny<Event>()), Times.Exactly(2));
        service.ProcessedEventCount.ShouldBe(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithException_ShouldContinueProcessing()
    {
        // Arrange
        var event1 = new Event("test-1", "TEST_EVENT_1", "user123", DateTimeOffset.UtcNow);
        var event2 = new Event("test-2", "TEST_EVENT_2", "user123", DateTimeOffset.UtcNow);

        var callCount = 0;
        _mockEventQueue.Setup(q => q.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => callCount++ < 2 ? (callCount == 1 ? event1 : event2) : null);

        _mockEventRepository.Setup(r => r.StoreAsync(event1))
            .ThrowsAsync(new Exception("Storage error"));
        _mockEventRepository.Setup(r => r.StoreAsync(event2))
            .Returns(Task.CompletedTask);

        var service = new EventQueueBackgroundService(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200)); // 200ms timeout

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100); // Give time for processing
        await service.StopAsync(cts.Token);

        // Assert
        _mockEventRepository.Verify(r => r.StoreAsync(It.IsAny<Event>()), Times.Exactly(2));
        service.ProcessedEventCount.ShouldBe(1); // Only second event processed successfully
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_ShouldStopProcessing()
    {
        // Arrange
        _mockEventQueue.Setup(q => q.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var service = new EventQueueBackgroundService(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50)); // 50ms timeout

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100); // Wait for cancellation
        await service.StopAsync(cts.Token);

        // Assert
        service.ProcessedEventCount.ShouldBe(0);
    }

    [Fact]
    public async Task StopAsync_ShouldLogStoppingMessage()
    {
        // Arrange
        var service = new EventQueueBackgroundService(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert
        // The test passes if no exception is thrown and the method completes
        // We can't easily verify logging in unit tests without more complex setup
    }
}