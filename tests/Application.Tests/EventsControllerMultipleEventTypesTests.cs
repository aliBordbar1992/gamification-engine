using Microsoft.AspNetCore.Mvc;
using GamificationEngine.Api.Controllers;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Shared;
using Moq;
using Shouldly;
using Xunit;

namespace GamificationEngine.Application.Tests;

public class EventsControllerMultipleEventTypesTests
{
    private readonly Mock<IEventIngestionService> _mockEventIngestionService;
    private readonly EventsController _controller;

    public EventsControllerMultipleEventTypesTests()
    {
        _mockEventIngestionService = new Mock<IEventIngestionService>();
        _controller = new EventsController(_mockEventIngestionService.Object);
    }

    private static void AssertEventsEqual(Event expected, object? actual)
    {
        actual.ShouldNotBeNull();
        actual.ShouldBeOfType<Event>();
        var actualEvent = (Event)actual!;
        actualEvent.EventId.ShouldBe(expected.EventId);
        actualEvent.EventType.ShouldBe(expected.EventType);
        actualEvent.UserId.ShouldBe(expected.UserId);
        actualEvent.OccurredAt.ShouldBe(expected.OccurredAt);
        if (expected.Attributes is null)
        {
            actualEvent.Attributes.ShouldNotBeNull();
            actualEvent.Attributes.Count.ShouldBe(0);
        }
        else
        {
            actualEvent.Attributes.Count.ShouldBe(expected.Attributes.Count);
            foreach (var kv in expected.Attributes)
            {
                actualEvent.Attributes.ContainsKey(kv.Key).ShouldBeTrue();
            }
        }
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act & Assert
        _controller.ShouldNotBeNull();
    }

