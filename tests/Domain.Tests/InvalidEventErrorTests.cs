using GamificationEngine.Domain.Errors;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests;

public class InvalidEventErrorTests
{
    [Fact]
    public void Constructor_WithValidMessage_ShouldCreateError()
    {
        // Arrange
        var message = "Event validation failed";

        // Act
        var error = new InvalidEventError(message);

        // Assert
        error.ShouldBeOfType<InvalidEventError>();
        error.ShouldBeAssignableTo<DomainError>();
    }

    [Fact]
    public void Constructor_WithValidMessage_ShouldSetCorrectCode()
    {
        // Arrange
        var message = "Event validation failed";

        // Act
        var error = new InvalidEventError(message);

        // Assert
        error.Code.ShouldBe("INVALID_EVENT");
    }

    [Fact]
    public void Constructor_WithValidMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Event validation failed";

        // Act
        var error = new InvalidEventError(message);

        // Assert
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldSetEmptyMessage()
    {
        // Arrange
        var message = "";

        // Act
        var error = new InvalidEventError(message);

        // Assert
        error.Code.ShouldBe("INVALID_EVENT");
        error.Message.ShouldBe("");
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldSetNullMessage()
    {
        // Arrange
        string? message = null;

        // Act
        var error = new InvalidEventError(message);

        // Assert
        error.Code.ShouldBe("INVALID_EVENT");
        error.Message.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithLongMessage_ShouldSetLongMessage()
    {
        // Arrange
        var message = "This is a very long error message that contains detailed information about why the event validation failed, including specific field names and validation rules that were violated";

        // Act
        var error = new InvalidEventError(message);

        // Assert
        error.Code.ShouldBe("INVALID_EVENT");
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_ShouldSetSpecialCharacters()
    {
        // Arrange
        var message = "Event validation failed: Invalid characters in field 'user@email.com'";

        // Act
        var error = new InvalidEventError(message);

        // Assert
        error.Code.ShouldBe("INVALID_EVENT");
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var message = "Event validation failed";
        var error = new InvalidEventError(message);
        var expected = $"INVALID_EVENT: {message}";

        // Act
        var result = error.ToString();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var error = new InvalidEventError("Test message");

        // Act & Assert
        error.Code.ShouldBe("INVALID_EVENT");
        error.Message.ShouldBe("Test message");

        // Properties should be read-only (no setters)
        error.GetType().GetProperty("Code")!.CanWrite.ShouldBeFalse();
        error.GetType().GetProperty("Message")!.CanWrite.ShouldBeFalse();
    }

    [Fact]
    public void Error_ShouldInheritFromDomainError()
    {
        // Arrange & Act
        var error = new InvalidEventError("Test");

        // Assert
        error.ShouldBeAssignableTo<DomainError>();
    }
}