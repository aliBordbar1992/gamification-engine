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
    private readonly Mock<IEventValidationService> _mockEventValidationService;
    private readonly EventIngestionService _service;

    public EventIngestionServiceTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockEventQueue = new Mock<IEventQueue>();
        _mockEventValidationService = new Mock<IEventValidationService>();
        _service = new EventIngestionService(_mockEventRepository.Object, _mockEventQueue.Object, _mockEventValidationService.Object);
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
            new EventIngestionService(null!, _mockEventQueue.Object, _mockEventValidationService.Object));
    }

    [Fact]
    public void Constructor_WithNullEventQueue_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new EventIngestionService(_mockEventRepository.Object, null!, _mockEventValidationService.Object));
    }

    [Fact]
    public void Constructor_WithNullEventValidationService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new EventIngestionService(_mockEventRepository.Object, _mockEventQueue.Object, null!));
    }

    [Fact]
    public async Task IngestEventAsync_WithValidEvent_ShouldEnqueueEventAndReturnSuccess()
    {
        // Arrange
        var @event = new Event("test-1", "TEST_EVENT", "user123", DateTimeOffset.UtcNow);
        _mockEventValidationService.Setup(v => v.ValidateEventAsync(@event))
            .ReturnsAsync(true);
        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(@event);
        _mockEventValidationService.Verify(v => v.ValidateEventAsync(@event), Times.Once);
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
    public async Task IngestEventAsync_WhenValidationFails_ShouldReturnFailure()
    {
        // Arrange
        var @event = new Event("test-1", "TEST_EVENT", "user123", DateTimeOffset.UtcNow);
        _mockEventValidationService.Setup(v => v.ValidateEventAsync(@event))
            .ReturnsAsync(false);

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.Code.ShouldBe("INVALID_EVENT");
        _mockEventValidationService.Verify(v => v.ValidateEventAsync(@event), Times.Once);
        _mockEventQueue.Verify(q => q.EnqueueAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task IngestEventAsync_WhenEnqueueFails_ShouldReturnFailure()
    {
        // Arrange
        var @event = new Event("test-1", "TEST_EVENT", "user123", DateTimeOffset.UtcNow);
        _mockEventValidationService.Setup(v => v.ValidateEventAsync(@event))
            .ReturnsAsync(true);
        var expectedError = new EventStorageError("Queue is full");
        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Failure(expectedError));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(expectedError);
        _mockEventValidationService.Verify(v => v.ValidateEventAsync(@event), Times.Once);
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

    [Fact]
    public async Task GetEventByIdAsync_WithValidEventId_ShouldReturnEvent()
    {
        // Arrange
        var eventId = "test-event-123";
        var @event = new Event(eventId, "TEST_EVENT", "user123", DateTimeOffset.UtcNow);

        _mockEventRepository.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(@event);

        // Act
        var result = await _service.GetEventByIdAsync(eventId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.EventId.ShouldBe(eventId);
        result.Value!.EventType.ShouldBe("TEST_EVENT");
        result.Value!.UserId.ShouldBe("user123");
        _mockEventRepository.Verify(r => r.GetByIdAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetEventByIdAsync_WithNonExistentEventId_ShouldReturnNull()
    {
        // Arrange
        var eventId = "non-existent-event-123";

        _mockEventRepository.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((Event?)null);

        // Act
        var result = await _service.GetEventByIdAsync(eventId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
        _mockEventRepository.Verify(r => r.GetByIdAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetEventByIdAsync_WithEmptyEventId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.GetEventByIdAsync("");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.Code.ShouldBe("INVALID_PARAMETER");
        result.Error!.Message.ShouldContain("Event ID cannot be empty");
        _mockEventRepository.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetEventByIdAsync_WithNullEventId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.GetEventByIdAsync(null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.Code.ShouldBe("INVALID_PARAMETER");
        result.Error!.Message.ShouldContain("Event ID cannot be empty");
        _mockEventRepository.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var eventId = "test-event-123";
        _mockEventRepository.Setup(r => r.GetByIdAsync(eventId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _service.GetEventByIdAsync(eventId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.Code.ShouldBe("EVENT_RETRIEVAL_ERROR");
        result.Error!.Message.ShouldContain("Failed to retrieve event by ID");
        _mockEventRepository.Verify(r => r.GetByIdAsync(eventId), Times.Once);
    }
}