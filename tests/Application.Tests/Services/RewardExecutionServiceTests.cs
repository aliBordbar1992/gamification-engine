using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.Services;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Users;
using GamificationEngine.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace GamificationEngine.Application.Tests.Services;

public class RewardExecutionServiceTests
{
    private readonly Mock<IUserStateRepository> _mockUserStateRepository;
    private readonly Mock<IRewardHistoryRepository> _mockRewardHistoryRepository;
    private readonly Mock<ILogger<RewardExecutionService>> _mockLogger;
    private readonly RewardExecutionService _service;

    public RewardExecutionServiceTests()
    {
        _mockUserStateRepository = new Mock<IUserStateRepository>();
        _mockRewardHistoryRepository = new Mock<IRewardHistoryRepository>();
        _mockLogger = new Mock<ILogger<RewardExecutionService>>();
        _service = new RewardExecutionService(
            _mockUserStateRepository.Object,
            _mockRewardHistoryRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteRewardAsync_WithValidPointsReward_ShouldExecuteSuccessfully()
    {
        // Arrange
        var userId = "user-123";
        var reward = new PointsReward("reward-1", "xp", 100L);
        var triggerEvent = CreateTestEvent(userId);
        var userState = new UserState(userId);

        _mockUserStateRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userState);
        _mockUserStateRepository.Setup(x => x.SaveAsync(It.IsAny<UserState>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRewardHistoryRepository.Setup(x => x.StoreAsync(It.IsAny<RewardHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteRewardAsync(reward, userId, triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Success.ShouldBeTrue();
        result.Value.Message.ShouldContain("Awarded 100 xp points");

        userState.PointsByCategory["xp"].ShouldBe(100L);

        _mockUserStateRepository.Verify(x => x.SaveAsync(userState, It.IsAny<CancellationToken>()), Times.Once);
        _mockRewardHistoryRepository.Verify(x => x.StoreAsync(It.IsAny<RewardHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteRewardAsync_WithValidBadgeReward_ShouldExecuteSuccessfully()
    {
        // Arrange
        var userId = "user-123";
        var reward = new BadgeReward("reward-1", "first-login");
        var triggerEvent = CreateTestEvent(userId);
        var userState = new UserState(userId);

        _mockUserStateRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userState);
        _mockUserStateRepository.Setup(x => x.SaveAsync(It.IsAny<UserState>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRewardHistoryRepository.Setup(x => x.StoreAsync(It.IsAny<RewardHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteRewardAsync(reward, userId, triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Success.ShouldBeTrue();
        result.Value.Message.ShouldContain("Awarded badge first-login");

        userState.Badges.ShouldContain("first-login");

        _mockUserStateRepository.Verify(x => x.SaveAsync(userState, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteRewardAsync_WithDuplicateBadgeReward_ShouldNotAddDuplicate()
    {
        // Arrange
        var userId = "user-123";
        var reward = new BadgeReward("reward-1", "first-login");
        var triggerEvent = CreateTestEvent(userId);
        var userState = new UserState(userId);
        userState.GrantBadge("first-login"); // Already has the badge

        _mockUserStateRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userState);
        _mockUserStateRepository.Setup(x => x.SaveAsync(It.IsAny<UserState>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRewardHistoryRepository.Setup(x => x.StoreAsync(It.IsAny<RewardHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteRewardAsync(reward, userId, triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Success.ShouldBeTrue();
        result.Value.Message.ShouldContain("Badge first-login already awarded");

        userState.Badges.Count.ShouldBe(1); // Still only one badge

        _mockUserStateRepository.Verify(x => x.SaveAsync(userState, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteRewardAsync_WithValidTrophyReward_ShouldExecuteSuccessfully()
    {
        // Arrange
        var userId = "user-123";
        var reward = new TrophyReward("reward-1", "master-collector");
        var triggerEvent = CreateTestEvent(userId);
        var userState = new UserState(userId);

        _mockUserStateRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userState);
        _mockUserStateRepository.Setup(x => x.SaveAsync(It.IsAny<UserState>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRewardHistoryRepository.Setup(x => x.StoreAsync(It.IsAny<RewardHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteRewardAsync(reward, userId, triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Success.ShouldBeTrue();
        result.Value.Message.ShouldContain("Awarded trophy master-collector");

        userState.Trophies.ShouldContain("master-collector");

        _mockUserStateRepository.Verify(x => x.SaveAsync(userState, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteRewardAsync_WithValidLevelReward_ShouldExecuteSuccessfully()
    {
        // Arrange
        var userId = "user-123";
        var reward = new LevelReward("reward-1", "level-5", "xp");
        var triggerEvent = CreateTestEvent(userId);
        var userState = new UserState(userId);

        _mockUserStateRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userState);
        _mockUserStateRepository.Setup(x => x.SaveAsync(It.IsAny<UserState>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRewardHistoryRepository.Setup(x => x.StoreAsync(It.IsAny<RewardHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteRewardAsync(reward, userId, triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Success.ShouldBeTrue();
        result.Value.Message.ShouldContain("Level level-5 progression calculated for xp category");

        _mockUserStateRepository.Verify(x => x.SaveAsync(userState, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteRewardAsync_WithValidPointsPenalty_ShouldExecuteSuccessfully()
    {
        // Arrange
        var userId = "user-123";
        var reward = new PenaltyReward("reward-1", "points", "xp", 50L);
        var triggerEvent = CreateTestEvent(userId);
        var userState = new UserState(userId);
        userState.AddPoints("xp", 200L); // Start with 200 points

        _mockUserStateRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userState);
        _mockUserStateRepository.Setup(x => x.SaveAsync(It.IsAny<UserState>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRewardHistoryRepository.Setup(x => x.StoreAsync(It.IsAny<RewardHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteRewardAsync(reward, userId, triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Success.ShouldBeTrue();
        result.Value.Message.ShouldContain("Penalty: Removed 50 xp points");

        userState.PointsByCategory["xp"].ShouldBe(150L); // 200 - 50

        _mockUserStateRepository.Verify(x => x.SaveAsync(userState, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteRewardAsync_WithValidBadgePenalty_ShouldExecuteSuccessfully()
    {
        // Arrange
        var userId = "user-123";
        var reward = new PenaltyReward("reward-1", "badge", "first-login");
        var triggerEvent = CreateTestEvent(userId);
        var userState = new UserState(userId);
        userState.GrantBadge("first-login"); // User has the badge

        _mockUserStateRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userState);
        _mockUserStateRepository.Setup(x => x.SaveAsync(It.IsAny<UserState>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRewardHistoryRepository.Setup(x => x.StoreAsync(It.IsAny<RewardHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteRewardAsync(reward, userId, triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Success.ShouldBeTrue();
        result.Value.Message.ShouldContain("Penalty: Removed badge first-login");

        userState.Badges.ShouldNotContain("first-login");

        _mockUserStateRepository.Verify(x => x.SaveAsync(userState, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteRewardAsync_WithNewUser_ShouldCreateUserState()
    {
        // Arrange
        var userId = "user-123";
        var reward = new PointsReward("reward-1", "xp", 100L);
        var triggerEvent = CreateTestEvent(userId);

        _mockUserStateRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserState?)null); // User doesn't exist yet
        _mockUserStateRepository.Setup(x => x.SaveAsync(It.IsAny<UserState>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRewardHistoryRepository.Setup(x => x.StoreAsync(It.IsAny<RewardHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteRewardAsync(reward, userId, triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Success.ShouldBeTrue();

        _mockUserStateRepository.Verify(x => x.SaveAsync(It.Is<UserState>(us => us.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteRewardAsync_WithInvalidReward_ShouldReturnFailure()
    {
        // Arrange
        var userId = "user-123";
        var reward = new PointsReward("reward-1", "xp", 100L);
        reward.Category = ""; // Make it invalid
        var triggerEvent = CreateTestEvent(userId);

        _mockRewardHistoryRepository.Setup(x => x.StoreAsync(It.IsAny<RewardHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteRewardAsync(reward, userId, triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldContain("Invalid reward configuration");

        _mockUserStateRepository.Verify(x => x.SaveAsync(It.IsAny<UserState>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteRewardAsync_WithUnsupportedPenaltyType_ShouldReturnFailure()
    {
        // Arrange
        var userId = "user-123";
        var reward = new PenaltyReward("reward-1", "unsupported", "target");
        var triggerEvent = CreateTestEvent(userId);
        var userState = new UserState(userId);

        _mockUserStateRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userState);
        _mockRewardHistoryRepository.Setup(x => x.StoreAsync(It.IsAny<RewardHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteRewardAsync(reward, userId, triggerEvent);

        // Assert
        result.IsSuccess.ShouldBeTrue(); // Service doesn't fail, but reward execution fails
        result.Value.ShouldNotBeNull();
        result.Value.Success.ShouldBeFalse();
        result.Value.Message.ShouldContain("Unsupported penalty type: unsupported");

        _mockUserStateRepository.Verify(x => x.SaveAsync(It.IsAny<UserState>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Event CreateTestEvent(string userId)
    {
        return new Event(
            Guid.NewGuid().ToString(),
            "test-event",
            userId,
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>());
    }
}
