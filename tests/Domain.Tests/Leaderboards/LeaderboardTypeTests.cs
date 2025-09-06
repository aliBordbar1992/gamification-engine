using GamificationEngine.Domain.Leaderboards;
using Shouldly;

namespace GamificationEngine.Domain.Tests.Leaderboards;

public class LeaderboardTypeTests
{
    [Theory]
    [InlineData(LeaderboardType.Points)]
    [InlineData(LeaderboardType.Badges)]
    [InlineData(LeaderboardType.Trophies)]
    [InlineData(LeaderboardType.Level)]
    public void IsValid_WithValidType_ShouldReturnTrue(string type)
    {
        // Act
        var isValid = LeaderboardType.IsValid(type);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("POINTS")] // case sensitive
    public void IsValid_WithInvalidType_ShouldReturnFalse(string type)
    {
        // Act
        var isValid = LeaderboardType.IsValid(type);

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void AllTypes_ShouldContainAllValidTypes()
    {
        // Act
        var allTypes = LeaderboardType.AllTypes;

        // Assert
        allTypes.ShouldContain(LeaderboardType.Points);
        allTypes.ShouldContain(LeaderboardType.Badges);
        allTypes.ShouldContain(LeaderboardType.Trophies);
        allTypes.ShouldContain(LeaderboardType.Level);
        allTypes.Length.ShouldBe(4);
    }
}
