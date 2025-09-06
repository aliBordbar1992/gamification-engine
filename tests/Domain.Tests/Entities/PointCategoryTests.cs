using GamificationEngine.Domain.Entities;
using Shouldly;

namespace GamificationEngine.Domain.Tests.Entities;

/// <summary>
/// Unit tests for PointCategory domain entity
/// </summary>
public class PointCategoryTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreatePointCategory()
    {
        // Arrange
        var id = "xp";
        var name = "Experience Points";
        var description = "Points earned through user activities";
        var aggregation = "sum";

        // Act
        var pointCategory = new PointCategory(id, name, description, aggregation);

        // Assert
        pointCategory.Id.ShouldBe(id);
        pointCategory.Name.ShouldBe(name);
        pointCategory.Description.ShouldBe(description);
        pointCategory.Aggregation.ShouldBe(aggregation);
        pointCategory.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "name", "description", "sum", "ID")]
    [InlineData("id", "", "description", "sum", "Name")]
    [InlineData("id", "name", "", "sum", "Description")]
    [InlineData("id", "name", "description", "", "Aggregation")]
    public void Constructor_WithInvalidParameters_ShouldThrowArgumentException(
        string id, string name, string description, string aggregation, string expectedParam)
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => new PointCategory(id, name, description, aggregation));
        exception.ParamName.ShouldBe(expectedParam.ToLowerInvariant());
    }

    [Fact]
    public void UpdateInfo_WithValidParameters_ShouldUpdateProperties()
    {
        // Arrange
        var pointCategory = new PointCategory("xp", "Experience", "Old description", "sum");
        var newName = "Experience Points";
        var newDescription = "New description";
        var newAggregation = "max";

        // Act
        pointCategory.UpdateInfo(newName, newDescription, newAggregation);

        // Assert
        pointCategory.Name.ShouldBe(newName);
        pointCategory.Description.ShouldBe(newDescription);
        pointCategory.Aggregation.ShouldBe(newAggregation);
        pointCategory.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "description", "sum", "name")]
    [InlineData("name", "", "sum", "description")]
    [InlineData("name", "description", "", "aggregation")]
    public void UpdateInfo_WithInvalidParameters_ShouldThrowArgumentException(
        string name, string description, string aggregation, string expectedParam)
    {
        // Arrange
        var pointCategory = new PointCategory("xp", "Experience", "Description", "sum");

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => pointCategory.UpdateInfo(name, description, aggregation));
        exception.ParamName.ShouldBe(expectedParam.ToLowerInvariant());
    }

    [Theory]
    [InlineData("sum", true)]
    [InlineData("max", true)]
    [InlineData("min", true)]
    [InlineData("avg", true)]
    [InlineData("count", true)]
    [InlineData("SUM", true)]
    [InlineData("Max", true)]
    [InlineData("invalid", false)]
    public void IsValid_WithDifferentAggregations_ShouldReturnCorrectResult(string aggregation, bool expectedValid)
    {
        // Arrange
        var pointCategory = new PointCategory("xp", "Experience", "Description", aggregation);

        // Act
        var isValid = pointCategory.IsValid();

        // Assert
        isValid.ShouldBe(expectedValid);
    }

    [Fact]
    public void IsValid_WithEmptyAggregation_ShouldReturnFalse()
    {
        // Arrange
        var pointCategory = new PointCategory("xp", "Experience", "Description", "sum");
        // Use reflection to set invalid aggregation
        var aggregationProperty = typeof(PointCategory).GetProperty("Aggregation");
        aggregationProperty!.SetValue(pointCategory, "");

        // Act
        var isValid = pointCategory.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyId_ShouldReturnFalse()
    {
        // Arrange
        var pointCategory = new PointCategory("xp", "Experience", "Description", "sum");
        // Use reflection to set invalid ID
        var idProperty = typeof(PointCategory).GetProperty("Id");
        idProperty!.SetValue(pointCategory, "");

        // Act
        var isValid = pointCategory.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyName_ShouldReturnFalse()
    {
        // Arrange
        var pointCategory = new PointCategory("xp", "Experience", "Description", "sum");
        // Use reflection to set invalid name
        var nameProperty = typeof(PointCategory).GetProperty("Name");
        nameProperty!.SetValue(pointCategory, "");

        // Act
        var isValid = pointCategory.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyDescription_ShouldReturnFalse()
    {
        // Arrange
        var pointCategory = new PointCategory("xp", "Experience", "Description", "sum");
        // Use reflection to set invalid description
        var descriptionProperty = typeof(PointCategory).GetProperty("Description");
        descriptionProperty!.SetValue(pointCategory, "");

        // Act
        var isValid = pointCategory.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }
}
