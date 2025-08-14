using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.Services;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Shared;
using Moq;
using Shouldly;
using Xunit;

namespace GamificationEngine.Application.Tests;

public class EventIngestionServiceTests
{
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<IEventQueue> _mockEventQueue;
    private readonly EventIngestionService _service;

    public EventIngestionServiceTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockEventQueue = new Mock<IEventQueue>();
        _service = new EventIngestionService(_mockEventRepository.Object, _mockEventQueue.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act & Assert
        _service.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullEventRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new EventIngestionService(null!, _mockEventQueue.Object));
    }

    [Fact]
    public void Constructor_WithNullEventQueue_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new EventIngestionService(_mockEventRepository.Object, null!));
    }

    [Fact]
    public async Task IngestEventAsync_WithValidEvent_ShouldEnqueueEventAndReturnSuccess()
    {
        // Arrange
        var @event = new Event("test-1", "TEST_EVENT", "user123", DateTimeOffset.UtcNow);
        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(@event);
        _mockEventQueue.Verify(q => q.EnqueueAsync(@event), Times.Once);
        _mockEventRepository.Verify(r => r.StoreAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task IngestEventAsync_WithNullEvent_ShouldReturnFailure()
    {
        // Act
        var result = await _service.IngestEventAsync(null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.Code.ShouldBe("INVALID_EVENT");
        _mockEventQueue.Verify(q => q.EnqueueAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task IngestEventAsync_WhenEnqueueFails_ShouldReturnFailure()
    {
        // Arrange
        var @event = new Event("test-1", "TEST_EVENT", "user123", DateTimeOffset.UtcNow);
        var expectedError = new EventStorageError("Queue is full");
        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Failure(expectedError));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(expectedError);
        _mockEventQueue.Verify(q => q.EnqueueAsync(@event), Times.Once);
    }

    [Fact]
    public async Task IngestEventAsync_WhenEnqueueThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var @event = new Event("test-1", "TEST_EVENT", "user123", DateTimeOffset.UtcNow);
        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.Code.ShouldBe("EVENT_STORAGE_ERROR");
        result.Error!.Message.ShouldContain("Failed to enqueue event");
    }

    [Fact]
    public async Task GetUserEventsAsync_WithValidParameters_ShouldReturnEvents()
    {
        // Arrange
        var userId = "user123";
        var events = new List<Event>
        {
            new Event("test-1", "TEST_EVENT_1", userId, DateTimeOffset.UtcNow),
            new Event("test-2", "TEST_EVENT_2", userId, DateTimeOffset.UtcNow)
        };

        _mockEventRepository.Setup(r => r.GetByUserIdAsync(userId, 100, 0))
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetUserEventsAsync(userId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count().ShouldBe(2);
        result.Value!.First().EventId.ShouldBe("test-1");
        result.Value!.Last().EventId.ShouldBe("test-2");
        _mockEventRepository.Verify(r => r.GetByUserIdAsync(userId, 100, 0), Times.Once);
    }

    [Fact]
    public async Task GetUserEventsAsync_WithEmptyUserId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.GetUserEventsAsync("");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.Code.ShouldBe("INVALID_USER_ID");
        _mockEventRepository.Verify(r => r.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetUserEventsAsync_WithInvalidLimit_ShouldReturnFailure()
    {
        // Act
        var result = await _service.GetUserEventsAsync("user123", 0);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.Code.ShouldBe("INVALID_PARAMETER");
        _mockEventRepository.Verify(r => r.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetUserEventsAsync_WithInvalidOffset_ShouldReturnFailure()
    {
        // Act
        var result = await _service.GetUserEventsAsync("user123", 100, -1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.Code.ShouldBe("INVALID_PARAMETER");
        _mockEventRepository.Verify(r => r.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetEventsByTypeAsync_WithValidParameters_ShouldReturnEvents()
    {
        // Arrange
        var eventType = "TEST_EVENT";
        var events = new List<Event>
        {
            new Event("test-1", eventType, "user123", DateTimeOffset.UtcNow),
            new Event("test-2", eventType, "user456", DateTimeOffset.UtcNow)
        };

        _mockEventRepository.Setup(r => r.GetByTypeAsync(eventType, 100, 0))
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetEventsByTypeAsync(eventType);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count().ShouldBe(2);
        _mockEventRepository.Verify(r => r.GetByTypeAsync(eventType, 100, 0), Times.Once);
    }

    [Fact]
    public async Task GetEventsByTypeAsync_WithEmptyEventType_ShouldReturnFailure()
    {
        // Act
        var result = await _service.GetEventsByTypeAsync("");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.Code.ShouldBe("INVALID_EVENT_TYPE");
        _mockEventRepository.Verify(r => r.GetByTypeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}