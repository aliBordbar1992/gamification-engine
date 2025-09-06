using GamificationEngine.Domain.Rewards;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rewards;

public class LevelRewardTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var levelId = "level-5";
        var category = "xp";

        // Act
        var reward = new LevelReward(rewardId, levelId, category);

        // Assert
        reward.RewardId.ShouldBe(rewardId);
        reward.Type.ShouldBe("level");
        reward.LevelId.ShouldBe(levelId);
        reward.Category.ShouldBe(category);
        reward.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithDefaultCategory_ShouldUseXp()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var levelId = "level-5";

        // Act
        var reward = new LevelReward(rewardId, levelId);

        // Assert
        reward.Category.ShouldBe("xp");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidLevelId_ShouldThrowArgumentException(string? levelId)
    {
        // Arrange
        var rewardId = "test-reward-1";
        var category = "xp";

        // Act & Assert
        Should.Throw<ArgumentException>(() => new LevelReward(rewardId, levelId!, category))
            .Message.ShouldContain("levelId cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidRewardId_ShouldThrowArgumentException(string? rewardId)
    {
        // Arrange
        var levelId = "level-5";
        var category = "xp";

        // Act & Assert
        Should.Throw<ArgumentException>(() => new LevelReward(rewardId!, levelId, category))
            .Message.ShouldContain("rewardId cannot be empty");
    }

    [Fact]
    public void Constructor_WithParameters_ShouldIncludeParameters()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var levelId = "level-5";
        var category = "xp";
        var parameters = new Dictionary<string, object>
        {
            { "threshold", 1000 },
            { "bonus", "unlock-feature" }
        };

        // Act
        var reward = new LevelReward(rewardId, levelId, category, parameters);

        // Assert
        reward.Parameters.ContainsKey("threshold").ShouldBeTrue();
        reward.Parameters.ContainsKey("bonus").ShouldBeTrue();
        reward.Parameters["threshold"].ShouldBe(1000);
        reward.Parameters["bonus"].ShouldBe("unlock-feature");
    }

    [Fact]
    public void GetLevelId_ShouldReturnLevelId()
    {
        // Arrange
        var reward = new LevelReward("test-reward-1", "level-10", "score");

        // Act
        var levelId = reward.GetLevelId();

        // Assert
        levelId.ShouldBe("level-10");
    }

    [Fact]
    public void GetCategory_ShouldReturnCategory()
    {
        // Arrange
        var reward = new LevelReward("test-reward-1", "level-10", "score");

        // Act
        var category = reward.GetCategory();

        // Assert
        category.ShouldBe("score");
    }

    [Fact]
    public void IsValid_WithValidReward_ShouldReturnTrue()
    {
        // Arrange
        var reward = new LevelReward("test-reward-1", "level-5", "xp");

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyLevelId_ShouldReturnFalse()
    {
        // Arrange
        var reward = new LevelReward("test-reward-1", "level-5", "xp");
        reward.LevelId = "";

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyCategory_ShouldReturnFalse()
    {
        // Arrange
        var reward = new LevelReward("test-reward-1", "level-5", "xp");
        reward.Category = "";

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }
}
