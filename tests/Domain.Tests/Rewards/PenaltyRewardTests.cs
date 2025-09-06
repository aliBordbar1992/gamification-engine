using GamificationEngine.Domain.Rewards;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rewards;

public class PenaltyRewardTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var penaltyType = "points";
        var targetId = "xp";
        var amount = 50L;

        // Act
        var reward = new PenaltyReward(rewardId, penaltyType, targetId, amount);

        // Assert
        reward.RewardId.ShouldBe(rewardId);
        reward.Type.ShouldBe("penalty");
        reward.PenaltyType.ShouldBe(penaltyType);
        reward.TargetId.ShouldBe(targetId);
        reward.Amount.ShouldBe(amount);
        reward.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithNullAmount_ShouldCreateInstance()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var penaltyType = "badge";
        var targetId = "badge-id";

        // Act
        var reward = new PenaltyReward(rewardId, penaltyType, targetId);

        // Assert
        reward.Amount.ShouldBeNull();
        reward.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidPenaltyType_ShouldThrowArgumentException(string? penaltyType)
    {
        // Arrange
        var rewardId = "test-reward-1";
        var targetId = "xp";
        var amount = 50L;

        // Act & Assert
        Should.Throw<ArgumentException>(() => new PenaltyReward(rewardId, penaltyType!, targetId, amount))
            .Message.ShouldContain("penaltyType cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidTargetId_ShouldThrowArgumentException(string? targetId)
    {
        // Arrange
        var rewardId = "test-reward-1";
        var penaltyType = "points";
        var amount = 50L;

        // Act & Assert
        Should.Throw<ArgumentException>(() => new PenaltyReward(rewardId, penaltyType, targetId!, amount))
            .Message.ShouldContain("targetId cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidRewardId_ShouldThrowArgumentException(string? rewardId)
    {
        // Arrange
        var penaltyType = "points";
        var targetId = "xp";
        var amount = 50L;

        // Act & Assert
        Should.Throw<ArgumentException>(() => new PenaltyReward(rewardId!, penaltyType, targetId, amount))
            .Message.ShouldContain("rewardId cannot be empty");
    }

    [Fact]
    public void Constructor_WithParameters_ShouldIncludeParameters()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var penaltyType = "points";
        var targetId = "xp";
        var amount = 50L;
        var parameters = new Dictionary<string, object>
        {
            { "reason", "cheating" },
            { "duration", "permanent" }
        };

        // Act
        var reward = new PenaltyReward(rewardId, penaltyType, targetId, amount, parameters);

        // Assert
        reward.Parameters.ContainsKey("reason").ShouldBeTrue();
        reward.Parameters.ContainsKey("duration").ShouldBeTrue();
        reward.Parameters["reason"].ShouldBe("cheating");
        reward.Parameters["duration"].ShouldBe("permanent");
    }

    [Fact]
    public void GetPenaltyType_ShouldReturnPenaltyType()
    {
        // Arrange
        var reward = new PenaltyReward("test-reward-1", "badge", "badge-id");

        // Act
        var penaltyType = reward.GetPenaltyType();

        // Assert
        penaltyType.ShouldBe("badge");
    }

    [Fact]
    public void GetTargetId_ShouldReturnTargetId()
    {
        // Arrange
        var reward = new PenaltyReward("test-reward-1", "points", "xp", 100L);

        // Act
        var targetId = reward.GetTargetId();

        // Assert
        targetId.ShouldBe("xp");
    }

    [Fact]
    public void GetAmount_ShouldReturnAmount()
    {
        // Arrange
        var reward = new PenaltyReward("test-reward-1", "points", "xp", 100L);

        // Act
        var amount = reward.GetAmount();

        // Assert
        amount.ShouldBe(100L);
    }

    [Fact]
    public void GetAmount_WithNullAmount_ShouldReturnNull()
    {
        // Arrange
        var reward = new PenaltyReward("test-reward-1", "badge", "badge-id");

        // Act
        var amount = reward.GetAmount();

        // Assert
        amount.ShouldBeNull();
    }

    [Fact]
    public void IsValid_WithValidPointsPenalty_ShouldReturnTrue()
    {
        // Arrange
        var reward = new PenaltyReward("test-reward-1", "points", "xp", 50L);

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithValidBadgePenalty_ShouldReturnTrue()
    {
        // Arrange
        var reward = new PenaltyReward("test-reward-1", "badge", "badge-id");

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyPenaltyType_ShouldReturnFalse()
    {
        // Arrange
        var reward = new PenaltyReward("test-reward-1", "points", "xp", 50L);
        reward.PenaltyType = "";

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyTargetId_ShouldReturnFalse()
    {
        // Arrange
        var reward = new PenaltyReward("test-reward-1", "points", "xp", 50L);
        reward.TargetId = "";

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithPointsPenaltyButNoAmount_ShouldReturnFalse()
    {
        // Arrange
        var reward = new PenaltyReward("test-reward-1", "points", "xp", 50L);
        reward.Amount = null;

        // Act
        var isValid = reward.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }
}
