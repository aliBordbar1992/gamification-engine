using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Rules.Conditions;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rules;

public class ConditionTests
{
    private readonly Event _triggerEvent;
    private readonly List<Event> _events;

    public ConditionTests()
    {
        _triggerEvent = new Event("trigger-1", "USER_COMMENT", "user-123", DateTimeOffset.UtcNow,
            new Dictionary<string, object> { { "rating", 5 }, { "category", "review" } });

        _events = new List<Event>
        {
            new Event("event-1", "USER_LOGIN", "user-123", DateTimeOffset.UtcNow.AddMinutes(-30)),
            new Event("event-2", "USER_COMMENT", "user-123", DateTimeOffset.UtcNow.AddMinutes(-15)),
            new Event("event-3", "USER_PURCHASE", "user-123", DateTimeOffset.UtcNow.AddMinutes(-10),
                new Dictionary<string, object> { { "amount", 100.50m } }),
            _triggerEvent
        };
    }

    [Fact]
    public void AlwaysTrueCondition_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var condition = new AlwaysTrueCondition("always-true-1");

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AttributeEqualsCondition_WithMatchingAttribute_ShouldReturnTrue()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "attributeName", "rating" },
            { "expectedValue", 5 }
        };
        var condition = new AttributeEqualsCondition("attr-equals-1", parameters);

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void AttributeEqualsCondition_WithNonMatchingAttribute_ShouldReturnFalse()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "attributeName", "rating" },
            { "expectedValue", 3 }
        };
        var condition = new AttributeEqualsCondition("attr-equals-2", parameters);

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AttributeEqualsCondition_WithMissingAttribute_ShouldReturnFalse()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "attributeName", "nonexistent" },
            { "expectedValue", "value" }
        };
        var condition = new AttributeEqualsCondition("attr-equals-3", parameters);

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CountCondition_WithSufficientEvents_ShouldReturnTrue()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "eventType", "USER_COMMENT" },
            { "minCount", 2 }
        };
        var condition = new CountCondition("count-1", parameters);

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CountCondition_WithInsufficientEvents_ShouldReturnFalse()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "eventType", "USER_PURCHASE" },
            { "minCount", 5 }
        };
        var condition = new CountCondition("count-2", parameters);

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CountCondition_WithTimeWindow_ShouldRespectTimeLimit()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "eventType", "USER_LOGIN" },
            { "minCount", 1 },
            { "timeWindowMinutes", 20 } // Only 20 minutes, login was 30 minutes ago
        };
        var condition = new CountCondition("count-3", parameters);

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ThresholdCondition_WithGreaterThanOrEqual_ShouldReturnTrue()
    {
        // Arrange
        var triggerEventWithAmount = new Event("trigger-2", "USER_PURCHASE", "user-123", DateTimeOffset.UtcNow,
            new Dictionary<string, object> { { "amount", 150.75m } });

        var parameters = new Dictionary<string, object>
        {
            { "attributeName", "amount" },
            { "threshold", 100m },
            { "operation", ">=" }
        };
        var condition = new ThresholdCondition("threshold-1", parameters);

        // Act
        var result = condition.Evaluate(_events, triggerEventWithAmount);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ThresholdCondition_WithLessThan_ShouldReturnFalse()
    {
        // Arrange
        var triggerEventWithAmount = new Event("trigger-3", "USER_PURCHASE", "user-123", DateTimeOffset.UtcNow,
            new Dictionary<string, object> { { "amount", 50m } });

        var parameters = new Dictionary<string, object>
        {
            { "attributeName", "amount" },
            { "threshold", 100m },
            { "operation", ">=" }
        };
        var condition = new ThresholdCondition("threshold-2", parameters);

        // Act
        var result = condition.Evaluate(_events, triggerEventWithAmount);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void SequenceCondition_WithValidSequence_ShouldReturnTrue()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "eventTypes", new List<object> { "USER_LOGIN", "USER_COMMENT" } },
            { "timeWindowMinutes", 60 }
        };
        var condition = new SequenceCondition("sequence-1", parameters);

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void SequenceCondition_WithInvalidSequence_ShouldReturnFalse()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "eventTypes", new List<object> { "USER_PURCHASE", "USER_LOGIN" } }, // Wrong order
            { "timeWindowMinutes", 60 }
        };
        var condition = new SequenceCondition("sequence-2", parameters);

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void TimeSinceLastEventCondition_WithSufficientTime_ShouldReturnTrue()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "eventType", "USER_COMMENT" },
            { "minMinutes", 10 } // Last comment was 15 minutes ago
        };
        var condition = new TimeSinceLastEventCondition("time-since-1", parameters);

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void TimeSinceLastEventCondition_WithInsufficientTime_ShouldReturnFalse()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "eventType", "USER_COMMENT" },
            { "minMinutes", 20 } // Last comment was only 15 minutes ago
        };
        var condition = new TimeSinceLastEventCondition("time-since-2", parameters);

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void FirstOccurrenceCondition_WithFirstEvent_ShouldReturnTrue()
    {
        // Arrange
        var firstLoginEvent = new Event("first-login", "USER_LOGIN", "user-456", DateTimeOffset.UtcNow);
        var condition = new FirstOccurrenceCondition("first-occurrence-1");

        // Act
        var result = condition.Evaluate(new List<Event>(), firstLoginEvent);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void FirstOccurrenceCondition_WithSubsequentEvent_ShouldReturnFalse()
    {
        // Arrange
        var condition = new FirstOccurrenceCondition("first-occurrence-2");

        // Act
        var result = condition.Evaluate(_events, _triggerEvent);

        // Assert
        result.ShouldBeFalse(); // There's already a USER_COMMENT event before the trigger
    }
}
