using GamificationEngine.Domain.Rewards;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rewards;

public class BadgeRewardTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var badgeId = "first-login";

        // Act
        var reward = new BadgeReward(rewardId, badgeId);

        // Assert
        reward.RewardId.ShouldBe(rewardId);
        reward.Type.ShouldBe("badge");
        reward.BadgeId.ShouldBe(badgeId);
        reward.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidBadgeId_ShouldThrowArgumentException(string? badgeId)
    {
        // Arrange
        var rewardId = "test-reward-1";

        // Act & Assert
        Should.Throw<ArgumentException>(() => new BadgeReward(rewardId, badgeId!))
            .Message.ShouldContain("badgeId cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidRewardId_ShouldThrowArgumentException(string? rewardId)
    {
        // Arrange
        var badgeId = "first-login";

        // Act & Assert
        Should.Throw<ArgumentException>(() => new BadgeReward(rewardId!, badgeId))
            .Message.ShouldContain("rewardId cannot be empty");
    }

    [Fact]
    public void Constructor_WithParameters_ShouldIncludeParameters()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var badgeId = "first-login";
        var parameters = new Dictionary<string, object>
        {
            { "rarity", "common" },
            { "description", "First time logging in" }
        };

        // Act
        var reward = new BadgeReward(rewardId, badgeId, parameters);

        // Assert
        reward.Parameters.ContainsKey("rarity").ShouldBeTrue();
        reward.Parameters.ContainsKey("description").ShouldBeTrue();
        reward.Parameters["rarity"].ShouldBe("common");
        reward.Parameters["description"].ShouldBe("First time logging in");
    }

    [Fact]
    public void GetBadgeId_ShouldReturnBadgeId()
    {
        // Arrange
        var reward = new BadgeReward("test-reward-1", "achievement-badge");

        // Act
        var badgeId = reward.GetBadgeId();

        // Assert
        badgeId.ShouldBe("achievement-badge");
    }

    [Fact]
    public void IsValid_WithValidReward_ShouldReturnTrue()
    {
        // Arrange
        var reward = new BadgeReward("test-reward-1", "first-login");

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyBadgeId_ShouldReturnFalse()
    {
        // Arrange
        var reward = new BadgeReward("test-reward-1", "first-login");
        reward.BadgeId = "";

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }
}
