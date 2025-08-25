using GamificationEngine.Domain.Errors;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests;

public class EventStorageErrorTests
{
    [Fact]
    public void Constructor_WithValidMessage_ShouldCreateError()
    {
        // Arrange
        var message = "Failed to store event in database";

        // Act
        var error = new EventStorageError(message);

        // Assert
        error.ShouldBeOfType<EventStorageError>();
        error.ShouldBeAssignableTo<DomainError>();
    }

    [Fact]
    public void Constructor_WithValidMessage_ShouldSetCorrectCode()
    {
        // Arrange
        var message = "Failed to store event in database";

        // Act
        var error = new EventStorageError(message);

        // Assert
        error.Code.ShouldBe("EVENT_STORAGE_ERROR");
    }

    [Fact]
    public void Constructor_WithValidMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Failed to store event in database";

        // Act
        var error = new EventStorageError(message);

        // Assert
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldSetEmptyMessage()
    {
        // Arrange
        var message = "";

        // Act
        var error = new EventStorageError(message);

        // Assert
        error.Code.ShouldBe("EVENT_STORAGE_ERROR");
        error.Message.ShouldBe("");
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldSetNullMessage()
    {
        // Arrange
        string? message = null;

        // Act
        var error = new EventStorageError(message);

        // Assert
        error.Code.ShouldBe("EVENT_STORAGE_ERROR");
        error.Message.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithLongMessage_ShouldSetLongMessage()
    {
        // Arrange
        var message = "This is a very long error message that contains detailed information about why the event storage failed, including specific database connection details, SQL errors, and any retry attempts that were made";

        // Act
        var error = new EventStorageError(message);

        // Assert
        error.Code.ShouldBe("EVENT_STORAGE_ERROR");
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_ShouldSetSpecialCharacters()
    {
        // Arrange
        var message = "Storage failed: Connection string 'server=db.example.com;port=5432' is invalid";

        // Act
        var error = new EventStorageError(message);

        // Assert
        error.Code.ShouldBe("EVENT_STORAGE_ERROR");
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var message = "Failed to store event in database";
        var error = new EventStorageError(message);
        var expected = $"EVENT_STORAGE_ERROR: {message}";

        // Act
        var result = error.ToString();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var error = new EventStorageError("Test message");

        // Act & Assert
        error.Code.ShouldBe("EVENT_STORAGE_ERROR");
        error.Message.ShouldBe("Test message");

        // Properties should be read-only (no setters)
        error.GetType().GetProperty("Code")!.CanWrite.ShouldBeFalse();
        error.GetType().GetProperty("Message")!.CanWrite.ShouldBeFalse();
    }

    [Fact]
    public void Error_ShouldInheritFromDomainError()
    {
        // Arrange & Act
        var error = new EventStorageError("Test");

        // Assert
        error.ShouldBeAssignableTo<DomainError>();
    }

    [Fact]
    public void Error_ShouldBeDifferentFromOtherEventErrors()
    {
        // Arrange
        var message = "Test message";
        var storageError = new EventStorageError(message);
        var invalidEventError = new InvalidEventError(message);
        var invalidEventTypeError = new InvalidEventTypeError(message);

        // Act & Assert
        storageError.ShouldNotBeOfType<InvalidEventError>();
        storageError.ShouldNotBeOfType<InvalidEventTypeError>();
        storageError.Code.ShouldNotBe(invalidEventError.Code);
        storageError.Code.ShouldNotBe(invalidEventTypeError.Code);
    }
}