using GamificationEngine.Domain.Entities;
using Shouldly;

namespace GamificationEngine.Domain.Tests.Entities;

/// <summary>
/// Unit tests for Badge domain entity
/// </summary>
public class BadgeTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateBadge()
    {
        // Arrange
        var id = "badge-commenter";
        var name = "First Comment";
        var description = "Awarded when user posts their first comment";
        var image = "/assets/badges/commenter.png";
        var visible = true;

        // Act
        var badge = new Badge(id, name, description, image, visible);

        // Assert
        badge.Id.ShouldBe(id);
        badge.Name.ShouldBe(name);
        badge.Description.ShouldBe(description);
        badge.Image.ShouldBe(image);
        badge.Visible.ShouldBe(visible);
        badge.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithDefaultVisibility_ShouldCreateBadgeWithVisibleTrue()
    {
        // Arrange
        var id = "badge-commenter";
        var name = "First Comment";
        var description = "Awarded when user posts their first comment";
        var image = "/assets/badges/commenter.png";

        // Act
        var badge = new Badge(id, name, description, image);

        // Assert
        badge.Visible.ShouldBeTrue();
        badge.IsValid().ShouldBeTrue();
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
        var exception = Should.Throw<ArgumentException>(() => new Badge(id, name, description, image));
        exception.ParamName.ShouldBe(expectedParam.ToLowerInvariant());
    }

    [Fact]
    public void UpdateInfo_WithValidParameters_ShouldUpdateProperties()
    {
        // Arrange
        var badge = new Badge("badge-commenter", "Old Name", "Old description", "/old/image.png", false);
        var newName = "New Name";
        var newDescription = "New description";
        var newImage = "/new/image.png";
        var newVisible = true;

        // Act
        badge.UpdateInfo(newName, newDescription, newImage, newVisible);

        // Assert
        badge.Name.ShouldBe(newName);
        badge.Description.ShouldBe(newDescription);
        badge.Image.ShouldBe(newImage);
        badge.Visible.ShouldBe(newVisible);
        badge.IsValid().ShouldBeTrue();
    }

    [Theory]
    [InlineData("", "description", "image", "name")]
    [InlineData("name", "", "image", "description")]
    [InlineData("name", "description", "", "image")]
    public void UpdateInfo_WithInvalidParameters_ShouldThrowArgumentException(
        string name, string description, string image, string expectedParam)
    {
        // Arrange
        var badge = new Badge("badge-commenter", "Name", "Description", "/image.png");

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => badge.UpdateInfo(name, description, image, true));
        exception.ParamName.ShouldBe(expectedParam.ToLowerInvariant());
    }

    [Fact]
    public void SetVisibility_WithTrue_ShouldSetVisibleToTrue()
    {
        // Arrange
        var badge = new Badge("badge-commenter", "Name", "Description", "/image.png", false);

        // Act
        badge.SetVisibility(true);

        // Assert
        badge.Visible.ShouldBeTrue();
    }

    [Fact]
    public void SetVisibility_WithFalse_ShouldSetVisibleToFalse()
    {
        // Arrange
        var badge = new Badge("badge-commenter", "Name", "Description", "/image.png", true);

        // Act
        badge.SetVisibility(false);

        // Assert
        badge.Visible.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var badge = new Badge("badge-commenter", "Name", "Description", "/image.png");

        // Act
        var isValid = badge.IsValid();

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyId_ShouldReturnFalse()
    {
        // Arrange
        var badge = new Badge("badge-commenter", "Name", "Description", "/image.png");
        // Use reflection to set invalid ID
        var idProperty = typeof(Badge).GetProperty("Id");
        idProperty!.SetValue(badge, "");

        // Act
        var isValid = badge.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyName_ShouldReturnFalse()
    {
        // Arrange
        var badge = new Badge("badge-commenter", "Name", "Description", "/image.png");
        // Use reflection to set invalid name
        var nameProperty = typeof(Badge).GetProperty("Name");
        nameProperty!.SetValue(badge, "");

        // Act
        var isValid = badge.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyDescription_ShouldReturnFalse()
    {
        // Arrange
        var badge = new Badge("badge-commenter", "Name", "Description", "/image.png");
        // Use reflection to set invalid description
        var descriptionProperty = typeof(Badge).GetProperty("Description");
        descriptionProperty!.SetValue(badge, "");

        // Act
        var isValid = badge.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithEmptyImage_ShouldReturnFalse()
    {
        // Arrange
        var badge = new Badge("badge-commenter", "Name", "Description", "/image.png");
        // Use reflection to set invalid image
        var imageProperty = typeof(Badge).GetProperty("Image");
        imageProperty!.SetValue(badge, "");

        // Act
        var isValid = badge.IsValid();

        // Assert
        isValid.ShouldBeFalse();
    }
}
