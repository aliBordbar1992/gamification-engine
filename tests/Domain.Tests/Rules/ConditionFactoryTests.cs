using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Rules.Conditions;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rules;

public class ConditionFactoryTests
{
    private readonly ConditionFactory _factory;

    public ConditionFactoryTests()
    {
        _factory = new ConditionFactory();
    }

    [Fact]
    public void CreateCondition_WithAlwaysTrueType_ShouldReturnAlwaysTrueCondition()
    {
        // Act
        var condition = _factory.CreateCondition("test-1", ConditionTypes.AlwaysTrue);

        // Assert
        condition.ShouldBeOfType<AlwaysTrueCondition>();
        condition.ConditionId.ShouldBe("test-1");
        condition.Type.ShouldBe(ConditionTypes.AlwaysTrue);
    }

    [Fact]
    public void CreateCondition_WithAttributeEqualsType_ShouldReturnAttributeEqualsCondition()
    {
        // Act
        var condition = _factory.CreateCondition("test-2", ConditionTypes.AttributeEquals);

        // Assert
        condition.ShouldBeOfType<AttributeEqualsCondition>();
        condition.ConditionId.ShouldBe("test-2");
        condition.Type.ShouldBe(ConditionTypes.AttributeEquals);
    }

    [Fact]
    public void CreateCondition_WithCountType_ShouldReturnCountCondition()
    {
        // Act
        var condition = _factory.CreateCondition("test-3", ConditionTypes.Count);

        // Assert
        condition.ShouldBeOfType<CountCondition>();
        condition.ConditionId.ShouldBe("test-3");
        condition.Type.ShouldBe(ConditionTypes.Count);
    }

    [Fact]
    public void CreateCondition_WithThresholdType_ShouldReturnThresholdCondition()
    {
        // Act
        var condition = _factory.CreateCondition("test-4", ConditionTypes.Threshold);

        // Assert
        condition.ShouldBeOfType<ThresholdCondition>();
        condition.ConditionId.ShouldBe("test-4");
        condition.Type.ShouldBe(ConditionTypes.Threshold);
    }

    [Fact]
    public void CreateCondition_WithSequenceType_ShouldReturnSequenceCondition()
    {
        // Act
        var condition = _factory.CreateCondition("test-5", ConditionTypes.Sequence);

        // Assert
        condition.ShouldBeOfType<SequenceCondition>();
        condition.ConditionId.ShouldBe("test-5");
        condition.Type.ShouldBe(ConditionTypes.Sequence);
    }

    [Fact]
    public void CreateCondition_WithTimeSinceLastEventType_ShouldReturnTimeSinceLastEventCondition()
    {
        // Act
        var condition = _factory.CreateCondition("test-6", ConditionTypes.TimeSinceLastEvent);

        // Assert
        condition.ShouldBeOfType<TimeSinceLastEventCondition>();
        condition.ConditionId.ShouldBe("test-6");
        condition.Type.ShouldBe(ConditionTypes.TimeSinceLastEvent);
    }

    [Fact]
    public void CreateCondition_WithFirstOccurrenceType_ShouldReturnFirstOccurrenceCondition()
    {
        // Act
        var condition = _factory.CreateCondition("test-7", ConditionTypes.FirstOccurrence);

        // Assert
        condition.ShouldBeOfType<FirstOccurrenceCondition>();
        condition.ConditionId.ShouldBe("test-7");
        condition.Type.ShouldBe(ConditionTypes.FirstOccurrence);
    }

    [Fact]
    public void CreateCondition_WithCustomScriptType_ShouldReturnCustomScriptCondition()
    {
        // Act
        var condition = _factory.CreateCondition("test-8", ConditionTypes.CustomScript);

        // Assert
        condition.ShouldBeOfType<CustomScriptCondition>();
        condition.ConditionId.ShouldBe("test-8");
        condition.Type.ShouldBe(ConditionTypes.CustomScript);
    }

    [Fact]
    public void CreateCondition_WithParameters_ShouldSetParameters()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "attributeName", "rating" },
            { "expectedValue", 5 }
        };

        // Act
        var condition = _factory.CreateCondition("test-9", ConditionTypes.AttributeEquals, parameters);

        // Assert
        condition.Parameters.ShouldNotBeNull();
        condition.Parameters["attributeName"].ShouldBe("rating");
        condition.Parameters["expectedValue"].ShouldBe(5);
    }

    [Fact]
    public void CreateCondition_WithUnknownType_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            _factory.CreateCondition("test-10", "unknownType"));
    }
}
