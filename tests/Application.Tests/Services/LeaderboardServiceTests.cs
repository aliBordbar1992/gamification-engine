using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Application.Services;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Leaderboards;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Users;
using GamificationEngine.Shared;
using Moq;
using Shouldly;

namespace GamificationEngine.Application.Tests.Services;

public class LeaderboardServiceTests
{
    private readonly Mock<ILeaderboardRepository> _mockLeaderboardRepository;
    private readonly Mock<IUserStateRepository> _mockUserStateRepository;
    private readonly Mock<IPointCategoryRepository> _mockPointCategoryRepository;
    private readonly Mock<ILevelRepository> _mockLevelRepository;
    private readonly LeaderboardService _service;

    public LeaderboardServiceTests()
    {
        _mockLeaderboardRepository = new Mock<ILeaderboardRepository>();
        _mockUserStateRepository = new Mock<IUserStateRepository>();
        _mockPointCategoryRepository = new Mock<IPointCategoryRepository>();
        _mockLevelRepository = new Mock<ILevelRepository>();

        _service = new LeaderboardService(
            _mockLeaderboardRepository.Object,
            _mockUserStateRepository.Object,
            _mockPointCategoryRepository.Object,
            _mockLevelRepository.Object);
    }

    [Fact]
    public async Task GetLeaderboardAsync_WithValidQuery_ShouldReturnSuccess()
    {
        // Arrange
        var queryDto = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Points,
            Category = "xp",
            TimeRange = TimeRange.Daily,
            Page = 1,
            PageSize = 50
        };

        var domainQuery = new LeaderboardQuery(queryDto.Type, queryDto.Category, queryDto.TimeRange, queryDto.Page, queryDto.PageSize);
        var entries = new List<LeaderboardEntry>
        {
            new("user1", 1000, 1),
            new("user2", 900, 2)
        };
        var result = new LeaderboardResult(domainQuery, entries, totalCount: 2, totalPages: 1);

        _mockLeaderboardRepository
            .Setup(x => x.GetLeaderboardAsync(It.IsAny<LeaderboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _service.GetLeaderboardAsync(queryDto);

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Value.ShouldNotBeNull();
        response.Value.Query.Type.ShouldBe(queryDto.Type);
        response.Value.Entries.Count().ShouldBe(2);
        response.Value.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetLeaderboardAsync_WithInvalidType_ShouldReturnFailure()
    {
        // Arrange
        var queryDto = new LeaderboardQueryDto
        {
            Type = "invalid",
            Category = "xp",
            TimeRange = TimeRange.Daily,
            Page = 1,
            PageSize = 50
        };

        // Act
        var response = await _service.GetLeaderboardAsync(queryDto);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Error.ShouldContain("Invalid leaderboard type");
    }

    [Fact]
    public async Task GetLeaderboardAsync_WithInvalidTimeRange_ShouldReturnFailure()
    {
        // Arrange
        var queryDto = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Points,
            Category = "xp",
            TimeRange = "invalid",
            Page = 1,
            PageSize = 50
        };

        // Act
        var response = await _service.GetLeaderboardAsync(queryDto);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Error.ShouldContain("Invalid time range");
    }

    [Fact]
    public async Task GetLeaderboardAsync_WithInvalidPage_ShouldReturnFailure()
    {
        // Arrange
        var queryDto = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Points,
            Category = "xp",
            TimeRange = TimeRange.Daily,
            Page = 0,
            PageSize = 50
        };

        // Act
        var response = await _service.GetLeaderboardAsync(queryDto);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Error.ShouldContain("Page must be at least 1");
    }

    [Fact]
    public async Task GetLeaderboardAsync_WithInvalidPageSize_ShouldReturnFailure()
    {
        // Arrange
        var queryDto = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Points,
            Category = "xp",
            TimeRange = TimeRange.Daily,
            Page = 1,
            PageSize = 1001
        };

