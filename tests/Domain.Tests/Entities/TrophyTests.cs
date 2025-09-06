using GamificationEngine.Domain.Entities;
using Shouldly;

namespace GamificationEngine.Domain.Tests.Entities;

/// <summary>
/// Unit tests for Trophy domain entity
/// </summary>
public class TrophyTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateTrophy()
    {
        // Arrange
        var id = "trophy-badge-collector";
        var name = "Badge Collector";
        var description = "Collect 10 different badges";
        var image = "/assets/trophies/badge_collector.png";
        var visible = true;

        // Act
        var trophy = new Trophy(id, name, description, image, visible);

        // Assert
        trophy.Id.ShouldBe(id);
        trophy.Name.ShouldBe(name);
        trophy.Description.ShouldBe(description);
        trophy.Image.ShouldBe(image);
        trophy.Visible.ShouldBe(visible);
        trophy.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithDefaultVisibility_ShouldCreateTrophyWithVisibleTrue()
    {
        // Arrange
        var id = "trophy-badge-collector";
        var name = "Badge Collector";
        var description = "Collect 10 different badges";
        var image = "/assets/trophies/badge_collector.png";

        // Act
        var trophy = new Trophy(id, name, description, image);

        // Assert
        trophy.Visible.ShouldBeTrue();
        trophy.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "name", "description", "image", "ID")]
    [InlineData("id", "", "description", "image", "Name")]
    [InlineData("id", "name", "", "image", "Description")]
    [InlineData("id", "name", "description", "", "Image")]
    public void Constructor_WithInvalidParameters_ShouldThrowArgumentException(
        string id, string name, string description, string image, string expectedParam)
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => new Trophy(id, name, description, image));
        exception.ParamName.ShouldBe(expectedParam.ToLowerInvariant());
    }

    [Fact]
    public void UpdateInfo_WithValidParameters_ShouldUpdateProperties()
    {
        // Arrange
        var trophy = new Trophy("trophy-badge-collector", "Old Name", "Old description", "/old/image.png", false);
        var newName = "New Name";
        var newDescription = "New description";
        var newImage = "/new/image.png";
        var newVisible = true;

        // Act
        trophy.UpdateInfo(newName, newDescription, newImage, newVisible);

        // Assert
        trophy.Name.ShouldBe(newName);
        trophy.Description.ShouldBe(newDescription);
        trophy.Image.ShouldBe(newImage);
        trophy.Visible.ShouldBe(newVisible);
        trophy.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "description", "image", "name")]
    [InlineData("name", "", "image", "description")]
    [InlineData("name", "description", "", "image")]
    public void UpdateInfo_WithInvalidParameters_ShouldThrowArgumentException(
        string name, string description, string image, string expectedParam)
    {
        // Arrange
        var trophy = new Trophy("trophy-badge-collector", "Name", "Description", "/image.png");

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => trophy.UpdateInfo(name, description, image, true));
        exception.ParamName.ShouldBe(expectedParam.ToLowerInvariant());
    }

    [Fact]
    public void SetVisibility_WithTrue_ShouldSetVisibleToTrue()
    {
        // Arrange
        var trophy = new Trophy("trophy-badge-collector", "Name", "Description", "/image.png", false);

        // Act
        trophy.SetVisibility(true);

        // Assert
        trophy.Visible.ShouldBeTrue();
    }

    [Fact]
    public void SetVisibility_WithFalse_ShouldSetVisibleToFalse()
    {
        // Arrange
        var trophy = new Trophy("trophy-badge-collector", "Name", "Description", "/image.png", true);

        // Act
        trophy.SetVisibility(false);

        // Assert
        trophy.Visible.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var trophy = new Trophy("trophy-badge-collector", "Name", "Description", "/image.png");

        // Act
        var isValid = trophy.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyId_ShouldReturnFalse()
    {
        // Arrange
        var trophy = new Trophy("trophy-badge-collector", "Name", "Description", "/image.png");
        // Use reflection to set invalid ID
        var idProperty = typeof(Trophy).GetProperty("Id");
        idProperty!.SetValue(trophy, "");

        // Act
        var isValid = trophy.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyName_ShouldReturnFalse()
    {
        // Arrange
        var trophy = new Trophy("trophy-badge-collector", "Name", "Description", "/image.png");
        // Use reflection to set invalid name
        var nameProperty = typeof(Trophy).GetProperty("Name");
        nameProperty!.SetValue(trophy, "");

        // Act
        var isValid = trophy.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyDescription_ShouldReturnFalse()
    {
        // Arrange
        var trophy = new Trophy("trophy-badge-collector", "Name", "Description", "/image.png");
        // Use reflection to set invalid description
        var descriptionProperty = typeof(Trophy).GetProperty("Description");
        descriptionProperty!.SetValue(trophy, "");

        // Act
        var isValid = trophy.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyImage_ShouldReturnFalse()
    {
        // Arrange
        var trophy = new Trophy("trophy-badge-collector", "Name", "Description", "/image.png");
        // Use reflection to set invalid image
        var imageProperty = typeof(Trophy).GetProperty("Image");
        imageProperty!.SetValue(trophy, "");

        // Act
        var isValid = trophy.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }
}
