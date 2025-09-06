using GamificationEngine.Domain.Leaderboards;
using Shouldly;

namespace GamificationEngine.Domain.Tests.Leaderboards;

public class LeaderboardQueryTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateQuery()
    {
        // Arrange
        var type = LeaderboardType.Points;
        var category = "xp";
        var timeRange = TimeRange.Daily;
        var page = 1;
        var pageSize = 50;
        var referenceDate = DateTime.UtcNow;

        // Act
        var query = new LeaderboardQuery(type, category, timeRange, page, pageSize, referenceDate);

        // Assert
        query.Type.ShouldBe(type);
        query.Category.ShouldBe(category);
        query.TimeRange.ShouldBe(timeRange);
        query.Page.ShouldBe(page);
        query.PageSize.ShouldBe(pageSize);
        query.ReferenceDate.ShouldBe(referenceDate);
    }

    [Fact]
    public void Constructor_WithMinimalParameters_ShouldCreateQuery()
    {
        // Arrange
        var type = LeaderboardType.Badges;

        // Act
        var query = new LeaderboardQuery(type);

        // Assert
        query.Type.ShouldBe(type);
        query.Category.ShouldBeNull();
        query.TimeRange.ShouldBe(TimeRange.AllTime);
        query.Page.ShouldBe(1);
        query.PageSize.ShouldBe(50);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_WithInvalidType_ShouldThrowArgumentException(string type)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new LeaderboardQuery(type))
            .Message.ShouldContain($"Invalid leaderboard type: {type}");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_WithInvalidTimeRange_ShouldThrowArgumentException(string timeRange)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new LeaderboardQuery(LeaderboardType.Points, timeRange: timeRange))
            .Message.ShouldContain($"Invalid time range: {timeRange}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidPage_ShouldThrowArgumentException(int page)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new LeaderboardQuery(LeaderboardType.Points, page: page))
            .Message.ShouldContain("Page must be at least 1");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Constructor_WithInvalidPageSize_ShouldThrowArgumentException(int pageSize)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new LeaderboardQuery(LeaderboardType.Points, pageSize: pageSize))
            .Message.ShouldContain("Page size must be between 1 and 1000");
    }

    [Fact]
    public void StartDate_ShouldReturnCorrectStartDate()
    {
        // Arrange
        var referenceDate = new DateTime(2024, 1, 15, 14, 30, 45);
        var query = new LeaderboardQuery(LeaderboardType.Points, timeRange: TimeRange.Daily, referenceDate: referenceDate);

        // Act
        var startDate = query.StartDate;

        // Assert
        startDate.ShouldBe(new DateTime(2024, 1, 15, 0, 0, 0));
    }

    [Fact]
    public void EndDate_ShouldReturnCorrectEndDate()
    {
        // Arrange
        var referenceDate = new DateTime(2024, 1, 15, 14, 30, 45);
        var query = new LeaderboardQuery(LeaderboardType.Points, timeRange: TimeRange.Daily, referenceDate: referenceDate);

        // Act
        var endDate = query.EndDate;

        // Assert
        endDate.ShouldBe(new DateTime(2024, 1, 16, 0, 0, 0).AddTicks(-1));
    }

    [Fact]
    public void Skip_ShouldCalculateCorrectSkipValue()
    {
        // Arrange
        var query = new LeaderboardQuery(LeaderboardType.Points, page: 3, pageSize: 25);

        // Act
        var skip = query.Skip;

        // Assert
        skip.ShouldBe(50); // (3-1) * 25
    }

    [Fact]
    public void IsValid_WithValidQuery_ShouldReturnTrue()
    {
        // Arrange
        var query = new LeaderboardQuery(LeaderboardType.Points, "xp", TimeRange.Daily, 1, 50);

        // Act
        var isValid = query.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithInvalidType_ShouldReturnFalse()
    {
        // Arrange - Test the validation method directly
        var isValidType = LeaderboardType.IsValid("invalid");

        // Act & Assert
        isValidType.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithInvalidTimeRange_ShouldReturnFalse()
    {
        // Arrange - Test the validation method directly
        var isValidTimeRange = GamificationEngine.Domain.Leaderboards.TimeRange.IsValid("invalid");

        // Act & Assert
        isValidTimeRange.ShouldBeFalse();
    }
}
