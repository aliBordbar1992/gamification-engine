using GamificationEngine.Domain.Errors;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests;

public class InvalidEventTypeErrorTests
{
    [Fact]
    public void Constructor_WithValidMessage_ShouldCreateError()
    {
        // Arrange
        var message = "Event type 'INVALID_TYPE' is not supported";

        // Act
        var error = new InvalidEventTypeError(message);

        // Assert
        error.ShouldBeOfType<InvalidEventTypeError>();
        error.ShouldBeAssignableTo<DomainError>();
    }

    [Fact]
    public void Constructor_WithValidMessage_ShouldSetCorrectCode()
    {
        // Arrange
        var message = "Event type 'INVALID_TYPE' is not supported";

        // Act
        var error = new InvalidEventTypeError(message);

        // Assert
        error.Code.ShouldBe("INVALID_EVENT_TYPE");
    }

    [Fact]
    public void Constructor_WithValidMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Event type 'INVALID_TYPE' is not supported";

        // Act
        var error = new InvalidEventTypeError(message);

        // Assert
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldSetEmptyMessage()
    {
        // Arrange
        var message = "";

        // Act
        var error = new InvalidEventTypeError(message);

        // Assert
        error.Code.ShouldBe("INVALID_EVENT_TYPE");
        error.Message.ShouldBe("");
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldSetNullMessage()
    {
        // Arrange
        string? message = null;

        // Act
        var error = new InvalidEventTypeError(message);

        // Assert
        error.Code.ShouldBe("INVALID_EVENT_TYPE");
        error.Message.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithLongMessage_ShouldSetLongMessage()
    {
        // Arrange
        var message = "This is a very long error message that contains detailed information about why the event type is invalid, including the specific event type that was provided and all the valid event types that are currently supported by the system";

        // Act
        var error = new InvalidEventTypeError(message);

        // Assert
        error.Code.ShouldBe("INVALID_EVENT_TYPE");
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_ShouldSetSpecialCharacters()
    {
        // Arrange
        var message = "Event type 'USER@COMMENTED' contains invalid characters";

        // Act
        var error = new InvalidEventTypeError(message);

        // Assert
        error.Code.ShouldBe("INVALID_EVENT_TYPE");
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var message = "Event type 'INVALID_TYPE' is not supported";
        var error = new InvalidEventTypeError(message);
        var expected = $"INVALID_EVENT_TYPE: {message}";

        // Act
        var result = error.ToString();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var error = new InvalidEventTypeError("Test message");

        // Act & Assert
        error.Code.ShouldBe("INVALID_EVENT_TYPE");
        error.Message.ShouldBe("Test message");

        // Properties should be read-only (no setters)
        error.GetType().GetProperty("Code")!.CanWrite.ShouldBeFalse();
        error.GetType().GetProperty("Message")!.CanWrite.ShouldBeFalse();
    }

    [Fact]
    public void Error_ShouldInheritFromDomainError()
    {
        // Arrange & Act
        var error = new InvalidEventTypeError("Test");

        // Assert
        error.ShouldBeAssignableTo<DomainError>();
    }

    [Fact]
    public void Error_ShouldBeDifferentFromInvalidEventError()
    {
        // Arrange
        var message = "Test message";
        var invalidEventTypeError = new InvalidEventTypeError(message);
        var invalidEventError = new InvalidEventError(message);

        // Act & Assert
        invalidEventTypeError.ShouldNotBeOfType<InvalidEventError>();
        invalidEventError.ShouldNotBeOfType<InvalidEventTypeError>();
        invalidEventTypeError.Code.ShouldNotBe(invalidEventError.Code);
    }
}