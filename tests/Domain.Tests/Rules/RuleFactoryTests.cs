using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Rules.Conditions;
using GamificationEngine.Domain.Rewards;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rules;

public class RuleFactoryTests
{
    private readonly Func<string, string, IDictionary<string, object>?, Condition> _conditionFactory;
    private readonly Func<string, string, IDictionary<string, object>?, Reward> _rewardFactory;

    public RuleFactoryTests()
    {
        _conditionFactory = (id, type, parameters) => new AlwaysTrueCondition(id);
        _rewardFactory = (id, type, parameters) => new TestReward(id, type, parameters);
    }

    [Fact]
    public void CreateFromConfiguration_WithValidConfiguration_ShouldCreateRule()
    {
        // Arrange
        var config = new RuleConfiguration
        {
            RuleId = "rule-1",
            Name = "Test Rule",
            Triggers = new[] { "USER_COMMENT" },
            Conditions = new[]
            {
                new ConditionConfiguration
                {
                    ConditionId = "condition-1",
                    Type = "alwaysTrue",
                    Parameters = new Dictionary<string, object>()
                }
            },
            Rewards = new[]
            {
                new RewardConfiguration
                {
                    RewardId = "reward-1",
                    Type = "POINTS",
                    Parameters = new Dictionary<string, object> { { "amount", 100 } }
                }
            },
            IsActive = true,
            Description = "Test description"
        };

        // Act
        var rule = RuleFactory.CreateFromConfiguration(config, _conditionFactory, _rewardFactory);

        // Assert
        rule.RuleId.ShouldBe("rule-1");
        rule.Name.ShouldBe("Test Rule");
        rule.Description.ShouldBe("Test description");
        rule.Triggers.ShouldContain("USER_COMMENT");
        rule.Conditions.ShouldHaveSingleItem();
        rule.Rewards.ShouldHaveSingleItem();
        rule.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void CreateFromConfiguration_WithMultipleConditionsAndRewards_ShouldCreateRule()
    {
        // Arrange
        var config = new RuleConfiguration
        {
            RuleId = "rule-1",
            Name = "Test Rule",
            Triggers = new[] { "USER_COMMENT", "USER_LOGIN" },
            Conditions = new[]
            {
                new ConditionConfiguration
                {
                    ConditionId = "condition-1",
                    Type = "alwaysTrue",
                    Parameters = new Dictionary<string, object>()
                },
                new ConditionConfiguration
                {
                    ConditionId = "condition-2",
                    Type = "attributeEquals",
                    Parameters = new Dictionary<string, object> { { "attributeName", "rating" } }
                }
            },
            Rewards = new[]
            {
                new RewardConfiguration
                {
                    RewardId = "reward-1",
                    Type = "POINTS",
                    Parameters = new Dictionary<string, object> { { "amount", 100 } }
                },
                new RewardConfiguration
                {
                    RewardId = "reward-2",
                    Type = "BADGE",
                    Parameters = new Dictionary<string, object> { { "badgeId", "reviewer" } }
                }
            }
        };

        // Act
        var rule = RuleFactory.CreateFromConfiguration(config, _conditionFactory, _rewardFactory);

        // Assert
        rule.RuleId.ShouldBe("rule-1");
        rule.Name.ShouldBe("Test Rule");
        rule.Triggers.ShouldContain("USER_COMMENT");
        rule.Triggers.ShouldContain("USER_LOGIN");
        rule.Conditions.Count.ShouldBe(2);
        rule.Rewards.Count.ShouldBe(2);
    }

    [Fact]
    public void CreateFromConfiguration_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            RuleFactory.CreateFromConfiguration((RuleConfiguration)null!, _conditionFactory, _rewardFactory));
    }

    [Fact]
    public void CreateFromConfiguration_WithEmptyRuleId_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new RuleConfiguration
        {
            RuleId = "",
            Name = "Test Rule",
            Triggers = new[] { "USER_COMMENT" },
            Conditions = new[] { new ConditionConfiguration { ConditionId = "condition-1", Type = "alwaysTrue" } },
            Rewards = new[] { new RewardConfiguration { RewardId = "reward-1", Type = "POINTS" } }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            RuleFactory.CreateFromConfiguration(config, _conditionFactory, _rewardFactory))
            .Message.ShouldContain("RuleId is required");
    }

    [Fact]
    public void CreateFromConfiguration_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new RuleConfiguration
        {
            RuleId = "rule-1",
            Name = "",
            Triggers = new[] { "USER_COMMENT" },
            Conditions = new[] { new ConditionConfiguration { ConditionId = "condition-1", Type = "alwaysTrue" } },
            Rewards = new[] { new RewardConfiguration { RewardId = "reward-1", Type = "POINTS" } }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            RuleFactory.CreateFromConfiguration(config, _conditionFactory, _rewardFactory))
            .Message.ShouldContain("Name is required");
    }

    [Fact]
    public void CreateFromConfiguration_WithEmptyTriggers_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new RuleConfiguration
        {
            RuleId = "rule-1",
            Name = "Test Rule",
            Triggers = Array.Empty<string>(),
            Conditions = new[] { new ConditionConfiguration { ConditionId = "condition-1", Type = "alwaysTrue" } },
            Rewards = new[] { new RewardConfiguration { RewardId = "reward-1", Type = "POINTS" } }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            RuleFactory.CreateFromConfiguration(config, _conditionFactory, _rewardFactory))
            .Message.ShouldContain("Triggers are required");
    }

    [Fact]
    public void CreateFromConfiguration_WithEmptyConditions_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new RuleConfiguration
        {
            RuleId = "rule-1",
            Name = "Test Rule",
            Triggers = new[] { "USER_COMMENT" },
            Conditions = Array.Empty<ConditionConfiguration>(),
            Rewards = new[] { new RewardConfiguration { RewardId = "reward-1", Type = "POINTS" } }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            RuleFactory.CreateFromConfiguration(config, _conditionFactory, _rewardFactory))
            .Message.ShouldContain("Conditions are required");
    }

    [Fact]
    public void CreateFromConfiguration_WithEmptyRewards_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new RuleConfiguration
        {
            RuleId = "rule-1",
            Name = "Test Rule",
            Triggers = new[] { "USER_COMMENT" },
            Conditions = new[] { new ConditionConfiguration { ConditionId = "condition-1", Type = "alwaysTrue" } },
            Rewards = Array.Empty<RewardConfiguration>()
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            RuleFactory.CreateFromConfiguration(config, _conditionFactory, _rewardFactory))
            .Message.ShouldContain("Rewards are required");
    }

    [Fact]
    public void CreateFromConfiguration_WithNullConditionFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new RuleConfiguration
        {
            RuleId = "rule-1",
            Name = "Test Rule",
            Triggers = new[] { "USER_COMMENT" },
            Conditions = new[] { new ConditionConfiguration { ConditionId = "condition-1", Type = "alwaysTrue" } },
            Rewards = new[] { new RewardConfiguration { RewardId = "reward-1", Type = "POINTS" } }
        };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            RuleFactory.CreateFromConfiguration(config, null!, _rewardFactory));
    }

    [Fact]
    public void CreateFromConfiguration_WithNullRewardFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new RuleConfiguration
        {
            RuleId = "rule-1",
            Name = "Test Rule",
            Triggers = new[] { "USER_COMMENT" },
            Conditions = new[] { new ConditionConfiguration { ConditionId = "condition-1", Type = "alwaysTrue" } },
            Rewards = new[] { new RewardConfiguration { RewardId = "reward-1", Type = "POINTS" } }
        };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            RuleFactory.CreateFromConfiguration(config, _conditionFactory, null!));
    }

    [Fact]
    public void CreateFromConfiguration_WithMultipleConfigurations_ShouldCreateMultipleRules()
    {
        // Arrange
        var configs = new[]
        {
            new RuleConfiguration
            {
                RuleId = "rule-1",
                Name = "Test Rule 1",
                Triggers = new[] { "USER_COMMENT" },
                Conditions = new[] { new ConditionConfiguration { ConditionId = "condition-1", Type = "alwaysTrue" } },
                Rewards = new[] { new RewardConfiguration { RewardId = "reward-1", Type = "POINTS" } }
            },
            new RuleConfiguration
            {
                RuleId = "rule-2",
                Name = "Test Rule 2",
                Triggers = new[] { "USER_LOGIN" },
                Conditions = new[] { new ConditionConfiguration { ConditionId = "condition-2", Type = "alwaysTrue" } },
                Rewards = new[] { new RewardConfiguration { RewardId = "reward-2", Type = "BADGE" } }
            }
        };

        // Act
        var rules = RuleFactory.CreateFromConfiguration(configs, _conditionFactory, _rewardFactory);

        // Assert
        rules.Count().ShouldBe(2);
        rules.First().RuleId.ShouldBe("rule-1");
        rules.Last().RuleId.ShouldBe("rule-2");
    }

    [Fact]
    public void CreateFromConfiguration_WithNullConfigurations_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            RuleFactory.CreateFromConfiguration((IEnumerable<RuleConfiguration>)null!, _conditionFactory, _rewardFactory));
    }
}
