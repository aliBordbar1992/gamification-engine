using GamificationEngine.Domain.Rewards;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rewards;

public class PointsRewardTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var category = "xp";
        var amount = 100L;

        // Act
        var reward = new PointsReward(rewardId, category, amount);

        // Assert
        reward.RewardId.ShouldBe(rewardId);
        reward.Type.ShouldBe("points");
        reward.Category.ShouldBe(category);
        reward.Amount.ShouldBe(amount);
        reward.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldCreateInstance()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var category = "xp";
        var amount = -50L;

        // Act
        var reward = new PointsReward(rewardId, category, amount);

        // Assert
        reward.Amount.ShouldBe(amount);
        reward.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithZeroAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var category = "xp";
        var amount = 0L;

        // Act & Assert
        Should.Throw<ArgumentException>(() => new PointsReward(rewardId, category, amount))
            .Message.ShouldContain("amount cannot be zero");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidCategory_ShouldThrowArgumentException(string? category)
    {
        // Arrange
        var rewardId = "test-reward-1";
        var amount = 100L;

        // Act & Assert
        Should.Throw<ArgumentException>(() => new PointsReward(rewardId, category!, amount))
            .Message.ShouldContain("category cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidRewardId_ShouldThrowArgumentException(string? rewardId)
    {
        // Arrange
        var category = "xp";
        var amount = 100L;

        // Act & Assert
        Should.Throw<ArgumentException>(() => new PointsReward(rewardId!, category, amount))
            .Message.ShouldContain("rewardId cannot be empty");
    }

    [Fact]
    public void Constructor_WithParameters_ShouldIncludeParameters()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var category = "xp";
        var amount = 100L;
        var parameters = new Dictionary<string, object>
        {
            { "bonus", true },
            { "multiplier", 2.0 }
        };

        // Act
        var reward = new PointsReward(rewardId, category, amount, parameters);

        // Assert
        reward.Parameters.ContainsKey("bonus").ShouldBeTrue();
        reward.Parameters.ContainsKey("multiplier").ShouldBeTrue();
        reward.Parameters["bonus"].ShouldBe(true);
        reward.Parameters["multiplier"].ShouldBe(2.0);
    }

    [Fact]
    public void GetPointsAmount_ShouldReturnAmount()
    {
        // Arrange
        var reward = new PointsReward("test-reward-1", "xp", 150L);

        // Act
        var amount = reward.GetPointsAmount();

        // Assert
        amount.ShouldBe(150L);
    }

    [Fact]
    public void GetCategory_ShouldReturnCategory()
    {
        // Arrange
        var reward = new PointsReward("test-reward-1", "score", 200L);

        // Act
        var category = reward.GetCategory();

        // Assert
        category.ShouldBe("score");
    }

    [Fact]
    public void IsValid_WithValidReward_ShouldReturnTrue()
    {
        // Arrange
        var reward = new PointsReward("test-reward-1", "xp", 100L);

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyCategory_ShouldReturnFalse()
    {
        // Arrange
        var reward = new PointsReward("test-reward-1", "xp", 100L);
        reward.Category = "";

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithZeroAmount_ShouldReturnFalse()
    {
        // Arrange
        var reward = new PointsReward("test-reward-1", "xp", 100L);
        reward.Amount = 0;

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }
}
