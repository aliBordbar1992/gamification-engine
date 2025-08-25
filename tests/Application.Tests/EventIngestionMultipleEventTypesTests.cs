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

public class EventIngestionMultipleEventTypesTests
{
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<IEventQueue> _mockEventQueue;
    private readonly EventIngestionService _service;

    public EventIngestionMultipleEventTypesTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockEventQueue = new Mock<IEventQueue>();
        _service = new EventIngestionService(_mockEventRepository.Object, _mockEventQueue.Object);
    }

    [Fact]
    public async Task IngestEvent_WithUserCommentedEvent_ShouldSucceed()
    {
        // Arrange
        var @event = new Event(
            "comment-event-1",
            "USER_COMMENTED",
            "user-123",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                { "commentId", "comment-456" },
                { "postId", "post-789" },
                { "text", "Great post!" }
            }
        );

        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(@event);
        result.Value!.EventType.ShouldBe("USER_COMMENTED");
        result.Value!.Attributes["commentId"].ShouldBe("comment-456");
        result.Value!.Attributes["postId"].ShouldBe("post-789");
        result.Value!.Attributes["text"].ShouldBe("Great post!");
        _mockEventQueue.Verify(q => q.EnqueueAsync(@event), Times.Once);
    }

    [Fact]
    public async Task IngestEvent_WithProfileCompletedEvent_ShouldSucceed()
    {
        // Arrange
        var @event = new Event(
            "profile-event-1",
            "PROFILE_COMPLETED",
            "user-456",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                { "completenessPercent", 95.5 }
            }
        );

        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.EventType.ShouldBe("PROFILE_COMPLETED");
        result.Value!.Attributes["completenessPercent"].ShouldBe(95.5);
        _mockEventQueue.Verify(q => q.EnqueueAsync(@event), Times.Once);
    }

    [Fact]
    public async Task IngestEvent_WithUserVisitedSpecialOffersEvent_ShouldSucceed()
    {
        // Arrange
        var @event = new Event(
            "visit-event-1",
            "USER_VISITED_SPECIAL_OFFERS",
            "user-789",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                { "sessionId", "session-abc-123" }
            }
        );

        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.EventType.ShouldBe("USER_VISITED_SPECIAL_OFFERS");
        result.Value!.Attributes["sessionId"].ShouldBe("session-abc-123");
        _mockEventQueue.Verify(q => q.EnqueueAsync(@event), Times.Once);
    }

    [Fact]
    public async Task IngestEvent_WithUserPurchasedProductEvent_ShouldSucceed()
    {
        // Arrange
        var @event = new Event(
            "purchase-event-1",
            "USER_PURCHASED_PRODUCT",
            "user-101",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                { "productId", "prod-123" },
                { "source", "special_offers" },
                { "amount", 29.99m },
                { "currency", "USD" }
            }
        );

        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.EventType.ShouldBe("USER_PURCHASED_PRODUCT");
        result.Value!.Attributes["productId"].ShouldBe("prod-123");
        result.Value!.Attributes["source"].ShouldBe("special_offers");
        result.Value!.Attributes["amount"].ShouldBe(29.99m);
        result.Value!.Attributes["currency"].ShouldBe("USD");
        _mockEventQueue.Verify(q => q.EnqueueAsync(@event), Times.Once);
    }

    [Fact]
    public async Task IngestEvent_WithUserReceivedDislikeEvent_ShouldSucceed()
    {
        // Arrange
        var @event = new Event(
            "dislike-event-1",
            "USER_RECEIVED_DISLIKE",
            "user-202",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                { "postId", "post-xyz-789" }
            }
        );

        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.EventType.ShouldBe("USER_RECEIVED_DISLIKE");
        result.Value!.Attributes["postId"].ShouldBe("post-xyz-789");
        _mockEventQueue.Verify(q => q.EnqueueAsync(@event), Times.Once);
    }

    [Fact]
    public async Task IngestEvent_WithDisputeCreatedEvent_ShouldSucceed()
    {
        // Arrange
        var @event = new Event(
            "dispute-event-1",
            "DISPUTE_CREATED",
            "user-303",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                { "disputeId", "dispute-abc-def" }
            }
        );

        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.EventType.ShouldBe("DISPUTE_CREATED");
        result.Value!.Attributes["disputeId"].ShouldBe("dispute-abc-def");
        _mockEventQueue.Verify(q => q.EnqueueAsync(@event), Times.Once);
    }

    [Fact]
    public async Task IngestEvent_WithBadgeGrantedEvent_ShouldSucceed()
    {
        // Arrange
        var @event = new Event(
            "badge-event-1",
            "BADGE_GRANTED",
            "user-404",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                { "badgeId", "badge-commenter" }
            }
        );

        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.EventType.ShouldBe("BADGE_GRANTED");
        result.Value!.Attributes["badgeId"].ShouldBe("badge-commenter");
        _mockEventQueue.Verify(q => q.EnqueueAsync(@event), Times.Once);
    }

    [Fact]
    public async Task IngestEvent_WithMultipleEventTypesForSameUser_ShouldSucceed()
    {
        // Arrange
        var userId = "user-multi";
        var events = new[]
        {
            new Event("event-1", "USER_COMMENTED", userId, DateTimeOffset.UtcNow.AddMinutes(-5)),
            new Event("event-2", "PROFILE_COMPLETED", userId, DateTimeOffset.UtcNow.AddMinutes(-4)),
            new Event("event-3", "USER_VISITED_SPECIAL_OFFERS", userId, DateTimeOffset.UtcNow.AddMinutes(-3)),
            new Event("event-4", "USER_PURCHASED_PRODUCT", userId, DateTimeOffset.UtcNow.AddMinutes(-2)),
            new Event("event-5", "BADGE_GRANTED", userId, DateTimeOffset.UtcNow.AddMinutes(-1))
        };

        _mockEventQueue.Setup(q => q.EnqueueAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act & Assert
        foreach (var @event in events)
        {
            var result = await _service.IngestEventAsync(@event);
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBe(@event);
        }

        _mockEventQueue.Verify(q => q.EnqueueAsync(It.IsAny<Event>()), Times.Exactly(5));
    }

    [Fact]
    public async Task IngestEvent_WithComplexNestedAttributes_ShouldSucceed()
    {
        // Arrange
        var @event = new Event(
            "complex-event-1",
            "USER_PURCHASED_PRODUCT",
            "user-505",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                { "productId", "prod-456" },
                { "source", "special_offers" },
                { "amount", 99.99m },
                { "currency", "EUR" },
                { "metadata", new Dictionary<string, object>
                    {
                        { "campaign", "summer_sale" },
                        { "discount", 0.15 },
                        { "tags", new[] { "electronics", "premium", "sale" } }
                    }
                },
                { "shipping", new Dictionary<string, object>
                    {
                        { "method", "express" },
                        { "cost", 9.99m },
                        { "estimatedDays", 2 }
                    }
                }
            }
        );

        _mockEventQueue.Setup(q => q.EnqueueAsync(@event))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act
        var result = await _service.IngestEventAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.EventType.ShouldBe("USER_PURCHASED_PRODUCT");
        result.Value!.Attributes["productId"].ShouldBe("prod-456");
        result.Value!.Attributes["source"].ShouldBe("special_offers");
        result.Value!.Attributes["amount"].ShouldBe(99.99m);
        result.Value!.Attributes["currency"].ShouldBe("EUR");

        var metadata = result.Value!.Attributes["metadata"] as Dictionary<string, object>;
        metadata.ShouldNotBeNull();
        metadata!["campaign"].ShouldBe("summer_sale");
        metadata!["discount"].ShouldBe(0.15);
        metadata!["tags"].ShouldBe(new[] { "electronics", "premium", "sale" });

        var shipping = result.Value!.Attributes["shipping"] as Dictionary<string, object>;
        shipping.ShouldNotBeNull();
        shipping!["method"].ShouldBe("express");
        shipping!["cost"].ShouldBe(9.99m);
        shipping!["estimatedDays"].ShouldBe(2);

        _mockEventQueue.Verify(q => q.EnqueueAsync(@event), Times.Once);
    }

    [Fact]
    public async Task IngestEvent_WithEventsAtDifferentTimes_ShouldPreserveTimestamps()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var events = new[]
        {
            new Event("time-event-1", "USER_COMMENTED", "user-time-1", baseTime.AddHours(-24)),
            new Event("time-event-2", "PROFILE_COMPLETED", "user-time-2", baseTime.AddHours(-12)),
            new Event("time-event-3", "USER_VISITED_SPECIAL_OFFERS", "user-time-3", baseTime.AddHours(-6)),
            new Event("time-event-4", "USER_PURCHASED_PRODUCT", "user-time-4", baseTime.AddHours(-1)),
            new Event("time-event-5", "BADGE_GRANTED", "user-time-5", baseTime)
        };

        _mockEventQueue.Setup(q => q.EnqueueAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act & Assert
        for (int i = 0; i < events.Length; i++)
        {
            var result = await _service.IngestEventAsync(events[i]);
            result.IsSuccess.ShouldBeTrue();
            result.Value!.OccurredAt.ShouldBe(events[i].OccurredAt);
        }

        _mockEventQueue.Verify(q => q.EnqueueAsync(It.IsAny<Event>()), Times.Exactly(5));
    }

    [Fact]
    public async Task IngestEvent_WithEventsFromMultipleUsers_ShouldHandleCorrectly()
    {
        // Arrange
        var users = new[] { "user-a", "user-b", "user-c", "user-d", "user-e" };
        var eventTypes = new[] { "USER_COMMENTED", "PROFILE_COMPLETED", "USER_VISITED_SPECIAL_OFFERS", "USER_PURCHASED_PRODUCT", "BADGE_GRANTED" };
        var events = new List<Event>();

        for (int i = 0; i < users.Length; i++)
        {
            events.Add(new Event(
                $"multi-user-event-{i}",
                eventTypes[i],
                users[i],
                DateTimeOffset.UtcNow.AddMinutes(-i)
            ));
        }

        _mockEventQueue.Setup(q => q.EnqueueAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act & Assert
        foreach (var @event in events)
        {
            var result = await _service.IngestEventAsync(@event);
            result.IsSuccess.ShouldBeTrue();
            result.Value!.UserId.ShouldBe(@event.UserId);
            result.Value!.EventType.ShouldBe(@event.EventType);
        }

        _mockEventQueue.Verify(q => q.EnqueueAsync(It.IsAny<Event>()), Times.Exactly(5));
    }
}