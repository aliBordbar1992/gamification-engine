using GamificationEngine.Domain.Rewards;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rewards;

public class RewardFactoryTests
{
    [Fact]
    public void CreateReward_WithPointsType_ShouldCreatePointsReward()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "points";
        var parameters = new Dictionary<string, object>
        {
            { "category", "xp" },
            { "amount", 100L }
        };

        // Act
        var reward = RewardFactory.CreateReward(rewardId, rewardType, parameters);

        // Assert
        reward.ShouldBeOfType<PointsReward>();
        reward.RewardId.ShouldBe(rewardId);
        reward.Type.ShouldBe("points");
        var pointsReward = (PointsReward)reward;
        pointsReward.Category.ShouldBe("xp");
        pointsReward.Amount.ShouldBe(100L);
    }

    [Fact]
    public void CreateReward_WithBadgeType_ShouldCreateBadgeReward()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "badge";
        var parameters = new Dictionary<string, object>
        {
            { "badgeId", "first-login" }
        };

        // Act
        var reward = RewardFactory.CreateReward(rewardId, rewardType, parameters);

        // Assert
        reward.ShouldBeOfType<BadgeReward>();
        reward.RewardId.ShouldBe(rewardId);
        reward.Type.ShouldBe("badge");
        var badgeReward = (BadgeReward)reward;
        badgeReward.BadgeId.ShouldBe("first-login");
    }

    [Fact]
    public void CreateReward_WithTrophyType_ShouldCreateTrophyReward()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "trophy";
        var parameters = new Dictionary<string, object>
        {
            { "trophyId", "master-collector" }
        };

        // Act
        var reward = RewardFactory.CreateReward(rewardId, rewardType, parameters);

        // Assert
        reward.ShouldBeOfType<TrophyReward>();
        reward.RewardId.ShouldBe(rewardId);
        reward.Type.ShouldBe("trophy");
        var trophyReward = (TrophyReward)reward;
        trophyReward.TrophyId.ShouldBe("master-collector");
    }

    [Fact]
    public void CreateReward_WithLevelType_ShouldCreateLevelReward()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "level";
        var parameters = new Dictionary<string, object>
        {
            { "levelId", "level-5" },
            { "category", "xp" }
        };

        // Act
        var reward = RewardFactory.CreateReward(rewardId, rewardType, parameters);

        // Assert
        reward.ShouldBeOfType<LevelReward>();
        reward.RewardId.ShouldBe(rewardId);
        reward.Type.ShouldBe("level");
        var levelReward = (LevelReward)reward;
        levelReward.LevelId.ShouldBe("level-5");
        levelReward.Category.ShouldBe("xp");
    }

    [Fact]
    public void CreateReward_WithLevelTypeWithoutCategory_ShouldUseDefaultCategory()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "level";
        var parameters = new Dictionary<string, object>
        {
            { "levelId", "level-5" }
        };

        // Act
        var reward = RewardFactory.CreateReward(rewardId, rewardType, parameters);

        // Assert
        reward.ShouldBeOfType<LevelReward>();
        var levelReward = (LevelReward)reward;
        levelReward.Category.ShouldBe("xp");
    }

    [Fact]
    public void CreateReward_WithPenaltyType_ShouldCreatePenaltyReward()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "penalty";
        var parameters = new Dictionary<string, object>
        {
            { "penaltyType", "points" },
            { "targetId", "xp" },
            { "amount", 50L }
        };

        // Act
        var reward = RewardFactory.CreateReward(rewardId, rewardType, parameters);

        // Assert
        reward.ShouldBeOfType<PenaltyReward>();
        reward.RewardId.ShouldBe(rewardId);
        reward.Type.ShouldBe("penalty");
        var penaltyReward = (PenaltyReward)reward;
        penaltyReward.PenaltyType.ShouldBe("points");
        penaltyReward.TargetId.ShouldBe("xp");
        penaltyReward.Amount.ShouldBe(50L);
    }

    [Fact]
    public void CreateReward_WithPenaltyTypeWithoutAmount_ShouldCreatePenaltyReward()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "penalty";
        var parameters = new Dictionary<string, object>
        {
            { "penaltyType", "badge" },
            { "targetId", "badge-id" }
        };

        // Act
        var reward = RewardFactory.CreateReward(rewardId, rewardType, parameters);

        // Assert
        reward.ShouldBeOfType<PenaltyReward>();
        var penaltyReward = (PenaltyReward)reward;
        penaltyReward.Amount.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateReward_WithInvalidRewardId_ShouldThrowArgumentException(string? rewardId)
    {
        // Arrange
        var rewardType = "points";
        var parameters = new Dictionary<string, object>
        {
            { "category", "xp" },
            { "amount", 100L }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => RewardFactory.CreateReward(rewardId!, rewardType, parameters))
            .Message.ShouldContain("rewardId cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateReward_WithInvalidRewardType_ShouldThrowArgumentException(string? rewardType)
    {
        // Arrange
        var rewardId = "test-reward-1";
        var parameters = new Dictionary<string, object>
        {
            { "category", "xp" },
            { "amount", 100L }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => RewardFactory.CreateReward(rewardId, rewardType!, parameters))
            .Message.ShouldContain("rewardType cannot be empty");
    }

    [Fact]
    public void CreateReward_WithUnsupportedRewardType_ShouldThrowArgumentException()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "unsupported";
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        Should.Throw<ArgumentException>(() => RewardFactory.CreateReward(rewardId, rewardType, parameters))
            .Message.ShouldContain("Unsupported reward type: unsupported");
    }

    [Fact]
    public void CreateReward_WithPointsTypeMissingCategory_ShouldThrowArgumentException()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "points";
        var parameters = new Dictionary<string, object>
        {
            { "amount", 100L }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => RewardFactory.CreateReward(rewardId, rewardType, parameters))
            .Message.ShouldContain("Points reward requires 'category' parameter");
    }

    [Fact]
    public void CreateReward_WithPointsTypeMissingAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "points";
        var parameters = new Dictionary<string, object>
        {
            { "category", "xp" }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => RewardFactory.CreateReward(rewardId, rewardType, parameters))
            .Message.ShouldContain("Points reward requires 'amount' parameter");
    }

    [Fact]
    public void CreateReward_WithBadgeTypeMissingBadgeId_ShouldThrowArgumentException()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "badge";
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        Should.Throw<ArgumentException>(() => RewardFactory.CreateReward(rewardId, rewardType, parameters))
            .Message.ShouldContain("Badge reward requires 'badgeId' parameter");
    }

    [Fact]
    public void CreateReward_WithTrophyTypeMissingTrophyId_ShouldThrowArgumentException()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "trophy";
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        Should.Throw<ArgumentException>(() => RewardFactory.CreateReward(rewardId, rewardType, parameters))
            .Message.ShouldContain("Trophy reward requires 'trophyId' parameter");
    }

    [Fact]
    public void CreateReward_WithLevelTypeMissingLevelId_ShouldThrowArgumentException()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "level";
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        Should.Throw<ArgumentException>(() => RewardFactory.CreateReward(rewardId, rewardType, parameters))
            .Message.ShouldContain("Level reward requires 'levelId' parameter");
    }

    [Fact]
    public void CreateReward_WithPenaltyTypeMissingPenaltyType_ShouldThrowArgumentException()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "penalty";
        var parameters = new Dictionary<string, object>
        {
            { "targetId", "xp" }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => RewardFactory.CreateReward(rewardId, rewardType, parameters))
            .Message.ShouldContain("Penalty reward requires 'penaltyType' parameter");
    }

    [Fact]
    public void CreateReward_WithPenaltyTypeMissingTargetId_ShouldThrowArgumentException()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "penalty";
        var parameters = new Dictionary<string, object>
        {
            { "penaltyType", "points" }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => RewardFactory.CreateReward(rewardId, rewardType, parameters))
            .Message.ShouldContain("Penalty reward requires 'targetId' parameter");
    }

    [Fact]
    public void CreateReward_WithNullParameters_ShouldCreateRewardWithEmptyParameters()
    {
        // Arrange
        var rewardId = "test-reward-1";
        var rewardType = "points";
        var parameters = new Dictionary<string, object>
        {
            { "category", "xp" },
            { "amount", 100L }
        };

        // Act
        var reward = RewardFactory.CreateReward(rewardId, rewardType, parameters);

        // Assert
        reward.Parameters.ShouldNotBeNull();
        reward.Parameters.ContainsKey("category").ShouldBeTrue();
        reward.Parameters.ContainsKey("amount").ShouldBeTrue();
    }
}
