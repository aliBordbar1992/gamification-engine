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

public class EventIngestionMultipleEventTypesIntegrationTests
{
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<IEventQueue> _mockEventQueue;
    private readonly Mock<IEventValidationService> _mockEventValidationService;
    private readonly EventIngestionService _service;

    public EventIngestionMultipleEventTypesIntegrationTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockEventQueue = new Mock<IEventQueue>();
        _mockEventValidationService = new Mock<IEventValidationService>();
        _service = new EventIngestionService(_mockEventRepository.Object, _mockEventQueue.Object, _mockEventValidationService.Object);

        // Setup validation service to return true by default
        _mockEventValidationService.Setup(v => v.ValidateEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task IngestEvent_CompleteUserJourney_ShouldProcessAllEventTypes()
    {
        // Arrange - Simulate a complete user journey with multiple event types
        var userId = "user-journey-123";
        var sessionId = "session-journey-abc";
        var baseTime = DateTimeOffset.UtcNow;

        var userJourneyEvents = new[]
        {
            // 1. User visits special offers page
            new Event(
                "journey-1",
                "USER_VISITED_SPECIAL_OFFERS",
                userId,
                baseTime.AddMinutes(-30),
                new Dictionary<string, object>
                {
                    { "sessionId", sessionId },
                    { "referrer", "email_campaign" }
                }
            ),
            
            // 2. User completes profile
            new Event(
                "journey-2",
                "PROFILE_COMPLETED",
                userId,
                baseTime.AddMinutes(-25),
                new Dictionary<string, object>
                {
                    { "completenessPercent", 95.0 },
                    { "completedFields", new[] { "name", "email", "phone", "address", "preferences" } }
                }
            ),
            
            // 3. User posts first comment
            new Event(
                "journey-3",
                "USER_COMMENTED",
                userId,
                baseTime.AddMinutes(-20),
                new Dictionary<string, object>
                {
                    { "commentId", "comment-journey-456" },
                    { "postId", "post-journey-789" },
                    { "text", "This is my first comment!" },
                    { "isFirstComment", true }
                }
            ),
            
            // 4. User makes a purchase from special offers
            new Event(
                "journey-4",
                "USER_PURCHASED_PRODUCT",
                userId,
                baseTime.AddMinutes(-15),
                new Dictionary<string, object>
                {
                    { "productId", "prod-journey-123" },
                    { "source", "special_offers" },
                    { "amount", 79.99m },
                    { "currency", "USD" },
                    { "sessionId", sessionId },
                    { "timeSinceVisit", 15 },
                    { "isFirstPurchase", true }
                }
            ),
            
            // 5. User receives a badge for first comment
            new Event(
                "journey-5",
                "BADGE_GRANTED",
                userId,
                baseTime.AddMinutes(-10),
                new Dictionary<string, object>
                {
                    { "badgeId", "badge-first-commenter" },
                    { "badgeName", "First Comment" },
                    { "triggeredBy", "USER_COMMENTED" }
                }
            ),
            
            // 6. User receives a badge for special offer purchase
            new Event(
                "journey-6",
                "BADGE_GRANTED",
                userId,
                baseTime.AddMinutes(-5),
                new Dictionary<string, object>
                {
                    { "badgeId", "badge-special-offer-buyer" },
                    { "badgeName", "Special Offer Buyer" },
                    { "triggeredBy", "USER_PURCHASED_PRODUCT" },
                    { "timeSinceVisit", 15 }
                }
            )
        };

        _mockEventQueue.Setup(q => q.EnqueueAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act - Process all events in sequence
        var results = new List<Result<Event, DomainError>>();
        foreach (var @event in userJourneyEvents)
        {
            var result = await _service.IngestEventAsync(@event);
            results.Add(result);
        }

        // Assert - All events should be processed successfully
        results.Count.ShouldBe(6);
        results.All(r => r.IsSuccess).ShouldBeTrue();

        // Verify each event type was processed correctly
        var processedEvents = results.Select(r => r.Value).ToList();

        // Event 1: USER_VISITED_SPECIAL_OFFERS
        var visitEvent = processedEvents[0];
        visitEvent.EventType.ShouldBe("USER_VISITED_SPECIAL_OFFERS");
        visitEvent.Attributes["sessionId"].ShouldBe(sessionId);
        visitEvent.Attributes["referrer"].ShouldBe("email_campaign");

        // Event 2: PROFILE_COMPLETED
        var profileEvent = processedEvents[1];
        profileEvent.EventType.ShouldBe("PROFILE_COMPLETED");
        profileEvent.Attributes["completenessPercent"].ShouldBe(95.0);
        profileEvent.Attributes["completedFields"].ShouldBe(new[] { "name", "email", "phone", "address", "preferences" });

        // Event 3: USER_COMMENTED
        var commentEvent = processedEvents[2];
        commentEvent.EventType.ShouldBe("USER_COMMENTED");
        commentEvent.Attributes["commentId"].ShouldBe("comment-journey-456");
        commentEvent.Attributes["isFirstComment"].ShouldBe(true);

        // Event 4: USER_PURCHASED_PRODUCT
        var purchaseEvent = processedEvents[3];
        purchaseEvent.EventType.ShouldBe("USER_PURCHASED_PRODUCT");
        purchaseEvent.Attributes["source"].ShouldBe("special_offers");
        purchaseEvent.Attributes["amount"].ShouldBe(79.99m);
        purchaseEvent.Attributes["sessionId"].ShouldBe(sessionId);
        purchaseEvent.Attributes["timeSinceVisit"].ShouldBe(15);

        // Event 5: BADGE_GRANTED (first comment)
        var badge1Event = processedEvents[4];
        badge1Event.EventType.ShouldBe("BADGE_GRANTED");
        badge1Event.Attributes["badgeId"].ShouldBe("badge-first-commenter");
        badge1Event.Attributes["triggeredBy"].ShouldBe("USER_COMMENTED");

        // Event 6: BADGE_GRANTED (special offer)
        var badge2Event = processedEvents[5];
        badge2Event.EventType.ShouldBe("BADGE_GRANTED");
        badge2Event.Attributes["badgeId"].ShouldBe("badge-special-offer-buyer");
        badge2Event.Attributes["triggeredBy"].ShouldBe("USER_PURCHASED_PRODUCT");

        // Verify all events were enqueued
        _mockEventQueue.Verify(q => q.EnqueueAsync(It.IsAny<Event>()), Times.Exactly(6));
    }

    [Fact]
    public async Task IngestEvent_MultipleUsersSameEventTypes_ShouldHandleCorrectly()
    {
        // Arrange - Multiple users performing similar actions
        var users = new[] { "user-multi-1", "user-multi-2", "user-multi-3" };
        var eventTypes = new[] { "USER_COMMENTED", "PROFILE_COMPLETED", "USER_PURCHASED_PRODUCT" };
        var baseTime = DateTimeOffset.UtcNow;

        var allEvents = new List<Event>();
        var eventCounter = 1;

        foreach (var user in users)
        {
            foreach (var eventType in eventTypes)
            {
                var attributes = eventType switch
                {
                    "USER_COMMENTED" => new Dictionary<string, object>
                    {
                        { "commentId", $"comment-{user}-{eventCounter}" },
                        { "postId", $"post-{user}-{eventCounter}" },
                        { "text", $"Comment from {user}" }
                    },
                    "PROFILE_COMPLETED" => new Dictionary<string, object>
                    {
                        { "completenessPercent", 85.0 + (eventCounter * 5) }
                    },
                    "USER_PURCHASED_PRODUCT" => new Dictionary<string, object>
                    {
                        { "productId", $"prod-{user}-{eventCounter}" },
                        { "amount", 10.0m + (eventCounter * 5) },
                        { "currency", "USD" }
                    },
                    _ => new Dictionary<string, object>()
                };

                allEvents.Add(new Event(
                    $"multi-{user}-{eventType}-{eventCounter}",
                    eventType,
                    user,
                    baseTime.AddMinutes(-eventCounter),
                    attributes
                ));

                eventCounter++;
            }
        }

        _mockEventQueue.Setup(q => q.EnqueueAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act - Process all events
        var results = new List<Result<Event, DomainError>>();
        foreach (var @event in allEvents)
        {
            var result = await _service.IngestEventAsync(@event);
            results.Add(result);
        }

        // Assert - All events should be processed successfully
        results.Count.ShouldBe(9); // 3 users × 3 event types
        results.All(r => r.IsSuccess).ShouldBeTrue();

        // Verify events are grouped correctly by user and event type
        var processedEvents = results.Select(r => r.Value).ToList();

        // Group by user
        var eventsByUser = processedEvents.GroupBy(e => e.UserId).ToDictionary(g => g.Key, g => g.ToList());
        eventsByUser.Count.ShouldBe(3);

        foreach (var user in users)
        {
            eventsByUser[user].Count.ShouldBe(3);
            eventsByUser[user].Select(e => e.EventType).Distinct().Count().ShouldBe(3);
        }

        // Group by event type
        var eventsByType = processedEvents.GroupBy(e => e.EventType).ToDictionary(g => g.Key, g => g.ToList());
        eventsByType.Count.ShouldBe(3);

        foreach (var eventType in eventTypes)
        {
            eventsByType[eventType].Count.ShouldBe(3);
            eventsByType[eventType].Select(e => e.UserId).Distinct().Count().ShouldBe(3);
        }

        // Verify all events were enqueued
        _mockEventQueue.Verify(q => q.EnqueueAsync(It.IsAny<Event>()), Times.Exactly(9));
    }

    [Fact]
    public async Task IngestEvent_ComplexEventSequence_ShouldMaintainOrderAndRelationships()
    {
        // Arrange - Complex sequence of related events
        var userId = "user-complex-456";
        var sessionId = "session-complex-def";
        var productId = "prod-complex-789";
        var baseTime = DateTimeOffset.UtcNow;

        var complexSequence = new[]
        {
            // 1. User starts session
            new Event(
                "complex-1",
                "USER_VISITED_SPECIAL_OFFERS",
                userId,
                baseTime.AddMinutes(-60),
                new Dictionary<string, object>
                {
                    { "sessionId", sessionId },
                    { "entryPoint", "homepage_banner" },
                    { "deviceType", "mobile" }
                }
            ),
            
            // 2. User browses products
            new Event(
                "complex-2",
                "USER_PURCHASED_PRODUCT",
                userId,
                baseTime.AddMinutes(-45),
                new Dictionary<string, object>
                {
                    { "productId", productId },
                    { "source", "special_offers" },
                    { "amount", 29.99m },
                    { "currency", "USD" },
                    { "sessionId", sessionId },
                    { "browsingTime", 15 },
                    { "relatedProducts", new[] { "prod-related-1", "prod-related-2" } }
                }
            ),
            
            // 3. User receives immediate reward
            new Event(
                "complex-3",
                "BADGE_GRANTED",
                userId,
                baseTime.AddMinutes(-44),
                new Dictionary<string, object>
                {
                    { "badgeId", "badge-quick-buyer" },
                    { "triggeredBy", "USER_PURCHASED_PRODUCT" },
                    { "timeSinceVisit", 16 },
                    { "productId", productId }
                }
            ),
            
            // 4. User comments on the product
            new Event(
                "complex-4",
                "USER_COMMENTED",
                userId,
                baseTime.AddMinutes(-30),
                new Dictionary<string, object>
                {
                    { "commentId", "comment-complex-123" },
                    { "postId", productId },
                    { "text", "Great product, fast delivery!" },
                    { "rating", 5 },
                    { "sessionId", sessionId }
                }
            ),
            
            // 5. User receives comment reward
            new Event(
                "complex-5",
                "BADGE_GRANTED",
                userId,
                baseTime.AddMinutes(-29),
                new Dictionary<string, object>
                {
                    { "badgeId", "badge-reviewer" },
                    { "triggeredBy", "USER_COMMENTED" },
                    { "productId", productId },
                    { "rating", 5 }
                }
            ),
            
            // 6. User completes profile to unlock more features
            new Event(
                "complex-6",
                "PROFILE_COMPLETED",
                userId,
                baseTime.AddMinutes(-20),
                new Dictionary<string, object>
                {
                    { "completenessPercent", 100.0 },
                    { "unlockedFeatures", new[] { "advanced_search", "wishlist", "notifications" } },
                    { "sessionId", sessionId }
                }
            ),
            
            // 7. User receives profile completion reward
            new Event(
                "complex-7",
                "BADGE_GRANTED",
                userId,
                baseTime.AddMinutes(-19),
                new Dictionary<string, object>
                {
                    { "badgeId", "badge-profile-master" },
                    { "triggeredBy", "PROFILE_COMPLETED" },
                    { "completenessPercent", 100.0 }
                }
            )
        };

        _mockEventQueue.Setup(q => q.EnqueueAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act - Process complex sequence
        var results = new List<Result<Event, DomainError>>();
        foreach (var @event in complexSequence)
        {
            var result = await _service.IngestEventAsync(@event);
            results.Add(result);
        }

        // Assert - All events should be processed successfully
        results.Count.ShouldBe(7);
        results.All(r => r.IsSuccess).ShouldBeTrue();

        // Verify temporal relationships are maintained
        var processedEvents = results.Select(r => r.Value).OrderBy(e => e.OccurredAt).ToList();

        // Check that events are in chronological order
        for (int i = 1; i < processedEvents.Count; i++)
        {
            processedEvents[i].OccurredAt.ShouldBeGreaterThan(processedEvents[i - 1].OccurredAt);
        }

        // Verify session consistency across related events
        var sessionEvents = processedEvents.Where(e => e.Attributes.ContainsKey("sessionId")).ToList();
        sessionEvents.All(e => e.Attributes["sessionId"].ToString() == sessionId).ShouldBeTrue();

        // Verify product relationship consistency
        var productEvents = processedEvents.Where(e => e.Attributes.ContainsKey("productId")).ToList();
        productEvents.All(e => e.Attributes["productId"].ToString() == productId).ShouldBeTrue();

        // Verify badge relationships
        var badgeEvents = processedEvents.Where(e => e.EventType == "BADGE_GRANTED").ToList();
        badgeEvents.Count.ShouldBe(3);
        badgeEvents.ShouldContain(e => e.Attributes["badgeId"].ToString() == "badge-quick-buyer");
        badgeEvents.ShouldContain(e => e.Attributes["badgeId"].ToString() == "badge-reviewer");
        badgeEvents.ShouldContain(e => e.Attributes["badgeId"].ToString() == "badge-profile-master");

        // Verify all events were enqueued
        _mockEventQueue.Verify(q => q.EnqueueAsync(It.IsAny<Event>()), Times.Exactly(7));
    }

    [Fact]
    public async Task IngestEvent_ErrorHandling_ShouldHandleFailuresGracefully()
    {
        // Arrange - Mix of successful and failing events
        var userId = "user-error-789";
        var baseTime = DateTimeOffset.UtcNow;

        var mixedEvents = new[]
        {
            // Successful event
            new Event(
                "error-1",
                "USER_COMMENTED",
                userId,
                baseTime.AddMinutes(-10),
                new Dictionary<string, object>
                {
                    { "commentId", "comment-error-123" }
                }
            ),
            
            // Event that will fail
            new Event(
                "error-2",
                "PROFILE_COMPLETED",
                userId,
                baseTime.AddMinutes(-5),
                new Dictionary<string, object>
                {
                    { "completenessPercent", 100 }
                }
            ),
            
            // Another successful event
            new Event(
                "error-3",
                "USER_PURCHASED_PRODUCT",
                userId,
                baseTime,
                new Dictionary<string, object>
                {
                    { "productId", "prod-error-456" },
                    { "amount", 19.99m }
                }
            )
        };

        // Setup mixed success/failure responses
        _mockEventQueue.Setup(q => q.EnqueueAsync(mixedEvents[0]))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));
        _mockEventQueue.Setup(q => q.EnqueueAsync(mixedEvents[1]))
            .ReturnsAsync(Result<bool, DomainError>.Failure(new EventStorageError("Queue is full")));
        _mockEventQueue.Setup(q => q.EnqueueAsync(mixedEvents[2]))
            .ReturnsAsync(Result<bool, DomainError>.Success(true));

        // Act - Process mixed events
        var results = new List<Result<Event, DomainError>>();
        foreach (var @event in mixedEvents)
        {
            var result = await _service.IngestEventAsync(@event);
            results.Add(result);
        }

        // Assert - Mixed results
        results.Count.ShouldBe(3);
        results[0].IsSuccess.ShouldBeTrue(); // First event succeeds
        results[1].IsSuccess.ShouldBeFalse(); // Second event fails
        results[2].IsSuccess.ShouldBeTrue(); // Third event succeeds

        // Verify error details
        results[1].Error.ShouldNotBeNull();
        results[1].Error!.Code.ShouldBe("EVENT_STORAGE_ERROR");
        results[1].Error!.Message.ShouldContain("Queue is full");

        // Verify successful events were processed
        results[0].Value!.EventType.ShouldBe("USER_COMMENTED");
        results[2].Value!.EventType.ShouldBe("USER_PURCHASED_PRODUCT");

        // Verify queue was called for all events
        _mockEventQueue.Verify(q => q.EnqueueAsync(It.IsAny<Event>()), Times.Exactly(3));
    }
}