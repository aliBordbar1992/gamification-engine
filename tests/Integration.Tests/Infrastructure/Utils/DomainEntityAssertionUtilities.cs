using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Users;
using Shouldly;

namespace GamificationEngine.Integration.Tests.Infrastructure.Utils;

/// <summary>
/// Utility class providing custom assertion helpers for domain entities
/// </summary>
public static class DomainEntityAssertionUtilities
{
    /// <summary>
    /// Asserts that a rule has the expected properties
    /// </summary>
    public static void AssertRuleProperties(Rule rule, string expectedRuleId, string expectedName, string[] expectedTriggers)
    {
        rule.ShouldNotBeNull();
        rule.RuleId.ShouldBe(expectedRuleId);
        rule.Name.ShouldBe(expectedName);
        rule.Triggers.ShouldBe(expectedTriggers);
        rule.Conditions.ShouldNotBeNull();
        rule.Rewards.ShouldNotBeNull();
    }

    /// <summary>
    /// Asserts that a rule has the expected number of conditions
    /// </summary>
    public static void AssertRuleConditionCount(Rule rule, int expectedConditionCount)
    {
        rule.ShouldNotBeNull();
        rule.Conditions.Count.ShouldBe(expectedConditionCount, $"Rule '{rule.RuleId}' should have {expectedConditionCount} conditions");
    }

    /// <summary>
    /// Asserts that a rule has the expected number of rewards
    /// </summary>
    public static void AssertRuleRewardCount(Rule rule, int expectedRewardCount)
    {
        rule.ShouldNotBeNull();
        rule.Rewards.Count.ShouldBe(expectedRewardCount, $"Rule '{rule.RuleId}' should have {expectedRewardCount} rewards");
    }

    /// <summary>
    /// Asserts that a rule contains a specific trigger
    /// </summary>
    public static void AssertRuleContainsTrigger(Rule rule, string expectedTrigger)
    {
        rule.ShouldNotBeNull();
        rule.Triggers.ShouldContain(expectedTrigger, $"Rule '{rule.RuleId}' should contain trigger '{expectedTrigger}'");
    }

    /// <summary>
    /// Asserts that a rule contains a condition of a specific type
    /// </summary>
    public static void AssertRuleContainsConditionType(Rule rule, string expectedConditionType)
    {
        rule.ShouldNotBeNull();
        rule.Conditions.Any(c => c.Type == expectedConditionType).ShouldBeTrue(
            $"Rule '{rule.RuleId}' should contain a condition of type '{expectedConditionType}'");
    }

    /// <summary>
    /// Asserts that a rule contains a reward of a specific type
    /// </summary>
    public static void AssertRuleContainsRewardType(Rule rule, string expectedRewardType)
    {
        rule.ShouldNotBeNull();
        rule.Rewards.Any(r => r.Type == expectedRewardType).ShouldBeTrue(
            $"Rule '{rule.RuleId}' should contain a reward of type '{expectedRewardType}'");
    }

    /// <summary>
    /// Asserts that a condition has the expected properties
    /// </summary>
    public static void AssertConditionProperties(Condition condition, string expectedConditionId, string expectedType)
    {
        condition.ShouldNotBeNull();
        condition.ConditionId.ShouldBe(expectedConditionId);
        condition.Type.ShouldBe(expectedType);
        condition.Parameters.ShouldNotBeNull();
    }

    /// <summary>
    /// Asserts that a condition has a specific parameter with expected value
    /// </summary>
    public static void AssertConditionParameter<T>(Condition condition, string parameterName, T expectedValue)
    {
        condition.ShouldNotBeNull();
        condition.Parameters.ContainsKey(parameterName).ShouldBeTrue($"Condition should contain parameter '{parameterName}'");
        condition.Parameters[parameterName].ShouldBe(expectedValue);
    }

