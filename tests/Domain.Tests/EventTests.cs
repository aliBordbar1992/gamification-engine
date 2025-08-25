using System.Text.Json;
using GamificationEngine.Domain.Events;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests;

public class EventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var eventId = "test-event-1";
        var eventType = "USER_COMMENTED";
        var userId = "user-123";
        var occurredAt = DateTimeOffset.UtcNow;
        var attributes = new Dictionary<string, object>
        {
            { "commentId", "comment-456" },
            { "postId", "post-789" },
            { "text", "Great post!" }
        };

        // Act
        var @event = new Event(eventId, eventType, userId, occurredAt, attributes);

        // Assert
        @event.EventId.ShouldBe(eventId);
        @event.EventType.ShouldBe(eventType);
        @event.UserId.ShouldBe(userId);
        @event.OccurredAt.ShouldBe(occurredAt);
        @event.Attributes.ShouldNotBeNull();
        @event.Attributes.Count.ShouldBe(3);
        @event.Attributes["commentId"].ShouldBe("comment-456");
        @event.Attributes["postId"].ShouldBe("post-789");
        @event.Attributes["text"].ShouldBe("Great post!");
    }

    [Fact]
    public void Constructor_WithNullAttributes_ShouldCreateEventWithEmptyAttributes()
    {
        // Arrange
        var eventId = "test-event-2";
        var eventType = "PROFILE_COMPLETED";
        var userId = "user-456";
        var occurredAt = DateTimeOffset.UtcNow;

        // Act
        var @event = new Event(eventId, eventType, userId, occurredAt, null);

        // Assert
        @event.Attributes.ShouldNotBeNull();
        @event.Attributes.Count.ShouldBe(0);
    }

    [Fact]
    public void Constructor_WithEmptyAttributes_ShouldCreateEventWithEmptyAttributes()
    {
        // Arrange
        var eventId = "test-event-3";
        var eventType = "USER_VISITED_SPECIAL_OFFERS";
        var userId = "user-789";
        var occurredAt = DateTimeOffset.UtcNow;
        var attributes = new Dictionary<string, object>();

        // Act
        var @event = new Event(eventId, eventType, userId, occurredAt, attributes);

        // Assert
        @event.Attributes.ShouldNotBeNull();
        @event.Attributes.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData("", "USER_COMMENTED", "user-123", "eventId cannot be empty")]
    [InlineData("test-event", "", "user-123", "eventType cannot be empty")]
    [InlineData("test-event", "USER_COMMENTED", "", "userId cannot be empty")]
    [InlineData(null, "USER_COMMENTED", "user-123", "eventId cannot be empty")]
    [InlineData("test-event", null, "user-123", "eventType cannot be empty")]
    [InlineData("test-event", "USER_COMMENTED", null, "userId cannot be empty")]
    public void Constructor_WithInvalidParameters_ShouldThrowArgumentException(string eventId, string eventType, string userId, string expectedMessage)
    {
        // Arrange
        var occurredAt = DateTimeOffset.UtcNow;

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            new Event(eventId, eventType, userId, occurredAt));

        exception.Message.ShouldContain(expectedMessage);
    }

    [Fact]
    public void Constructor_WithComplexAttributes_ShouldHandleAllDataTypes()
    {
        // Arrange
        var eventId = "test-event-4";
        var eventType = "USER_PURCHASED_PRODUCT";
        var userId = "user-101";
        var occurredAt = DateTimeOffset.UtcNow;
        var attributes = new Dictionary<string, object>
        {
            { "productId", "prod-123" },
            { "source", "special_offers" },
            { "amount", 29.99m },
            { "currency", "USD" },
            { "isFirstPurchase", true },
            { "tags", new[] { "electronics", "sale" } },
            { "metadata", new Dictionary<string, object> { { "campaign", "summer_sale" } } }
        };

        // Act
        var @event = new Event(eventId, eventType, userId, occurredAt, attributes);

        // Assert
        @event.Attributes.Count.ShouldBe(7);
        @event.Attributes["productId"].ShouldBe("prod-123");
        @event.Attributes["source"].ShouldBe("special_offers");
        @event.Attributes["amount"].ShouldBe(29.99m);
        @event.Attributes["currency"].ShouldBe("USD");
        @event.Attributes["isFirstPurchase"].ShouldBe(true);
        @event.Attributes["tags"].ShouldBe(new[] { "electronics", "sale" });
        @event.Attributes["metadata"].ShouldBeOfType<Dictionary<string, object>>();
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInIds_ShouldHandleCorrectly()
    {
        // Arrange
        var eventId = "event-123_456-789";
        var eventType = "USER_RECEIVED_DISLIKE";
        var userId = "user-abc_123-xyz";
        var occurredAt = DateTimeOffset.UtcNow;

        // Act
        var @event = new Event(eventId, eventType, userId, occurredAt);

        // Assert
        @event.EventId.ShouldBe("event-123_456-789");
        @event.EventType.ShouldBe("USER_RECEIVED_DISLIKE");
        @event.UserId.ShouldBe("user-abc_123-xyz");
    }

    [Fact]
    public void Constructor_WithUtcAndLocalTime_ShouldPreserveTimeZone()
    {
        // Arrange
        var eventId = "test-event-5";
        var eventType = "DISPUTE_CREATED";
        var userId = "user-202";
        var utcTime = DateTimeOffset.UtcNow;
        var localTime = DateTimeOffset.Now;

        // Act
        var utcEvent = new Event(eventId, eventType, userId, utcTime);
        var localEvent = new Event(eventId + "-local", eventType, userId, localTime);

        // Assert
        utcEvent.OccurredAt.Offset.ShouldBe(TimeSpan.Zero);
        localEvent.OccurredAt.Offset.ShouldNotBe(TimeSpan.Zero);
        utcEvent.OccurredAt.ShouldBe(utcTime);
        localEvent.OccurredAt.ShouldBe(localTime);
    }

    [Fact]
    public void Attributes_ShouldBeReadOnly()
    {
        // Arrange
        var @event = new Event("test-event-6", "BADGE_GRANTED", "user-303", DateTimeOffset.UtcNow);

        // Act & Assert
        @event.Attributes.ShouldBeAssignableTo<IReadOnlyDictionary<string, object>>();
    }

    [Fact]
    public void Attributes_ShouldBeImmutable()
    {
        // Arrange
        var originalAttributes = new Dictionary<string, object> { { "key1", "value1" } };
        var @event = new Event("test-event-7", "PROFILE_COMPLETED", "user-404", DateTimeOffset.UtcNow, originalAttributes);

        // Act
        var attributes = @event.Attributes;
        originalAttributes["key1"] = "modified";

        // Assert
        attributes["key1"].ShouldBe("value1");
        @event.Attributes["key1"].ShouldBe("value1");
    }

    [Fact]
    public void Event_ShouldSupportAllEventTypesFromConfiguration()
    {
        // Arrange
        var eventTypes = new[]
        {
            "USER_COMMENTED",
            "PROFILE_COMPLETED",
            "USER_VISITED_SPECIAL_OFFERS",
            "USER_PURCHASED_PRODUCT",
            "USER_RECEIVED_DISLIKE",
            "DISPUTE_CREATED",
            "BADGE_GRANTED"
        };

        // Act & Assert
        foreach (var eventType in eventTypes)
        {
            var @event = new Event($"test-{eventType}", eventType, "user-505", DateTimeOffset.UtcNow);
            @event.EventType.ShouldBe(eventType);
        }
    }

    [Fact]
    public void Event_ShouldHandleLargeAttributeSets()
    {
        // Arrange
        var eventId = "test-event-8";
        var eventType = "USER_PURCHASED_PRODUCT";
        var userId = "user-606";
        var occurredAt = DateTimeOffset.UtcNow;
        var attributes = new Dictionary<string, object>();

        // Create a large set of attributes
        for (int i = 0; i < 100; i++)
        {
            attributes[$"attribute_{i}"] = $"value_{i}";
        }

        // Act
        var @event = new Event(eventId, eventType, userId, occurredAt, attributes);

        // Assert
        @event.Attributes.Count.ShouldBe(100);
        @event.Attributes["attribute_0"].ShouldBe("value_0");
        @event.Attributes["attribute_99"].ShouldBe("value_99");
    }
}