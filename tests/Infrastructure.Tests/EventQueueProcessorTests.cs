using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Infrastructure.Storage.InMemory;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace GamificationEngine.Infrastructure.Tests;

public class EventQueueProcessorTests
{
    private readonly Mock<IEventQueue> _mockEventQueue;
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<ILogger<EventQueueProcessor>> _mockLogger;

    public EventQueueProcessorTests()
    {
        _mockEventQueue = new Mock<IEventQueue>();
        _mockEventRepository = new Mock<IEventRepository>();
        _mockLogger = new Mock<ILogger<EventQueueProcessor>>();
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var processor = new EventQueueProcessor(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);

        // Assert
        processor.ShouldNotBeNull();
        processor.IsProcessing.ShouldBeFalse();
        processor.ProcessedEventCount.ShouldBe(0);
    }

    [Fact]
    public void Constructor_WithNullEventQueue_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new EventQueueProcessor(null!, _mockEventRepository.Object, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullEventRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new EventQueueProcessor(_mockEventQueue.Object, null!, _mockLogger.Object));
    }

    [Fact]
    public async Task StartProcessingAsync_WhenNotProcessing_ShouldStartProcessing()
    {
        // Arrange
        var processor = new EventQueueProcessor(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);
        _mockEventQueue.Setup(q => q.DequeueAsync()).ReturnsAsync((Event?)null);

        // Act
        await processor.StartProcessingAsync();

        // Assert
        processor.IsProcessing.ShouldBeTrue();
    }

    [Fact]
    public async Task StartProcessingAsync_WhenAlreadyProcessing_ShouldNotStartAgain()
    {
        // Arrange
        var processor = new EventQueueProcessor(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);
        _mockEventQueue.Setup(q => q.DequeueAsync()).ReturnsAsync((Event?)null);

        // Act
        await processor.StartProcessingAsync();
        await processor.StartProcessingAsync(); // Try to start again

        // Assert
        processor.IsProcessing.ShouldBeTrue();
    }

    [Fact]
    public async Task StopProcessingAsync_WhenProcessing_ShouldStopProcessing()
    {
        // Arrange
        var processor = new EventQueueProcessor(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);
        _mockEventQueue.Setup(q => q.DequeueAsync()).ReturnsAsync((Event?)null);

        await processor.StartProcessingAsync();

        // Act
        await processor.StopProcessingAsync();

        // Assert
        processor.IsProcessing.ShouldBeFalse();
    }

    [Fact]
    public async Task StopProcessingAsync_WhenNotProcessing_ShouldNotStop()
    {
        // Arrange
        var processor = new EventQueueProcessor(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);

        // Act
        await processor.StopProcessingAsync();

        // Assert
        processor.IsProcessing.ShouldBeFalse();
    }

    [Fact]
    public async Task ProcessEventsAsync_ShouldProcessEventsAndStoreThem()
    {
        // Arrange
        var events = new List<Event>
        {
            new Event("test-1", "TEST_EVENT_1", "user123", DateTimeOffset.UtcNow),
            new Event("test-2", "TEST_EVENT_2", "user123", DateTimeOffset.UtcNow)
        };

        var eventIndex = 0;
        _mockEventQueue.Setup(q => q.DequeueAsync())
            .ReturnsAsync(() => eventIndex < events.Count ? events[eventIndex++] : null);

        var processor = new EventQueueProcessor(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);

        // Act
        await processor.StartProcessingAsync();
        await Task.Delay(100); // Give time for processing
        await processor.StopProcessingAsync();

        // Assert
        _mockEventRepository.Verify(r => r.StoreAsync(It.IsAny<Event>()), Times.Exactly(2));
        processor.ProcessedEventCount.ShouldBe(2);
    }

    [Fact]
    public async Task ProcessEventsAsync_WithException_ShouldContinueProcessing()
    {
        // Arrange
        var event1 = new Event("test-1", "TEST_EVENT_1", "user123", DateTimeOffset.UtcNow);
        var event2 = new Event("test-2", "TEST_EVENT_2", "user123", DateTimeOffset.UtcNow);

        var callCount = 0;
        _mockEventQueue.Setup(q => q.DequeueAsync())
            .ReturnsAsync(() => callCount++ < 2 ? (callCount == 1 ? event1 : event2) : null);

        _mockEventRepository.Setup(r => r.StoreAsync(event1))
            .ThrowsAsync(new Exception("Storage error"));
        _mockEventRepository.Setup(r => r.StoreAsync(event2))
            .Returns(Task.CompletedTask);

        var processor = new EventQueueProcessor(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);

        // Act
        await processor.StartProcessingAsync();
        await Task.Delay(100); // Give time for processing
        await processor.StopProcessingAsync();

        // Assert
        _mockEventRepository.Verify(r => r.StoreAsync(It.IsAny<Event>()), Times.Exactly(2));
        processor.ProcessedEventCount.ShouldBe(1); // Only second event processed successfully
    }

    [Fact]
    public void Dispose_ShouldCancelProcessing()
    {
        // Arrange
        var processor = new EventQueueProcessor(_mockEventQueue.Object, _mockEventRepository.Object, _mockLogger.Object);

        // Act
        processor.Dispose();

        // Assert
        processor.IsProcessing.ShouldBeFalse();
    }
}