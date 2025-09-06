using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Rules.Conditions;
using GamificationEngine.Infrastructure.Storage.InMemory;
using GamificationEngine.Domain.Rewards;
using Shouldly;
using Xunit;

namespace GamificationEngine.Infrastructure.Tests;

public class InMemoryRuleRepositoryTests
{
    private readonly InMemoryRuleRepository _repository;
    private readonly List<Condition> _conditions;
    private readonly List<Reward> _rewards;

    public InMemoryRuleRepositoryTests()
    {
        _repository = new InMemoryRuleRepository();

        _conditions = new List<Condition>
        {
            new AlwaysTrueCondition("condition-1")
        };

        _rewards = new List<Reward>
        {
            new TestReward("reward-1", "POINTS", new Dictionary<string, object> { { "amount", 100 } })
        };
    }

    [Fact]
    public async Task StoreAsync_WithValidRule_ShouldStoreRule()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);

        // Act
        await _repository.StoreAsync(rule);

        // Assert
        var retrievedRule = await _repository.GetByIdAsync("rule-1");
        retrievedRule.ShouldNotBeNull();
        retrievedRule.RuleId.ShouldBe("rule-1");
        retrievedRule.Name.ShouldBe("Test Rule");
    }

    [Fact]
    public async Task StoreAsync_WithNullRule_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() => _repository.StoreAsync(null!));
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingRule_ShouldReturnRule()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);
        await _repository.StoreAsync(rule);

        // Act
        var retrievedRule = await _repository.GetByIdAsync("rule-1");

        // Assert
        retrievedRule.ShouldNotBeNull();
        retrievedRule.RuleId.ShouldBe("rule-1");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingRule_ShouldReturnNull()
    {
        // Act
        var retrievedRule = await _repository.GetByIdAsync("non-existing");

        // Assert
        retrievedRule.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetByIdAsync_WithEmptyRuleId_ShouldThrowArgumentException(string ruleId)
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() => _repository.GetByIdAsync(ruleId!));
    }

    [Fact]
    public async Task GetByTriggerAsync_WithMatchingTrigger_ShouldReturnActiveRules()
    {
        // Arrange
        var rule1 = new Rule("rule-1", "Test Rule 1", new[] { "USER_COMMENT" }, _conditions, _rewards);
        var rule2 = new Rule("rule-2", "Test Rule 2", new[] { "USER_COMMENT", "USER_LOGIN" }, _conditions, _rewards);
        var rule3 = new Rule("rule-3", "Test Rule 3", new[] { "USER_LOGIN" }, _conditions, _rewards, isActive: false);

        await _repository.StoreAsync(rule1);
        await _repository.StoreAsync(rule2);
        await _repository.StoreAsync(rule3);

        // Act
        var rules = await _repository.GetByTriggerAsync("USER_COMMENT");

        // Assert
        rules.Count().ShouldBe(2);
        rules.ShouldContain(r => r.RuleId == "rule-1");
        rules.ShouldContain(r => r.RuleId == "rule-2");
        rules.ShouldNotContain(r => r.RuleId == "rule-3"); // Inactive rule
    }

    [Fact]
    public async Task GetByTriggerAsync_WithNonMatchingTrigger_ShouldReturnEmptyCollection()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);
        await _repository.StoreAsync(rule);

        // Act
        var rules = await _repository.GetByTriggerAsync("USER_LOGIN");

        // Assert
        rules.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetByTriggerAsync_WithEmptyEventType_ShouldThrowArgumentException(string eventType)
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() => _repository.GetByTriggerAsync(eventType!));
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveRules()
    {
        // Arrange
        var rule1 = new Rule("rule-1", "Test Rule 1", new[] { "USER_COMMENT" }, _conditions, _rewards, isActive: true);
        var rule2 = new Rule("rule-2", "Test Rule 2", new[] { "USER_LOGIN" }, _conditions, _rewards, isActive: false);

        await _repository.StoreAsync(rule1);
        await _repository.StoreAsync(rule2);

        // Act
        var activeRules = await _repository.GetAllActiveAsync();

        // Assert
        activeRules.Count().ShouldBe(1);
        activeRules.ShouldContain(r => r.RuleId == "rule-1");
        activeRules.ShouldNotContain(r => r.RuleId == "rule-2");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRules()
    {
        // Arrange
        var rule1 = new Rule("rule-1", "Test Rule 1", new[] { "USER_COMMENT" }, _conditions, _rewards, isActive: true);
        var rule2 = new Rule("rule-2", "Test Rule 2", new[] { "USER_LOGIN" }, _conditions, _rewards, isActive: false);

        await _repository.StoreAsync(rule1);
        await _repository.StoreAsync(rule2);

        // Act
        var allRules = await _repository.GetAllAsync();

        // Assert
        allRules.Count().ShouldBe(2);
        allRules.ShouldContain(r => r.RuleId == "rule-1");
        allRules.ShouldContain(r => r.RuleId == "rule-2");
    }

    [Fact]
    public async Task UpdateAsync_WithExistingRule_ShouldUpdateRule()
    {
        // Arrange
        var originalRule = new Rule("rule-1", "Original Name", new[] { "USER_COMMENT" }, _conditions, _rewards);
        await _repository.StoreAsync(originalRule);

        var updatedRule = new Rule("rule-1", "Updated Name", new[] { "USER_COMMENT", "USER_LOGIN" }, _conditions, _rewards);

        // Act
        await _repository.UpdateAsync(updatedRule);

        // Assert
        var retrievedRule = await _repository.GetByIdAsync("rule-1");
        retrievedRule.ShouldNotBeNull();
        retrievedRule.Name.ShouldBe("Updated Name");
        retrievedRule.Triggers.Length.ShouldBe(2);
        retrievedRule.Triggers.ShouldContain("USER_LOGIN");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingRule_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var rule = new Rule("non-existing", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _repository.UpdateAsync(rule));
    }

    [Fact]
    public async Task UpdateAsync_WithNullRule_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() => _repository.UpdateAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingRule_ShouldRemoveRule()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);
        await _repository.StoreAsync(rule);

        // Act
        await _repository.DeleteAsync("rule-1");

        // Assert
        var retrievedRule = await _repository.GetByIdAsync("rule-1");
        retrievedRule.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingRule_ShouldNotThrow()
    {
        // Act & Assert
        await Should.NotThrowAsync(() => _repository.DeleteAsync("non-existing"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task DeleteAsync_WithEmptyRuleId_ShouldThrowArgumentException(string ruleId)
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() => _repository.DeleteAsync(ruleId!));
    }

    [Fact]
    public async Task ExistsAsync_WithExistingRule_ShouldReturnTrue()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);
        await _repository.StoreAsync(rule);

        // Act
        var exists = await _repository.ExistsAsync("rule-1");

        // Assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingRule_ShouldReturnFalse()
    {
        // Act
        var exists = await _repository.ExistsAsync("non-existing");

        // Assert
        exists.ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExistsAsync_WithEmptyRuleId_ShouldThrowArgumentException(string ruleId)
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() => _repository.ExistsAsync(ruleId!));
    }

    [Fact]
    public void Clear_ShouldRemoveAllRules()
    {
        // Arrange
        var rule = new Rule("rule-1", "Test Rule", new[] { "USER_COMMENT" }, _conditions, _rewards);
        _repository.StoreAsync(rule).Wait();

        // Act
        _repository.Clear();

        // Assert
        _repository.Count.ShouldBe(0);
    }

    [Fact]
    public void Count_ShouldReturnCorrectNumberOfRules()
    {
        // Arrange
        var rule1 = new Rule("rule-1", "Test Rule 1", new[] { "USER_COMMENT" }, _conditions, _rewards);
        var rule2 = new Rule("rule-2", "Test Rule 2", new[] { "USER_LOGIN" }, _conditions, _rewards);

        // Act & Assert
        _repository.Count.ShouldBe(0);

        _repository.StoreAsync(rule1).Wait();
        _repository.Count.ShouldBe(1);

        _repository.StoreAsync(rule2).Wait();
        _repository.Count.ShouldBe(2);
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
