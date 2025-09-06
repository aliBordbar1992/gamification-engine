using GamificationEngine.Domain.Rewards;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Rewards;

public class RewardHistoryTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var rewardHistoryId = "history-1";
        var userId = "user-123";
        var rewardId = "reward-1";
        var rewardType = "points";
        var triggerEventId = "event-1";
        var awardedAt = DateTimeOffset.UtcNow;
        var success = true;
        var message = "Reward awarded successfully";

        // Act
        var rewardHistory = new RewardHistory(
            rewardHistoryId, userId, rewardId, rewardType,
            triggerEventId, awardedAt, success, message);

        // Assert
        rewardHistory.RewardHistoryId.ShouldBe(rewardHistoryId);
        rewardHistory.UserId.ShouldBe(userId);
        rewardHistory.RewardId.ShouldBe(rewardId);
        rewardHistory.RewardType.ShouldBe(rewardType);
        rewardHistory.TriggerEventId.ShouldBe(triggerEventId);
        rewardHistory.AwardedAt.ShouldBe(awardedAt);
        rewardHistory.Success.ShouldBe(success);
        rewardHistory.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithDetails_ShouldIncludeDetails()
    {
        // Arrange
        var rewardHistoryId = "history-1";
        var userId = "user-123";
        var rewardId = "reward-1";
        var rewardType = "points";
        var triggerEventId = "event-1";
        var awardedAt = DateTimeOffset.UtcNow;
        var success = true;
        var message = "Reward awarded successfully";
        var details = new Dictionary<string, object>
        {
            { "amount", 100L },
            { "category", "xp" }
        };

        // Act
        var rewardHistory = new RewardHistory(
            rewardHistoryId, userId, rewardId, rewardType,
            triggerEventId, awardedAt, success, message, details);

        // Assert
        rewardHistory.Details.ContainsKey("amount").ShouldBeTrue();
        rewardHistory.Details.ContainsKey("category").ShouldBeTrue();
        rewardHistory.Details["amount"].ShouldBe(100L);
        rewardHistory.Details["category"].ShouldBe("xp");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidRewardHistoryId_ShouldThrowArgumentException(string? rewardHistoryId)
    {
        // Arrange
        var userId = "user-123";
        var rewardId = "reward-1";
        var rewardType = "points";
        var triggerEventId = "event-1";
        var awardedAt = DateTimeOffset.UtcNow;
        var success = true;

        // Act & Assert
        Should.Throw<ArgumentException>(() => new RewardHistory(
            rewardHistoryId!, userId, rewardId, rewardType,
            triggerEventId, awardedAt, success))
            .Message.ShouldContain("rewardHistoryId cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidUserId_ShouldThrowArgumentException(string? userId)
    {
        // Arrange
        var rewardHistoryId = "history-1";
        var rewardId = "reward-1";
        var rewardType = "points";
        var triggerEventId = "event-1";
        var awardedAt = DateTimeOffset.UtcNow;
        var success = true;

        // Act & Assert
        Should.Throw<ArgumentException>(() => new RewardHistory(
            rewardHistoryId, userId!, rewardId, rewardType,
            triggerEventId, awardedAt, success))
            .Message.ShouldContain("userId cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidRewardId_ShouldThrowArgumentException(string? rewardId)
    {
        // Arrange
        var rewardHistoryId = "history-1";
        var userId = "user-123";
        var rewardType = "points";
        var triggerEventId = "event-1";
        var awardedAt = DateTimeOffset.UtcNow;
        var success = true;

        // Act & Assert
        Should.Throw<ArgumentException>(() => new RewardHistory(
            rewardHistoryId, userId, rewardId!, rewardType,
            triggerEventId, awardedAt, success))
            .Message.ShouldContain("rewardId cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidRewardType_ShouldThrowArgumentException(string? rewardType)
    {
        // Arrange
        var rewardHistoryId = "history-1";
        var userId = "user-123";
        var rewardId = "reward-1";
        var triggerEventId = "event-1";
        var awardedAt = DateTimeOffset.UtcNow;
        var success = true;

        // Act & Assert
        Should.Throw<ArgumentException>(() => new RewardHistory(
            rewardHistoryId, userId, rewardId, rewardType!,
            triggerEventId, awardedAt, success))
            .Message.ShouldContain("rewardType cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidTriggerEventId_ShouldThrowArgumentException(string? triggerEventId)
    {
        // Arrange
        var rewardHistoryId = "history-1";
        var userId = "user-123";
        var rewardId = "reward-1";
        var rewardType = "points";
        var awardedAt = DateTimeOffset.UtcNow;
        var success = true;

        // Act & Assert
        Should.Throw<ArgumentException>(() => new RewardHistory(
            rewardHistoryId, userId, rewardId, rewardType,
            triggerEventId!, awardedAt, success))
            .Message.ShouldContain("triggerEventId cannot be empty");
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldCreateInstance()
    {
        // Arrange
        var rewardHistoryId = "history-1";
        var userId = "user-123";
        var rewardId = "reward-1";
        var rewardType = "points";
        var triggerEventId = "event-1";
        var awardedAt = DateTimeOffset.UtcNow;
        var success = true;

        // Act
        var rewardHistory = new RewardHistory(
            rewardHistoryId, userId, rewardId, rewardType,
            triggerEventId, awardedAt, success, null);

        // Assert
        rewardHistory.Message.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNullDetails_ShouldCreateInstanceWithEmptyDetails()
    {
        // Arrange
        var rewardHistoryId = "history-1";
        var userId = "user-123";
        var rewardId = "reward-1";
        var rewardType = "points";
        var triggerEventId = "event-1";
        var awardedAt = DateTimeOffset.UtcNow;
        var success = true;

        // Act
        var rewardHistory = new RewardHistory(
            rewardHistoryId, userId, rewardId, rewardType,
            triggerEventId, awardedAt, success, null, null);

        // Assert
        rewardHistory.Details.ShouldNotBeNull();
        rewardHistory.Details.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WithFailedReward_ShouldCreateInstance()
    {
        // Arrange
        var rewardHistoryId = "history-1";
        var userId = "user-123";
        var rewardId = "reward-1";
        var rewardType = "points";
        var triggerEventId = "event-1";
        var awardedAt = DateTimeOffset.UtcNow;
        var success = false;
        var message = "Reward failed to execute";

        // Act
        var rewardHistory = new RewardHistory(
            rewardHistoryId, userId, rewardId, rewardType,
            triggerEventId, awardedAt, success, message);

        // Assert
        rewardHistory.Success.ShouldBeFalse();
        rewardHistory.Message.ShouldBe(message);
    }
}
