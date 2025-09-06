using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Rules.Conditions;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rules;

public class ConditionEvaluatorTests
{
    private readonly ConditionEvaluator _evaluator;
    private readonly Event _triggerEvent;
    private readonly List<Event> _events;

    public ConditionEvaluatorTests()
    {
        _evaluator = new ConditionEvaluator();
        _triggerEvent = new Event("trigger-1", "USER_COMMENT", "user-123", DateTimeOffset.UtcNow,
            new Dictionary<string, object> { { "rating", 5 } });
        _events = new List<Event> { _triggerEvent };
    }

    [Fact]
    public void EvaluateConditions_WithNoConditions_ShouldReturnTrue()
    {
        // Arrange
        var conditions = new List<Condition>();

        // Act
        var result = _evaluator.EvaluateConditions(conditions, _events, _triggerEvent, "all");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void EvaluateConditions_WithAllLogicAndAllTrue_ShouldReturnTrue()
    {
        // Arrange
        var conditions = new List<Condition>
        {
            new AlwaysTrueCondition("condition-1"),
            new AlwaysTrueCondition("condition-2")
        };

        // Act
        var result = _evaluator.EvaluateConditions(conditions, _events, _triggerEvent, "all");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void EvaluateConditions_WithAllLogicAndOneFalse_ShouldReturnFalse()
    {
        // Arrange
        var conditions = new List<Condition>
        {
            new AlwaysTrueCondition("condition-1"),
            new AttributeEqualsCondition("condition-2", new Dictionary<string, object>
            {
                { "attributeName", "rating" },
                { "expectedValue", 3 } // Trigger event has rating 5
            })
        };

        // Act
        var result = _evaluator.EvaluateConditions(conditions, _events, _triggerEvent, "all");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void EvaluateConditions_WithAnyLogicAndOneTrue_ShouldReturnTrue()
    {
        // Arrange
        var conditions = new List<Condition>
        {
            new AlwaysTrueCondition("condition-1"),
            new AttributeEqualsCondition("condition-2", new Dictionary<string, object>
            {
                { "attributeName", "rating" },
                { "expectedValue", 3 } // This will be false
            })
        };

        // Act
        var result = _evaluator.EvaluateConditions(conditions, _events, _triggerEvent, "any");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void EvaluateConditions_WithAnyLogicAndAllFalse_ShouldReturnFalse()
    {
        // Arrange
        var conditions = new List<Condition>
        {
            new AttributeEqualsCondition("condition-1", new Dictionary<string, object>
            {
                { "attributeName", "rating" },
                { "expectedValue", 3 } // This will be false
            }),
            new AttributeEqualsCondition("condition-2", new Dictionary<string, object>
            {
                { "attributeName", "nonexistent" },
                { "expectedValue", "value" } // This will be false
            })
        };

        // Act
        var result = _evaluator.EvaluateConditions(conditions, _events, _triggerEvent, "any");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void EvaluateConditions_WithInvalidLogic_ShouldThrowArgumentException()
    {
        // Arrange
        var conditions = new List<Condition> { new AlwaysTrueCondition("condition-1") };

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            _evaluator.EvaluateConditions(conditions, _events, _triggerEvent, "invalid"));
    }
}
