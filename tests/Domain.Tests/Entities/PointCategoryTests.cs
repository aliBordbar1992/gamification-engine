using GamificationEngine.Domain.Entities;
using GamificationEngine.Shared;
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
        var aggregation = PointCategoryAggregation.Sum;

        // Act
        var pointCategory = new PointCategory(id, name, description, aggregation, false, false);

        // Assert
        pointCategory.Id.ShouldBe(id);
        pointCategory.Name.ShouldBe(name);
        pointCategory.Description.ShouldBe(description);
        pointCategory.Aggregation.ShouldBe(aggregation);
        pointCategory.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "name", "description", PointCategoryAggregation.Sum, "ID")]
    [InlineData("id", "", "description", PointCategoryAggregation.Sum, "Name")]
    [InlineData("id", "name", "", PointCategoryAggregation.Sum, "Description")]
    [InlineData("id", "name", "description", PointCategoryAggregation.Sum, "Aggregation")]
    public void Constructor_WithInvalidParameters_ShouldThrowArgumentException(
        string id, string name, string description, PointCategoryAggregation aggregation, string expectedParam)
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => new PointCategory(id, name, description, aggregation, false, false));
        exception.ParamName.ShouldBe(expectedParam.ToLowerInvariant());
    }

    [Fact]
    public void UpdateInfo_WithValidParameters_ShouldUpdateProperties()
    {
        // Arrange
        var pointCategory = new PointCategory("xp", "Experience", "Old description", PointCategoryAggregation.Sum, false, false);
        var newName = "Experience Points";
        var newDescription = "New description";
        var newAggregation = PointCategoryAggregation.Max;

        // Act
        pointCategory.UpdateInfo(newName, newDescription, newAggregation);

        // Assert
        pointCategory.Name.ShouldBe(newName);
        pointCategory.Description.ShouldBe(newDescription);
        pointCategory.Aggregation.ShouldBe(newAggregation);
        pointCategory.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "description", PointCategoryAggregation.Sum, "name")]
    [InlineData("name", "", PointCategoryAggregation.Sum, "description")]
    [InlineData("name", "description", PointCategoryAggregation.Sum, "aggregation")]
    public void UpdateInfo_WithInvalidParameters_ShouldThrowArgumentException(
        string name, string description, PointCategoryAggregation aggregation, string expectedParam)
    {
        // Arrange
        var pointCategory = new PointCategory("xp", "Experience", "Description", aggregation, false, false);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => pointCategory.UpdateInfo(name, description, aggregation));
        exception.ParamName.ShouldBe(expectedParam.ToLowerInvariant());
    }


    [Fact]
    public void IsValid_WithEmptyAggregation_ShouldReturnFalse()
    {
        // Arrange
        var pointCategory = new PointCategory("xp", "Experience", "Description", PointCategoryAggregation.Sum, false, false);
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
        var pointCategory = new PointCategory("xp", "Experience", "Description", PointCategoryAggregation.Sum, false, false);
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
        var pointCategory = new PointCategory("xp", "Experience", "Description", PointCategoryAggregation.Sum, false, false);
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
        var pointCategory = new PointCategory("xp", "Experience", "Description", PointCategoryAggregation.Sum, false, false);
        // Use reflection to set invalid description
        var descriptionProperty = typeof(PointCategory).GetProperty("Description");
        descriptionProperty!.SetValue(pointCategory, "");

        // Act
        var isValid = pointCategory.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }
}
