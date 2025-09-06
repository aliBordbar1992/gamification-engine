using GamificationEngine.Domain.Entities;
using Shouldly;

namespace GamificationEngine.Domain.Tests.Entities;

/// <summary>
/// Unit tests for Level domain entity
/// </summary>
public class LevelTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateLevel()
    {
        // Arrange
        var id = "bronze";
        var name = "Bronze";
        var category = "xp";
        var minPoints = 100L;

        // Act
        var level = new Level(id, name, category, minPoints);

        // Assert
        level.Id.ShouldBe(id);
        level.Name.ShouldBe(name);
        level.Category.ShouldBe(category);
        level.MinPoints.ShouldBe(minPoints);
        level.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "name", "category", 100L, "id")]
    [InlineData("id", "", "category", 100L, "name")]
    [InlineData("id", "name", "", 100L, "category")]
    [InlineData("id", "name", "category", -1L, "minPoints")]
    public void Constructor_WithInvalidParameters_ShouldThrowArgumentException(
        string id, string name, string category, long minPoints, string expectedParam)
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => new Level(id, name, category, minPoints));
        exception.ParamName.ShouldBe(expectedParam);
    }

    [Fact]
    public void Constructor_WithZeroMinPoints_ShouldCreateLevel()
    {
        // Arrange
        var id = "bronze";
        var name = "Bronze";
        var category = "xp";
        var minPoints = 0L;

        // Act
        var level = new Level(id, name, category, minPoints);

        // Assert
        level.MinPoints.ShouldBe(minPoints);
        level.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void UpdateInfo_WithValidParameters_ShouldUpdateProperties()
    {
        // Arrange
        var level = new Level("bronze", "Old Name", "old_category", 50L);
        var newName = "New Name";
        var newCategory = "new_category";
        var newMinPoints = 200L;

        // Act
        level.UpdateInfo(newName, newCategory, newMinPoints);

        // Assert
        level.Name.ShouldBe(newName);
        level.Category.ShouldBe(newCategory);
        level.MinPoints.ShouldBe(newMinPoints);
        level.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "category", 100L, "name")]
    [InlineData("name", "", 100L, "category")]
    [InlineData("name", "category", -1L, "minPoints")]
    public void UpdateInfo_WithInvalidParameters_ShouldThrowArgumentException(
        string name, string category, long minPoints, string expectedParam)
    {
        // Arrange
        var level = new Level("bronze", "Name", "Category", 100L);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => level.UpdateInfo(name, category, minPoints));
        exception.ParamName.ShouldBe(expectedParam);
    }

    [Fact]
    public void UpdateMinPoints_WithValidValue_ShouldUpdateMinPoints()
    {
        // Arrange
        var level = new Level("bronze", "Bronze", "xp", 100L);
        var newMinPoints = 200L;

        // Act
        level.UpdateMinPoints(newMinPoints);

        // Assert
        level.MinPoints.ShouldBe(newMinPoints);
    }

    [Fact]
    public void UpdateMinPoints_WithZero_ShouldUpdateMinPoints()
    {
        // Arrange
        var level = new Level("bronze", "Bronze", "xp", 100L);
        var newMinPoints = 0L;

        // Act
        level.UpdateMinPoints(newMinPoints);

        // Assert
        level.MinPoints.ShouldBe(newMinPoints);
    }

    [Fact]
    public void UpdateMinPoints_WithNegativeValue_ShouldThrowArgumentException()
    {
        // Arrange
        var level = new Level("bronze", "Bronze", "xp", 100L);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => level.UpdateMinPoints(-1L));
        exception.ParamName.ShouldBe("minPoints");
    }

    [Theory]
    [InlineData(100L, 100L, true)]
    [InlineData(100L, 150L, true)]
    [InlineData(100L, 50L, false)]
    [InlineData(100L, 0L, false)]
    public void QualifiesForLevel_WithDifferentPoints_ShouldReturnCorrectResult(
        long minPoints, long userPoints, bool expectedQualifies)
    {
        // Arrange
        var level = new Level("bronze", "Bronze", "xp", minPoints);

        // Act
        var qualifies = level.QualifiesForLevel(userPoints);

        // Assert
        qualifies.ShouldBe(expectedQualifies);
    }

    [Fact]
    public void IsValid_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var level = new Level("bronze", "Bronze", "xp", 100L);

        // Act
        var isValid = level.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyId_ShouldReturnFalse()
    {
        // Arrange
        var level = new Level("bronze", "Bronze", "xp", 100L);
        // Use reflection to set invalid ID
        var idProperty = typeof(Level).GetProperty("Id");
        idProperty!.SetValue(level, "");

        // Act
        var isValid = level.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyName_ShouldReturnFalse()
    {
        // Arrange
        var level = new Level("bronze", "Bronze", "xp", 100L);
        // Use reflection to set invalid name
        var nameProperty = typeof(Level).GetProperty("Name");
        nameProperty!.SetValue(level, "");

        // Act
        var isValid = level.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyCategory_ShouldReturnFalse()
    {
        // Arrange
        var level = new Level("bronze", "Bronze", "xp", 100L);
        // Use reflection to set invalid category
        var categoryProperty = typeof(Level).GetProperty("Category");
        categoryProperty!.SetValue(level, "");

        // Act
        var isValid = level.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNegativeMinPoints_ShouldReturnFalse()
    {
        // Arrange
        var level = new Level("bronze", "Bronze", "xp", 100L);
        // Use reflection to set invalid min points
        var minPointsProperty = typeof(Level).GetProperty("MinPoints");
        minPointsProperty!.SetValue(level, -1L);

        // Act
        var isValid = level.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }
}
