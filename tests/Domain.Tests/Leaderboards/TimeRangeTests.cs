using GamificationEngine.Domain.Leaderboards;
using Shouldly;

namespace GamificationEngine.Domain.Tests.Leaderboards;

public class TimeRangeTests
{
    [Theory]
    [InlineData(TimeRange.Daily)]
    [InlineData(TimeRange.Weekly)]
    [InlineData(TimeRange.Monthly)]
    [InlineData(TimeRange.AllTime)]
    public void IsValid_WithValidTimeRange_ShouldReturnTrue(string timeRange)
    {
        // Act
        var isValid = TimeRange.IsValid(timeRange);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("DAILY")] // case sensitive
    public void IsValid_WithInvalidTimeRange_ShouldReturnFalse(string timeRange)
    {
        // Act
        var isValid = TimeRange.IsValid(timeRange);

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void AllRanges_ShouldContainAllValidRanges()
    {
        // Act
        var allRanges = TimeRange.AllRanges;

        // Assert
        allRanges.ShouldContain(TimeRange.Daily);
        allRanges.ShouldContain(TimeRange.Weekly);
        allRanges.ShouldContain(TimeRange.Monthly);
        allRanges.ShouldContain(TimeRange.AllTime);
        allRanges.Length.ShouldBe(4);
    }

    [Fact]
    public void GetStartDate_Daily_ShouldReturnStartOfDay()
    {
        // Arrange
        var referenceDate = new DateTime(2024, 1, 15, 14, 30, 45);

        // Act
        var startDate = TimeRange.GetStartDate(TimeRange.Daily, referenceDate);

        // Assert
        startDate.ShouldBe(new DateTime(2024, 1, 15, 0, 0, 0));
    }

    [Fact]
    public void GetStartDate_Weekly_ShouldReturnStartOfWeek()
    {
        // Arrange
        var referenceDate = new DateTime(2024, 1, 15, 14, 30, 45); // Monday

        // Act
        var startDate = TimeRange.GetStartDate(TimeRange.Weekly, referenceDate);

        // Assert
        startDate.ShouldBe(new DateTime(2024, 1, 14, 0, 0, 0)); // Sunday start (week starts on Sunday)
    }

    [Fact]
    public void GetStartDate_Monthly_ShouldReturnStartOfMonth()
    {
        // Arrange
        var referenceDate = new DateTime(2024, 1, 15, 14, 30, 45);

        // Act
        var startDate = TimeRange.GetStartDate(TimeRange.Monthly, referenceDate);

        // Assert
        startDate.ShouldBe(new DateTime(2024, 1, 1, 0, 0, 0));
    }

    [Fact]
    public void GetStartDate_AllTime_ShouldReturnMinValue()
    {
        // Arrange
        var referenceDate = new DateTime(2024, 1, 15, 14, 30, 45);

        // Act
        var startDate = TimeRange.GetStartDate(TimeRange.AllTime, referenceDate);

        // Assert
        startDate.ShouldBe(DateTime.MinValue);
    }

    [Fact]
    public void GetEndDate_Daily_ShouldReturnEndOfDay()
    {
        // Arrange
        var referenceDate = new DateTime(2024, 1, 15, 14, 30, 45);

        // Act
        var endDate = TimeRange.GetEndDate(TimeRange.Daily, referenceDate);

        // Assert
        endDate.ShouldBe(new DateTime(2024, 1, 16, 0, 0, 0).AddTicks(-1));
    }

    [Fact]
    public void GetEndDate_Weekly_ShouldReturnEndOfWeek()
    {
        // Arrange
        var referenceDate = new DateTime(2024, 1, 15, 14, 30, 45); // Monday

        // Act
        var endDate = TimeRange.GetEndDate(TimeRange.Weekly, referenceDate);

        // Assert
        endDate.ShouldBe(new DateTime(2024, 1, 21, 0, 0, 0).AddTicks(-1)); // Saturday end (week ends on Saturday)
    }

    [Fact]
    public void GetEndDate_Monthly_ShouldReturnEndOfMonth()
    {
        // Arrange
        var referenceDate = new DateTime(2024, 1, 15, 14, 30, 45);

        // Act
        var endDate = TimeRange.GetEndDate(TimeRange.Monthly, referenceDate);

        // Assert
        endDate.ShouldBe(new DateTime(2024, 2, 1, 0, 0, 0).AddTicks(-1));
    }

    [Fact]
    public void GetEndDate_AllTime_ShouldReturnMaxValue()
    {
        // Arrange
        var referenceDate = new DateTime(2024, 1, 15, 14, 30, 45);

        // Act
        var endDate = TimeRange.GetEndDate(TimeRange.AllTime, referenceDate);

        // Assert
        endDate.ShouldBe(DateTime.MaxValue);
    }

    [Fact]
    public void GetStartDate_WithNullReferenceDate_ShouldUseUtcNow()
    {
        // Act
        var startDate = TimeRange.GetStartDate(TimeRange.Daily);

        // Assert
        startDate.ShouldBe(DateTime.UtcNow.Date);
    }

    [Fact]
    public void GetEndDate_WithNullReferenceDate_ShouldUseUtcNow()
    {
        // Act
        var endDate = TimeRange.GetEndDate(TimeRange.Daily);

        // Assert
        endDate.ShouldBe(DateTime.UtcNow.Date.AddDays(1).AddTicks(-1));
    }

    [Theory]
    [InlineData("invalid")]
    public void GetStartDate_WithInvalidTimeRange_ShouldThrowArgumentException(string timeRange)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => TimeRange.GetStartDate(timeRange))
            .Message.ShouldContain($"Invalid time range: {timeRange}");
    }

    [Theory]
    [InlineData("invalid")]
    public void GetEndDate_WithInvalidTimeRange_ShouldThrowArgumentException(string timeRange)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => TimeRange.GetEndDate(timeRange))
            .Message.ShouldContain($"Invalid time range: {timeRange}");
    }
}
