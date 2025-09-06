using GamificationEngine.Domain.Leaderboards;
using Shouldly;

namespace GamificationEngine.Domain.Tests.Leaderboards;

public class LeaderboardEntryTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEntry()
    {
        // Arrange
        var userId = "user123";
        var points = 1000L;
        var rank = 1;

        // Act
        var entry = new LeaderboardEntry(userId, points, rank);

        // Assert
        entry.UserId.ShouldBe(userId);
        entry.Points.ShouldBe(points);
        entry.Rank.ShouldBe(rank);
        entry.DisplayName.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithDisplayName_ShouldCreateEntryWithDisplayName()
    {
        // Arrange
        var userId = "user123";
        var points = 1000L;
        var rank = 1;
        var displayName = "TestUser";

        // Act
        var entry = new LeaderboardEntry(userId, points, rank, displayName);

        // Assert
        entry.UserId.ShouldBe(userId);
        entry.Points.ShouldBe(points);
        entry.Rank.ShouldBe(rank);
        entry.DisplayName.ShouldBe(displayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithEmptyUserId_ShouldThrowArgumentException(string userId)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new LeaderboardEntry(userId, 1000, 1))
            .Message.ShouldContain("UserId cannot be empty");
    }

    [Fact]
    public void Constructor_WithNegativePoints_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new LeaderboardEntry("user123", -1, 1))
            .Message.ShouldContain("Points cannot be negative");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidRank_ShouldThrowArgumentException(int rank)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new LeaderboardEntry("user123", 1000, rank))
            .Message.ShouldContain("Rank must be at least 1");
    }

    [Fact]
    public void UpdateRank_WithValidRank_ShouldUpdateRank()
    {
        // Arrange
        var entry = new LeaderboardEntry("user123", 1000, 1);

        // Act
        entry.UpdateRank(5);

        // Assert
        entry.Rank.ShouldBe(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateRank_WithInvalidRank_ShouldThrowArgumentException(int rank)
    {
        // Arrange
        var entry = new LeaderboardEntry("user123", 1000, 1);

        // Act & Assert
        Should.Throw<ArgumentException>(() => entry.UpdateRank(rank))
            .Message.ShouldContain("Rank must be at least 1");
    }

    [Fact]
    public void UpdatePoints_WithValidPoints_ShouldUpdatePoints()
    {
        // Arrange
        var entry = new LeaderboardEntry("user123", 1000, 1);

        // Act
        entry.UpdatePoints(2000);

        // Assert
        entry.Points.ShouldBe(2000);
    }

    [Fact]
    public void UpdatePoints_WithNegativePoints_ShouldThrowArgumentException()
    {
        // Arrange
        var entry = new LeaderboardEntry("user123", 1000, 1);

        // Act & Assert
        Should.Throw<ArgumentException>(() => entry.UpdatePoints(-1))
            .Message.ShouldContain("Points cannot be negative");
    }

    [Fact]
    public void UpdateDisplayName_WithValidName_ShouldUpdateDisplayName()
    {
        // Arrange
        var entry = new LeaderboardEntry("user123", 1000, 1);

        // Act
        entry.UpdateDisplayName("NewDisplayName");

        // Assert
        entry.DisplayName.ShouldBe("NewDisplayName");
    }

    [Fact]
    public void UpdateDisplayName_WithNull_ShouldUpdateDisplayNameToNull()
    {
        // Arrange
        var entry = new LeaderboardEntry("user123", 1000, 1, "OriginalName");

        // Act
        entry.UpdateDisplayName(null);

        // Assert
        entry.DisplayName.ShouldBeNull();
    }

    [Fact]
    public void IsValid_WithValidEntry_ShouldReturnTrue()
    {
        // Arrange
        var entry = new LeaderboardEntry("user123", 1000, 1);

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyUserId_ShouldReturnFalse()
    {
        // Arrange - Create a valid entry first, then modify it to be invalid
        var entry = new LeaderboardEntry("user123", 1000, 1);

        // Use reflection to set the UserId to empty (simulating invalid state)
        var userIdProperty = typeof(LeaderboardEntry).GetProperty("UserId");
        userIdProperty?.SetValue(entry, "");

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }
}