        // Act
        var response = await _service.GetLeaderboardAsync(queryDto);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Error.ShouldContain("Page size must be between 1 and 1000");
    }

    [Fact]
    public async Task GetLeaderboardAsync_WithPointsTypeButNoCategory_ShouldReturnFailure()
    {
        // Arrange
        var queryDto = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Points,
            Category = null,
            TimeRange = TimeRange.Daily,
            Page = 1,
            PageSize = 50
        };

        // Act
        var response = await _service.GetLeaderboardAsync(queryDto);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Error.ShouldContain("Category is required for points leaderboard");
    }

    [Fact]
    public async Task GetPointsLeaderboardAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var category = "xp";
        var timeRange = TimeRange.Daily;
        var page = 1;
        var pageSize = 50;

        var domainQuery = new LeaderboardQuery(LeaderboardType.Points, category, timeRange, page, pageSize);
        var entries = new List<LeaderboardEntry>
        {
            new("user1", 1000, 1)
        };
        var result = new LeaderboardResult(domainQuery, entries, totalCount: 1, totalPages: 1);

        _mockLeaderboardRepository
            .Setup(x => x.GetLeaderboardAsync(It.IsAny<LeaderboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _service.GetPointsLeaderboardAsync(category, timeRange, page, pageSize);

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Value.ShouldNotBeNull();
        response.Value.Query.Type.ShouldBe(LeaderboardType.Points);
        response.Value.Query.Category.ShouldBe(category);
    }

    [Fact]
    public async Task GetBadgesLeaderboardAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var timeRange = TimeRange.Weekly;
        var page = 1;
        var pageSize = 25;

        var domainQuery = new LeaderboardQuery(LeaderboardType.Badges, null, timeRange, page, pageSize);
        var entries = new List<LeaderboardEntry>
        {
            new("user1", 5, 1)
        };
        var result = new LeaderboardResult(domainQuery, entries, totalCount: 1, totalPages: 1);

        _mockLeaderboardRepository
            .Setup(x => x.GetLeaderboardAsync(It.IsAny<LeaderboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _service.GetBadgesLeaderboardAsync(timeRange, page, pageSize);

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Value.ShouldNotBeNull();
        response.Value.Query.Type.ShouldBe(LeaderboardType.Badges);
    }

    [Fact]
    public async Task GetTrophiesLeaderboardAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var timeRange = TimeRange.Monthly;
        var page = 1;
        var pageSize = 25;

        var domainQuery = new LeaderboardQuery(LeaderboardType.Trophies, null, timeRange, page, pageSize);
        var entries = new List<LeaderboardEntry>
        {
            new("user1", 3, 1)
        };
        var result = new LeaderboardResult(domainQuery, entries, totalCount: 1, totalPages: 1);

        _mockLeaderboardRepository
            .Setup(x => x.GetLeaderboardAsync(It.IsAny<LeaderboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _service.GetTrophiesLeaderboardAsync(timeRange, page, pageSize);

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Value.ShouldNotBeNull();
        response.Value.Query.Type.ShouldBe(LeaderboardType.Trophies);
    }

    [Fact]
    public async Task GetLevelsLeaderboardAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var category = "xp";
        var timeRange = TimeRange.AllTime;
        var page = 1;
        var pageSize = 25;

        var domainQuery = new LeaderboardQuery(LeaderboardType.Level, category, timeRange, page, pageSize);
        var entries = new List<LeaderboardEntry>
        {
            new("user1", 5000, 1)
        };
        var result = new LeaderboardResult(domainQuery, entries, totalCount: 1, totalPages: 1);

        _mockLeaderboardRepository
            .Setup(x => x.GetLeaderboardAsync(It.IsAny<LeaderboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _service.GetLevelsLeaderboardAsync(category, timeRange, page, pageSize);

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Value.ShouldNotBeNull();
        response.Value.Query.Type.ShouldBe(LeaderboardType.Level);
        response.Value.Query.Category.ShouldBe(category);
    }

    [Fact]
    public async Task GetUserRankAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var userId = "user1";
        var queryDto = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Points,
            Category = "xp",
            TimeRange = TimeRange.Daily
        };

        var domainQuery = new LeaderboardQuery(queryDto.Type, queryDto.Category, queryDto.TimeRange, 1, 1);
        var userState = new UserState(userId);
        userState.AddPoints("xp", 1000);

        _mockLeaderboardRepository
            .Setup(x => x.GetUserRankAsync(userId, It.IsAny<LeaderboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _mockUserStateRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userState);

        // Act
        var response = await _service.GetUserRankAsync(userId, queryDto);

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Value.ShouldNotBeNull();
        response.Value.UserId.ShouldBe(userId);
        response.Value.Rank.ShouldBe(5);
        response.Value.Points.ShouldBe(1000);
        response.Value.IsInLeaderboard.ShouldBeTrue();
    }

    [Fact]
    public async Task GetUserRankAsync_WithEmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        var queryDto = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Points,
            Category = "xp",
            TimeRange = TimeRange.Daily
        };

        // Act
        var response = await _service.GetUserRankAsync("", queryDto);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Error.ShouldContain("UserId cannot be empty");
    }

    [Fact]
    public async Task GetUserRankAsync_WithUserNotInLeaderboard_ShouldReturnSuccessWithNullRank()
    {
        // Arrange
        var userId = "user1";
        var queryDto = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Points,
            Category = "xp",
            TimeRange = TimeRange.Daily
        };

        var userState = new UserState(userId);
        userState.AddPoints("xp", 100);

        _mockLeaderboardRepository
            .Setup(x => x.GetUserRankAsync(userId, It.IsAny<LeaderboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        _mockUserStateRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userState);

        // Act
        var response = await _service.GetUserRankAsync(userId, queryDto);

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Value.ShouldNotBeNull();
        response.Value.UserId.ShouldBe(userId);
        response.Value.Rank.ShouldBeNull();
        response.Value.Points.ShouldBe(100);
        response.Value.IsInLeaderboard.ShouldBeFalse();
    }

    [Fact]
    public async Task RefreshLeaderboardCacheAsync_WithValidQuery_ShouldReturnSuccess()
    {
        // Arrange
        var queryDto = new LeaderboardQueryDto
        {
            Type = LeaderboardType.Points,
            Category = "xp",
            TimeRange = TimeRange.Daily
        };

        _mockLeaderboardRepository
            .Setup(x => x.RefreshCacheAsync(It.IsAny<LeaderboardQuery>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _service.RefreshLeaderboardCacheAsync(queryDto);

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task RefreshLeaderboardCacheAsync_WithInvalidQuery_ShouldReturnFailure()
    {
        // Arrange
        var queryDto = new LeaderboardQueryDto
        {
            Type = "invalid",
            Category = "xp",
            TimeRange = TimeRange.Daily
        };

        // Act
        var response = await _service.RefreshLeaderboardCacheAsync(queryDto);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Error.ShouldContain("Invalid leaderboard type");
    }
}