    [Fact]
    public async Task IngestEvent_WithUserCommentedEvent_ShouldReturnCreated()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "comment-api-1",
            EventType = "USER_COMMENTED",
            UserId = "user-api-123",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "commentId", "comment-api-456" },
                { "postId", "post-api-789" },
                { "text", "Great post via API!" }
            }
        };

        var expectedEvent = new Event(
            request.EventId,
            request.EventType,
            request.UserId,
            request.OccurredAt.Value,
            request.Attributes
        );

        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        // Act
        var result = await _controller.IngestEvent(request);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        AssertEventsEqual(expectedEvent, createdAtResult!.Value);
        createdAtResult.ActionName.ShouldBe("GetEvent");
        createdAtResult.RouteValues!["eventId"].ShouldBe(request.EventId);
    }

    [Fact]
    public async Task IngestEvent_WithProfileCompletedEvent_ShouldReturnCreated()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "profile-api-1",
            EventType = "PROFILE_COMPLETED",
            UserId = "user-api-456",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "completenessPercent", 87.5 }
            }
        };

        var expectedEvent = new Event(
            request.EventId,
            request.EventType,
            request.UserId,
            request.OccurredAt.Value,
            request.Attributes
        );

        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        // Act
        var result = await _controller.IngestEvent(request);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        AssertEventsEqual(expectedEvent, createdAtResult!.Value);
    }

    [Fact]
    public async Task IngestEvent_WithUserVisitedSpecialOffersEvent_ShouldReturnCreated()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "visit-api-1",
            EventType = "USER_VISITED_SPECIAL_OFFERS",
            UserId = "user-api-789",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "sessionId", "session-api-abc-123" }
            }
        };

        var expectedEvent = new Event(
            request.EventId,
            request.EventType,
            request.UserId,
            request.OccurredAt.Value,
            request.Attributes
        );

        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        // Act
        var result = await _controller.IngestEvent(request);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        AssertEventsEqual(expectedEvent, createdAtResult!.Value);
    }

    [Fact]
    public async Task IngestEvent_WithUserPurchasedProductEvent_ShouldReturnCreated()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "purchase-api-1",
            EventType = "USER_PURCHASED_PRODUCT",
            UserId = "user-api-101",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "productId", "prod-api-123" },
                { "source", "special_offers" },
                { "amount", 49.99m },
                { "currency", "GBP" }
            }
        };

        var expectedEvent = new Event(
            request.EventId,
            request.EventType,
            request.UserId,
            request.OccurredAt.Value,
            request.Attributes
        );

        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        // Act
        var result = await _controller.IngestEvent(request);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        AssertEventsEqual(expectedEvent, createdAtResult!.Value);
    }

    [Fact]
    public async Task IngestEvent_WithUserReceivedDislikeEvent_ShouldReturnCreated()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "dislike-api-1",
            EventType = "USER_RECEIVED_DISLIKE",
            UserId = "user-api-202",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "postId", "post-api-xyz-789" }
            }
        };

        var expectedEvent = new Event(
            request.EventId,
            request.EventType,
            request.UserId,
            request.OccurredAt.Value,
            request.Attributes
        );

        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        // Act
        var result = await _controller.IngestEvent(request);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        AssertEventsEqual(expectedEvent, createdAtResult!.Value);
    }

    [Fact]
    public async Task IngestEvent_WithDisputeCreatedEvent_ShouldReturnCreated()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "dispute-api-1",
            EventType = "DISPUTE_CREATED",
            UserId = "user-api-303",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "disputeId", "dispute-api-abc-def" }
            }
        };

        var expectedEvent = new Event(
            request.EventId,
            request.EventType,
            request.UserId,
            request.OccurredAt.Value,
            request.Attributes
        );

        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        // Act
        var result = await _controller.IngestEvent(request);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        AssertEventsEqual(expectedEvent, createdAtResult!.Value);
    }

    [Fact]
    public async Task IngestEvent_WithBadgeGrantedEvent_ShouldReturnCreated()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "badge-api-1",
            EventType = "BADGE_GRANTED",
            UserId = "user-api-404",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "badgeId", "badge-api-commenter" }
            }
        };

        var expectedEvent = new Event(
            request.EventId,
            request.EventType,
            request.UserId,
            request.OccurredAt.Value,
            request.Attributes
        );

        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        // Act
        var result = await _controller.IngestEvent(request);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        AssertEventsEqual(expectedEvent, createdAtResult!.Value);
    }

    [Fact]
    public async Task IngestEvent_WithComplexNestedAttributes_ShouldReturnCreated()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "complex-api-1",
            EventType = "USER_PURCHASED_PRODUCT",
            UserId = "user-api-505",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "productId", "prod-api-456" },
                { "source", "special_offers" },
                { "amount", 199.99m },
                { "currency", "JPY" },
                { "metadata", new Dictionary<string, object>
                    {
                        { "campaign", "winter_sale" },
                        { "discount", 0.25 },
                        { "tags", new[] { "electronics", "premium", "winter_sale" } }
                    }
                },
                { "shipping", new Dictionary<string, object>
                    {
                        { "method", "standard" },
                        { "cost", 0m },
                        { "estimatedDays", 5 }
                    }
                }
            }
        };

        var expectedEvent = new Event(
            request.EventId,
            request.EventType,
            request.UserId,
            request.OccurredAt.Value,
            request.Attributes
        );

        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        // Act
        var result = await _controller.IngestEvent(request);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        AssertEventsEqual(expectedEvent, createdAtResult!.Value);
    }

    [Fact]
    public async Task IngestEvent_WithAutoGeneratedEventId_ShouldUseGeneratedId()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventType = "USER_COMMENTED",
            UserId = "user-api-auto",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "commentId", "comment-auto-123" }
            }
        };

        // Mock the service to capture the actual event passed to it
        Event capturedEvent = null!;
        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .Callback<Event>(e => capturedEvent = e)
            .ReturnsAsync(Result<Event, DomainError>.Success(capturedEvent));

        // Act
        var result = await _controller.IngestEvent(request);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        capturedEvent.ShouldNotBeNull();
        capturedEvent.EventId.ShouldNotBeNullOrEmpty();
        capturedEvent.EventId.ShouldNotBe(request.EventId); // Should be auto-generated
        capturedEvent.EventType.ShouldBe(request.EventType);
        capturedEvent.UserId.ShouldBe(request.UserId);
    }

    [Fact]
    public async Task IngestEvent_WithAutoGeneratedTimestamp_ShouldUseCurrentTime()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "timestamp-api-1",
            EventType = "PROFILE_COMPLETED",
            UserId = "user-api-timestamp",
            Attributes = new Dictionary<string, object>
            {
                { "completenessPercent", 100 }
            }
        };

        var beforeRequest = DateTimeOffset.UtcNow;

        // Mock the service to capture the actual event passed to it
        Event capturedEvent = null!;
        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .Callback<Event>(e => capturedEvent = e)
            .ReturnsAsync(Result<Event, DomainError>.Success(capturedEvent));

        // Act
        var result = await _controller.IngestEvent(request);

        var afterRequest = DateTimeOffset.UtcNow;

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        capturedEvent.ShouldNotBeNull();
        capturedEvent.OccurredAt.ShouldBeGreaterThanOrEqualTo(beforeRequest);
        capturedEvent.OccurredAt.ShouldBeLessThanOrEqualTo(afterRequest);
    }

    [Fact]
    public async Task IngestEvent_WithMultipleEventTypesForSameUser_ShouldHandleCorrectly()
    {
        // Arrange
        var userId = "user-api-multi";
        var requests = new[]
        {
            new IngestEventRequest
            {
                EventId = "multi-1",
                EventType = "USER_COMMENTED",
                UserId = userId,
                OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            },
            new IngestEventRequest
            {
                EventId = "multi-2",
                EventType = "PROFILE_COMPLETED",
                UserId = userId,
                OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-4)
            },
            new IngestEventRequest
            {
                EventId = "multi-3",
                EventType = "USER_VISITED_SPECIAL_OFFERS",
                UserId = userId,
                OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-3)
            },
            new IngestEventRequest
            {
                EventId = "multi-4",
                EventType = "USER_PURCHASED_PRODUCT",
                UserId = userId,
                OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-2)
            },
            new IngestEventRequest
            {
                EventId = "multi-5",
                EventType = "BADGE_GRANTED",
                UserId = userId,
                OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-1)
            }
        };

        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync((Event e) => Result<Event, DomainError>.Success(e));

        // Act & Assert
        foreach (var request in requests)
        {
            var result = await _controller.IngestEvent(request);
            result.ShouldBeOfType<CreatedAtActionResult>();
        }

        _mockEventIngestionService.Verify(s => s.IngestEventAsync(It.IsAny<Event>()), Times.Exactly(5));
    }

    [Fact]
    public async Task IngestEvent_WithServiceFailure_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "failure-api-1",
            EventType = "USER_COMMENTED",
            UserId = "user-api-failure",
            OccurredAt = DateTimeOffset.UtcNow
        };

        var expectedError = new InvalidEventError("Event validation failed");
        _mockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Failure(expectedError));

        // Act
        var result = await _controller.IngestEvent(request);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.ShouldNotBeNull();
        var errorProp = badRequestResult.Value.GetType().GetProperty("error");
        errorProp.ShouldNotBeNull();
        var errorValue = errorProp!.GetValue(badRequestResult.Value) as string;
        errorValue.ShouldBe(expectedError.Message);
    }
}