    /// <summary>
    /// Asserts that a condition has the expected number of parameters
    /// </summary>
    public static void AssertConditionParameterCount(Condition condition, int expectedParameterCount)
    {
        condition.ShouldNotBeNull();
        condition.Parameters.Count.ShouldBe(expectedParameterCount, $"Condition should have {expectedParameterCount} parameters");
    }

    /// <summary>
    /// Asserts that a reward has the expected properties
    /// </summary>
    public static void AssertRewardProperties(Reward reward, string expectedRewardId, string expectedType)
    {
        reward.ShouldNotBeNull();
        reward.RewardId.ShouldBe(expectedRewardId);
        reward.Type.ShouldBe(expectedType);
        reward.Parameters.ShouldNotBeNull();
    }

    /// <summary>
    /// Asserts that a reward has a specific parameter with expected value
    /// </summary>
    public static void AssertRewardParameter<T>(Reward reward, string parameterName, T expectedValue)
    {
        reward.ShouldNotBeNull();
        reward.Parameters.ContainsKey(parameterName).ShouldBeTrue($"Reward should contain parameter '{parameterName}'");
        reward.Parameters[parameterName].ShouldBe(expectedValue);
    }

    /// <summary>
    /// Asserts that a reward has the expected number of parameters
    /// </summary>
    public static void AssertRewardParameterCount(Reward reward, int expectedParameterCount)
    {
        reward.ShouldNotBeNull();
        reward.Parameters.Count.ShouldBe(expectedParameterCount, $"Reward should have {expectedParameterCount} parameters");
    }

    /// <summary>
    /// Asserts that a collection of rules contains a rule with specific properties
    /// </summary>
    public static void AssertRulesCollectionContains(IEnumerable<Rule> rules, string expectedRuleId, string expectedName)
    {
        rules.ShouldNotBeNull();
        rules.Any(r => r.RuleId == expectedRuleId && r.Name == expectedName).ShouldBeTrue(
            $"Rules collection should contain rule with ID '{expectedRuleId}' and name '{expectedName}'");
    }

    /// <summary>
    /// Asserts that a collection of conditions contains a condition with specific properties
    /// </summary>
    public static void AssertConditionsCollectionContains(IEnumerable<Condition> conditions, string expectedConditionId, string expectedType)
    {
        conditions.ShouldNotBeNull();
        conditions.Any(c => c.ConditionId == expectedConditionId && c.Type == expectedType).ShouldBeTrue(
            $"Conditions collection should contain condition with ID '{expectedConditionId}' and type '{expectedType}'");
    }

    /// <summary>
    /// Asserts that a collection of rewards contains a reward with specific properties
    /// </summary>
    public static void AssertRewardsCollectionContains(IEnumerable<Reward> rewards, string expectedRewardId, string expectedType)
    {
        rewards.ShouldNotBeNull();
        rewards.Any(r => r.RewardId == expectedRewardId && r.Type == expectedType).ShouldBeTrue(
            $"Rewards collection should contain reward with ID '{expectedRewardId}' and type '{expectedType}'");
    }

    /// <summary>
    /// Asserts that an event has the expected properties
    /// </summary>
    public static void AssertEventProperties(Event @event, string expectedUserId, string expectedEventType)
    {
        @event.ShouldNotBeNull();
        @event.UserId.ShouldBe(expectedUserId);
        @event.EventType.ShouldBe(expectedEventType);
        @event.OccurredAt.ShouldBeGreaterThan(DateTimeOffset.MinValue);
        @event.Attributes.ShouldNotBeNull();
    }

    /// <summary>
    /// Asserts that a user state has the expected properties
    /// </summary>
    public static void AssertUserStateProperties(UserState userState, string expectedUserId, Dictionary<string, long> expectedPointsByCategory)
    {
        userState.ShouldNotBeNull();
        userState.UserId.ShouldBe(expectedUserId);
        userState.PointsByCategory.ShouldBe(expectedPointsByCategory);
        userState.Badges.ShouldNotBeNull();
        userState.Trophies.ShouldNotBeNull();
    }
}