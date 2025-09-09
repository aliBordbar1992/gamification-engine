using GamificationEngine.Domain.Entities;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests.Entities;

/// <summary>
/// Unit tests for EventDefinition entity
/// </summary>
public class EventDefinitionTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var id = "USER_COMMENTED";
        var description = "User commented on a post";
        var payloadSchema = new Dictionary<string, string>
        {
            { "commentId", "string" },
            { "postId", "string" }
        };

        // Act
        var eventDefinition = new EventDefinition(id, description, payloadSchema);

        // Assert
        eventDefinition.Id.ShouldBe(id);
        eventDefinition.Description.ShouldBe(description);
        eventDefinition.PayloadSchema.ShouldBe(payloadSchema);
    }

    [Fact]
    public void Constructor_WithNullPayloadSchema_ShouldCreateInstanceWithEmptySchema()
    {
        // Arrange
        var id = "USER_COMMENTED";
        var description = "User commented on a post";

        // Act
        var eventDefinition = new EventDefinition(id, description, null);

        // Assert
        eventDefinition.Id.ShouldBe(id);
        eventDefinition.Description.ShouldBe(description);
        eventDefinition.PayloadSchema.ShouldNotBeNull();
        eventDefinition.PayloadSchema.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new EventDefinition("", "description", null));
    }

    [Fact]
    public void Constructor_WithWhitespaceId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new EventDefinition("   ", "description", null));
    }

    [Fact]
    public void Constructor_WithEmptyDescription_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new EventDefinition("id", "", null));
    }

    [Fact]
    public void Constructor_WithWhitespaceDescription_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new EventDefinition("id", "   ", null));
    }

    [Fact]
    public void ValidatePayload_WithNullSchema_ShouldReturnTrue()
    {
        // Arrange
        var eventDefinition = new EventDefinition("USER_COMMENTED", "description", null);
        var attributes = new Dictionary<string, object> { { "anyField", "anyValue" } };

        // Act
        var result = eventDefinition.ValidatePayload(attributes);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePayload_WithEmptySchema_ShouldReturnTrue()
    {
        // Arrange
        var eventDefinition = new EventDefinition("USER_COMMENTED", "description", new Dictionary<string, string>());
        var attributes = new Dictionary<string, object> { { "anyField", "anyValue" } };

        // Act
        var result = eventDefinition.ValidatePayload(attributes);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePayload_WithNullAttributes_ShouldReturnFalse()
    {
        // Arrange
        var payloadSchema = new Dictionary<string, string> { { "commentId", "string" } };
        var eventDefinition = new EventDefinition("USER_COMMENTED", "description", payloadSchema);

        // Act
        var result = eventDefinition.ValidatePayload(null);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ValidatePayload_WithMissingRequiredField_ShouldReturnFalse()
    {
        // Arrange
        var payloadSchema = new Dictionary<string, string>
        {
            { "commentId", "string" },
            { "postId", "string" }
        };
        var eventDefinition = new EventDefinition("USER_COMMENTED", "description", payloadSchema);
        var attributes = new Dictionary<string, object> { { "commentId", "comment123" } }; // Missing postId

        // Act
        var result = eventDefinition.ValidatePayload(attributes);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ValidatePayload_WithAllRequiredFields_ShouldReturnTrue()
    {
        // Arrange
        var payloadSchema = new Dictionary<string, string>
        {
            { "commentId", "string" },
            { "postId", "string" }
        };
        var eventDefinition = new EventDefinition("USER_COMMENTED", "description", payloadSchema);
        var attributes = new Dictionary<string, object>
        {
            { "commentId", "comment123" },
            { "postId", "post456" }
        };

        // Act
        var result = eventDefinition.ValidatePayload(attributes);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePayload_WithExtraFields_ShouldReturnTrue()
    {
        // Arrange
        var payloadSchema = new Dictionary<string, string>
        {
            { "commentId", "string" },
            { "postId", "string" }
        };
        var eventDefinition = new EventDefinition("USER_COMMENTED", "description", payloadSchema);
        var attributes = new Dictionary<string, object>
        {
            { "commentId", "comment123" },
            { "postId", "post456" },
            { "extraField", "extraValue" } // Extra field should be allowed
        };

        // Act
        var result = eventDefinition.ValidatePayload(attributes);

        // Assert
        result.ShouldBeTrue();
    }
}
