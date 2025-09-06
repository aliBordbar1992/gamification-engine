using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.Services;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Rules.Conditions;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Infrastructure.Storage.InMemory;
using GamificationEngine.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace GamificationEngine.Application.Tests;

public class RuleEvaluationServiceTests
{
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<IRewardExecutionService> _mockRewardExecutionService;
    private readonly Mock<ILogger<RuleEvaluationService>> _mockLogger;
    private readonly InMemoryRuleRepository _ruleRepository;
    private readonly RuleEvaluationService _service;

    public RuleEvaluationServiceTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockRewardExecutionService = new Mock<IRewardExecutionService>();
        _mockLogger = new Mock<ILogger<RuleEvaluationService>>();
        _ruleRepository = new InMemoryRuleRepository();

        _service = new RuleEvaluationService(
            _ruleRepository,
            _mockEventRepository.Object,
            _mockRewardExecutionService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task EvaluateRulesAsync_WithNoApplicableRules_ShouldReturnEmptyResult()
    {
        // Arrange
        var triggerEvent = new Event("event-1", "USER_COMMENT", "user-123", DateTimeOffset.UtcNow);
        var userEvents = new List<Event> { triggerEvent };
        _mockEventRepository.Setup(x => x.GetByUserIdAsync("user-123", 1000, 0))
            .ReturnsAsync(userEvents);

        // Act
        var result = await _service.EvaluateRulesAsync(triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ExecutedRewards.ShouldBeEmpty();
    }

    [Fact]
    public async Task EvaluateRulesAsync_WithMatchingRuleAndConditionsMet_ShouldExecuteRewards()
    {
        // Arrange
        var triggerEvent = new Event("event-1", "USER_COMMENT", "user-123", DateTimeOffset.UtcNow);
        var userEvents = new List<Event> { triggerEvent };

        var conditions = new List<Condition>
        {
            new AlwaysTrueCondition("condition-1")
        };

        var rewards = new List<Reward>
        {
            new TestReward("reward-1", "POINTS", new Dictionary<string, object> { { "amount", 100 } })
        };

        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, conditions, rewards);
        await _ruleRepository.StoreAsync(rule);

        _mockEventRepository.Setup(x => x.GetByUserIdAsync("user-123", 1000, 0))
            .ReturnsAsync(userEvents);

        _mockRewardExecutionService.Setup(x => x.ExecuteRewardAsync(
            It.IsAny<Reward>(),
            "user-123",
            triggerEvent,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RewardExecutionResult, DomainError>.Success(
                new RewardExecutionResult("reward-1", "POINTS", "user-123", "event-1", DateTimeOffset.UtcNow, true, "Reward executed successfully")));

        // Act
        var result = await _service.EvaluateRulesAsync(triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ExecutedRewards.Count.ShouldBe(1);
        result.Value.ExecutedRewards.First().RewardId.ShouldBe("reward-1");
        result.Value.ExecutedRewards.First().Success.ShouldBeTrue();
    }

    [Fact]
    public async Task EvaluateRulesAsync_WithMatchingRuleButConditionsNotMet_ShouldNotExecuteRewards()
    {
        // Arrange
        var triggerEvent = new Event("event-1", "USER_COMMENT", "user-123", DateTimeOffset.UtcNow,
            new Dictionary<string, object> { { "rating", 5 } });
        var userEvents = new List<Event> { triggerEvent };

        var conditions = new List<Condition>
        {
            new AttributeEqualsCondition("condition-1", new Dictionary<string, object>
            {
                { "attributeName", "rating" },
                { "expectedValue", 1 } // This won't match the trigger event's rating of 5
            })
        };

        var rewards = new List<Reward>
        {
            new TestReward("reward-1", "POINTS", new Dictionary<string, object> { { "amount", 100 } })
        };

        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, conditions, rewards);
        await _ruleRepository.StoreAsync(rule);

        _mockEventRepository.Setup(x => x.GetByUserIdAsync("user-123", 1000, 0))
            .ReturnsAsync(userEvents);

        // Act
        var result = await _service.EvaluateRulesAsync(triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ExecutedRewards.ShouldBeEmpty();
    }

    [Fact]
    public async Task EvaluateRulesAsync_WithMultipleRules_ShouldEvaluateAllApplicableRules()
    {
        // Arrange
        var triggerEvent = new Event("event-1", "USER_COMMENT", "user-123", DateTimeOffset.UtcNow);
        var userEvents = new List<Event> { triggerEvent };

        var conditions1 = new List<Condition>
        {
            new AlwaysTrueCondition("condition-1")
        };

        var conditions2 = new List<Condition>
        {
            new AlwaysTrueCondition("condition-2")
        };

        var rewards1 = new List<Reward>
        {
            new TestReward("reward-1", "POINTS", new Dictionary<string, object> { { "amount", 100 } })
        };

        var rewards2 = new List<Reward>
        {
            new TestReward("reward-2", "BADGE", new Dictionary<string, object> { { "badgeId", "reviewer" } })
        };

        var rule1 = new Rule("rule-1", "Test Rule 1", new[] { "USER_COMMENT" }, conditions1, rewards1);
        var rule2 = new Rule("rule-2", "Test Rule 2", new[] { "USER_COMMENT" }, conditions2, rewards2);

        await _ruleRepository.StoreAsync(rule1);
        await _ruleRepository.StoreAsync(rule2);

        _mockEventRepository.Setup(x => x.GetByUserIdAsync("user-123", 1000, 0))
            .ReturnsAsync(userEvents);

        _mockRewardExecutionService.Setup(x => x.ExecuteRewardAsync(
            It.Is<Reward>(r => r.RewardId == "reward-1"),
            "user-123",
            triggerEvent,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RewardExecutionResult, DomainError>.Success(
                new RewardExecutionResult("reward-1", "POINTS", "user-123", "event-1", DateTimeOffset.UtcNow, true, "Reward executed successfully")));

        _mockRewardExecutionService.Setup(x => x.ExecuteRewardAsync(
            It.Is<Reward>(r => r.RewardId == "reward-2"),
            "user-123",
            triggerEvent,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RewardExecutionResult, DomainError>.Success(
                new RewardExecutionResult("reward-2", "BADGE", "user-123", "event-1", DateTimeOffset.UtcNow, true, "Reward executed successfully")));

        // Act
        var result = await _service.EvaluateRulesAsync(triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ExecutedRewards.Count.ShouldBe(2);
        result.Value.ExecutedRewards.ShouldContain(r => r.RewardId == "reward-1");
        result.Value.ExecutedRewards.ShouldContain(r => r.RewardId == "reward-2");
    }

    [Fact]
    public async Task EvaluateRulesAsync_WithInactiveRule_ShouldNotEvaluateRule()
    {
        // Arrange
        var triggerEvent = new Event("event-1", "USER_COMMENT", "user-123", DateTimeOffset.UtcNow);
        var userEvents = new List<Event> { triggerEvent };

        var conditions = new List<Condition>
        {
            new AlwaysTrueCondition("condition-1")
        };

        var rewards = new List<Reward>
        {
            new TestReward("reward-1", "POINTS", new Dictionary<string, object> { { "amount", 100 } })
        };

        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, conditions, rewards, isActive: false);
        await _ruleRepository.StoreAsync(rule);

        _mockEventRepository.Setup(x => x.GetByUserIdAsync("user-123", 1000, 0))
            .ReturnsAsync(userEvents);

        // Act
        var result = await _service.EvaluateRulesAsync(triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ExecutedRewards.ShouldBeEmpty();
    }

    [Fact]
    public async Task EvaluateRulesAsync_WithNonMatchingTrigger_ShouldNotEvaluateRule()
    {
        // Arrange
        var triggerEvent = new Event("event-1", "USER_LOGIN", "user-123", DateTimeOffset.UtcNow);
        var userEvents = new List<Event> { triggerEvent };

        var conditions = new List<Condition>
        {
            new AlwaysTrueCondition("condition-1")
        };

        var rewards = new List<Reward>
        {
            new TestReward("reward-1", "POINTS", new Dictionary<string, object> { { "amount", 100 } })
        };

        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, conditions, rewards);
        await _ruleRepository.StoreAsync(rule);

        _mockEventRepository.Setup(x => x.GetByUserIdAsync("user-123", 1000, 0))
            .ReturnsAsync(userEvents);

        // Act
        var result = await _service.EvaluateRulesAsync(triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ExecutedRewards.ShouldBeEmpty();
    }

    [Fact]
    public async Task EvaluateRulesAsync_WithExceptionInEventRepository_ShouldReturnFailure()
    {
        // Arrange
        var triggerEvent = new Event("event-1", "USER_COMMENT", "user-123", DateTimeOffset.UtcNow);

        // Add a rule so that the service will try to evaluate it
        var conditions = new List<Condition>
        {
            new AlwaysTrueCondition("condition-1")
        };

        var rewards = new List<Reward>
        {
            new TestReward("reward-1", "POINTS", new Dictionary<string, object> { { "amount", 100 } })
        };

        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, conditions, rewards);
        await _ruleRepository.StoreAsync(rule);

        _mockEventRepository.Setup(x => x.GetByUserIdAsync("user-123", 1000, 0))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.EvaluateRulesAsync(triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.Code.ShouldBe("RULE_EVALUATION_ERROR");
        result.Error.Message.ShouldContain("Failed to evaluate rule rule-1");
    }

    [Fact]
    public async Task EvaluateRulesAsync_WithComplexConditions_ShouldEvaluateCorrectly()
    {
        // Arrange
        var triggerEvent = new Event("event-1", "USER_COMMENT", "user-123", DateTimeOffset.UtcNow,
            new Dictionary<string, object> { { "rating", 5 }, { "category", "review" } });

        var userEvents = new List<Event>
        {
            new Event("event-2", "USER_LOGIN", "user-123", DateTimeOffset.UtcNow.AddMinutes(-30)),
            triggerEvent
        };

        var conditions = new List<Condition>
        {
            new AlwaysTrueCondition("condition-1"),
            new AttributeEqualsCondition("condition-2", new Dictionary<string, object>
            {
                { "attributeName", "rating" },
                { "expectedValue", 5 }
            }),
            new CountCondition("condition-3", new Dictionary<string, object>
            {
                { "eventType", "USER_LOGIN" },
                { "minCount", 1 }
            })
        };

        var rewards = new List<Reward>
        {
            new TestReward("reward-1", "POINTS", new Dictionary<string, object> { { "amount", 200 } })
        };

        var rule = new Rule("rule-1", "Complex Rule", new[] { "USER_COMMENT" }, conditions, rewards);
        await _ruleRepository.StoreAsync(rule);

        _mockEventRepository.Setup(x => x.GetByUserIdAsync("user-123", 1000, 0))
            .ReturnsAsync(userEvents);

        _mockRewardExecutionService.Setup(x => x.ExecuteRewardAsync(
            It.IsAny<Reward>(),
            "user-123",
            triggerEvent,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RewardExecutionResult, DomainError>.Success(
                new RewardExecutionResult("reward-1", "POINTS", "user-123", "event-1", DateTimeOffset.UtcNow, true, "Reward executed successfully")));

        // Act
        var result = await _service.EvaluateRulesAsync(triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ExecutedRewards.Count.ShouldBe(1);
        result.Value.ExecutedRewards.First().RewardId.ShouldBe("reward-1");
        result.Value.ExecutedRewards.First().Success.ShouldBeTrue();
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
