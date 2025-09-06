using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Rules.Conditions;
using GamificationEngine.Domain.Rewards;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rules;

public class RuleTests
{
    private readonly Event _triggerEvent;
    private readonly List<Event> _events;
    private readonly List<Condition> _conditions;
    private readonly List<Reward> _rewards;

    public RuleTests()
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

        _conditions = new List<Condition>
        {
            new AlwaysTrueCondition("always-true-1"),
            new AttributeEqualsCondition("attr-equals-1", new Dictionary<string, object>
            {
                { "attributeName", "rating" },
                { "expectedValue", 5 }
            })
        };

        _rewards = new List<Reward>
        {
            new TestReward("reward-1", "POINTS", new Dictionary<string, object> { { "amount", 100 } }),
            new TestReward("reward-2", "BADGE", new Dictionary<string, object> { { "badgeId", "reviewer" } })
        };
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateRule()
    {
        // Arrange
        var singleCondition = new List<Condition> { _conditions[0] };
        var singleReward = new List<Reward> { _rewards[0] };

        // Act
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, singleCondition, singleReward);

        // Assert
        rule.RuleId.ShouldBe("rule-1");
        rule.Name.ShouldBe("Test Rule");
        rule.Triggers.ShouldContain("USER_COMMENT");
        rule.Conditions.ShouldHaveSingleItem();
        rule.Rewards.ShouldHaveSingleItem();
        rule.IsActive.ShouldBeTrue();
        rule.CreatedAt.ShouldNotBe(default);
        rule.UpdatedAt.ShouldNotBe(default);
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldCreateRule()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow;

        // Act
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards,
            isActive: false, description: "Test description", createdAt: createdAt, updatedAt: updatedAt);

        // Assert
        rule.RuleId.ShouldBe("rule-1");
        rule.Name.ShouldBe("Test Rule");
        rule.Description.ShouldBe("Test description");
        rule.IsActive.ShouldBeFalse();
        rule.CreatedAt.ShouldBe(createdAt);
        rule.UpdatedAt.ShouldBe(updatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyRuleId_ShouldThrowArgumentException(string ruleId)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Rule(ruleId!, "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards))
            .Message.ShouldContain("ruleId cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException(string name)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Rule("rule-1", name!, new[] { "USER_COMMENT" }, _conditions, _rewards))
            .Message.ShouldContain("name cannot be empty");
    }

    [Fact]
    public void Constructor_WithEmptyTriggers_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Rule("rule-1", "Test Rule", Array.Empty<string>(), _conditions, _rewards))
            .Message.ShouldContain("triggers cannot be null or empty");
    }

    [Fact]
    public void Constructor_WithNullTriggers_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Rule("rule-1", "Test Rule", null!, _conditions, _rewards))
            .Message.ShouldContain("triggers cannot be null or empty");
    }

    [Fact]
    public void Constructor_WithEmptyConditions_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, new List<Condition>(), _rewards))
            .Message.ShouldContain("conditions cannot be null or empty");
    }

    [Fact]
    public void Constructor_WithNullConditions_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, null!, _rewards))
            .Message.ShouldContain("conditions cannot be null or empty");
    }

    [Fact]
    public void Constructor_WithEmptyRewards_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, new List<Reward>()))
            .Message.ShouldContain("rewards cannot be null or empty");
    }

    [Fact]
    public void Constructor_WithNullRewards_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, null!))
            .Message.ShouldContain("rewards cannot be null or empty");
    }

    [Fact]
    public void ShouldTrigger_WithMatchingEventType_ShouldReturnTrue()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);

        // Act
        var result = rule.ShouldTrigger("USER_COMMENT");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ShouldTrigger_WithCaseInsensitiveMatchingEventType_ShouldReturnTrue()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);

        // Act
        var result = rule.ShouldTrigger("user_comment");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ShouldTrigger_WithNonMatchingEventType_ShouldReturnFalse()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);

        // Act
        var result = rule.ShouldTrigger("USER_LOGIN");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldTrigger_WithEmptyEventType_ShouldReturnFalse()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);

        // Act
        var result = rule.ShouldTrigger("");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldTrigger_WithNullEventType_ShouldReturnFalse()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);

        // Act
        var result = rule.ShouldTrigger(null!);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldTrigger_WithInactiveRule_ShouldReturnFalse()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards, isActive: false);

        // Act
        var result = rule.ShouldTrigger("USER_COMMENT");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void EvaluateConditions_WithAllConditionsMet_ShouldReturnTrue()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);

        // Act
        var result = rule.EvaluateConditions(_events, _triggerEvent);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void EvaluateConditions_WithSomeConditionsNotMet_ShouldReturnFalse()
    {
        // Arrange
        var failingCondition = new AttributeEqualsCondition("attr-equals-fail", new Dictionary<string, object>
        {
            { "attributeName", "rating" },
            { "expectedValue", 1 } // This won't match the trigger event's rating of 5
        });
        var conditions = new List<Condition> { _conditions[0], failingCondition };
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, conditions, _rewards);

        // Act
        var result = rule.EvaluateConditions(_events, _triggerEvent);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void EvaluateConditions_WithInactiveRule_ShouldReturnFalse()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards, isActive: false);

        // Act
        var result = rule.EvaluateConditions(_events, _triggerEvent);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void EvaluateConditions_WithNullEvents_ShouldThrowArgumentNullException()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => rule.EvaluateConditions(null!, _triggerEvent));
    }

    [Fact]
    public void EvaluateConditions_WithNullTriggerEvent_ShouldThrowArgumentNullException()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => rule.EvaluateConditions(_events, null!));
    }

    [Fact]
    public void IsValid_WithValidRule_ShouldReturnTrue()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);

        // Act
        var result = rule.IsValid();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyRuleId_ShouldReturnFalse()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);
        rule.RuleId = "";

        // Act
        var result = rule.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyName_ShouldReturnFalse()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);
        rule.Name = "";

        // Act
        var result = rule.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyTriggers_ShouldReturnFalse()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);
        rule.Triggers = Array.Empty<string>();

        // Act
        var result = rule.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyConditions_ShouldReturnFalse()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);
        rule.Conditions = new List<Condition>();

        // Act
        var result = rule.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyRewards_ShouldReturnFalse()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);
        rule.Rewards = new List<Reward>();

        // Act
        var result = rule.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void GetSummary_ShouldReturnFormattedSummary()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT", "USER_LOGIN" }, _conditions, _rewards);

        // Act
        var summary = rule.GetSummary();

        // Assert
        summary.ShouldContain("Test Rule");
        summary.ShouldContain("rule-1");
        summary.ShouldContain("USER_COMMENT");
        summary.ShouldContain("USER_LOGIN");
        summary.ShouldContain("Conditions: 2");
        summary.ShouldContain("Rewards: 2");
        summary.ShouldContain("Active: True");
    }
}

/// <summary>
/// Test implementation of Reward for testing purposes
/// </summary>
public class TestReward : Reward
{
    public TestReward(string rewardId, string type, IDictionary<string, object>? parameters = null)
        : base(rewardId, type, parameters)
    {
    }
}
