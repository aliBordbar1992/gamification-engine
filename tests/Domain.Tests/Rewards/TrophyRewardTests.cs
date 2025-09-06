using GamificationEngine.Domain.Rewards;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rewards;

public class TrophyRewardTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var trophyId = "master-collector";

        // Act
        var reward = new TrophyReward(rewardId, trophyId);

        // Assert
        reward.RewardId.ShouldBe(rewardId);
        reward.Type.ShouldBe("trophy");
        reward.TrophyId.ShouldBe(trophyId);
        reward.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidTrophyId_ShouldThrowArgumentException(string? trophyId)
    {
        // Arrange
        var rewardId = "test-reward-1";

        // Act & Assert
        Should.Throw<ArgumentException>(() => new TrophyReward(rewardId, trophyId!))
            .Message.ShouldContain("trophyId cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidRewardId_ShouldThrowArgumentException(string? rewardId)
    {
        // Arrange
        var trophyId = "master-collector";

        // Act & Assert
        Should.Throw<ArgumentException>(() => new TrophyReward(rewardId!, trophyId))
            .Message.ShouldContain("rewardId cannot be empty");
    }

    [Fact]
    public void Constructor_WithParameters_ShouldIncludeParameters()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var trophyId = "master-collector";
        var parameters = new Dictionary<string, object>
        {
            { "rarity", "legendary" },
            { "description", "Collected 100 badges" }
        };

        // Act
        var reward = new TrophyReward(rewardId, trophyId, parameters);

        // Assert
        reward.Parameters.ContainsKey("rarity").ShouldBeTrue();
        reward.Parameters.ContainsKey("description").ShouldBeTrue();
        reward.Parameters["rarity"].ShouldBe("legendary");
        reward.Parameters["description"].ShouldBe("Collected 100 badges");
    }

    [Fact]
    public void GetTrophyId_ShouldReturnTrophyId()
    {
        // Arrange
        var reward = new TrophyReward("test-reward-1", "achievement-trophy");

        // Act
        var trophyId = reward.GetTrophyId();

        // Assert
        trophyId.ShouldBe("achievement-trophy");
    }

    [Fact]
    public void IsValid_WithValidReward_ShouldReturnTrue()
    {
        // Arrange
        var reward = new TrophyReward("test-reward-1", "master-collector");

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyTrophyId_ShouldReturnFalse()
    {
        // Arrange
        var reward = new TrophyReward("test-reward-1", "master-collector");
        reward.TrophyId = "";

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }
}
