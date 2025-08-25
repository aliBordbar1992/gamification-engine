using GamificationEngine.Domain.Errors;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests;

public class EventRetrievalErrorTests
{
    [Fact]
    public void Constructor_WithValidMessage_ShouldCreateError()
    {
        // Arrange
        var message = "Failed to retrieve events from database";

        // Act
        var error = new EventRetrievalError(message);

        // Assert
        error.ShouldBeOfType<EventRetrievalError>();
        error.ShouldBeAssignableTo<DomainError>();
    }

    [Fact]
    public void Constructor_WithValidMessage_ShouldSetCorrectCode()
    {
        // Arrange
        var message = "Failed to retrieve events from database";

        // Act
        var error = new EventRetrievalError(message);

        // Assert
        error.Code.ShouldBe("EVENT_RETRIEVAL_ERROR");
    }

    [Fact]
    public void Constructor_WithValidMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Failed to retrieve events from database";

        // Act
        var error = new EventRetrievalError(message);

        // Assert
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldSetEmptyMessage()
    {
        // Arrange
        var message = "";

        // Act
        var error = new EventRetrievalError(message);

        // Assert
        error.Code.ShouldBe("EVENT_RETRIEVAL_ERROR");
        error.Message.ShouldBe("");
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldSetNullMessage()
    {
        // Arrange
        string? message = null;

        // Act
        var error = new EventRetrievalError(message);

        // Assert
        error.Code.ShouldBe("EVENT_RETRIEVAL_ERROR");
        error.Message.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithLongMessage_ShouldSetLongMessage()
    {
        // Arrange
        var message = "This is a very long error message that contains detailed information about why the event retrieval failed, including specific database connection details, SQL errors, timeout information, and any retry attempts that were made";

        // Act
        var error = new EventRetrievalError(message);

        // Assert
        error.Code.ShouldBe("EVENT_RETRIEVAL_ERROR");
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_ShouldSetSpecialCharacters()
    {
        // Arrange
        var message = "Retrieval failed: Query 'SELECT * FROM events WHERE user_id = 'user@example.com'' failed";

        // Act
        var error = new EventRetrievalError(message);

        // Assert
        error.Code.ShouldBe("EVENT_RETRIEVAL_ERROR");
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var message = "Failed to retrieve events from database";
        var error = new EventRetrievalError(message);
        var expected = $"EVENT_RETRIEVAL_ERROR: {message}";

        // Act
        var result = error.ToString();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var error = new EventRetrievalError("Test message");

        // Act & Assert
        error.Code.ShouldBe("EVENT_RETRIEVAL_ERROR");
        error.Message.ShouldBe("Test message");

        // Properties should be read-only (no setters)
        error.GetType().GetProperty("Code")!.CanWrite.ShouldBeFalse();
        error.GetType().GetProperty("Message")!.CanWrite.ShouldBeFalse();
    }

    [Fact]
    public void Error_ShouldInheritFromDomainError()
    {
        // Arrange & Act
        var error = new EventRetrievalError("Test");

        // Assert
        error.ShouldBeAssignableTo<DomainError>();
    }

    [Fact]
    public void Error_ShouldBeDifferentFromOtherEventErrors()
    {
        // Arrange
        var message = "Test message";
        var retrievalError = new EventRetrievalError(message);
        var storageError = new EventStorageError(message);
        var invalidEventError = new InvalidEventError(message);
        var invalidEventTypeError = new InvalidEventTypeError(message);

        // Act & Assert
        retrievalError.ShouldNotBeOfType<EventStorageError>();
        retrievalError.ShouldNotBeOfType<InvalidEventError>();
        retrievalError.ShouldNotBeOfType<InvalidEventTypeError>();
        retrievalError.Code.ShouldNotBe(storageError.Code);
        retrievalError.Code.ShouldNotBe(invalidEventError.Code);
        retrievalError.Code.ShouldNotBe(invalidEventTypeError.Code);
    }

    [Fact]
    public void Error_ShouldBeDifferentFromStorageError()
    {
        // Arrange
        var message = "Test message";
        var retrievalError = new EventRetrievalError(message);
        var storageError = new EventStorageError(message);

        // Act & Assert
        retrievalError.ShouldNotBeOfType<EventStorageError>();
        storageError.ShouldNotBeOfType<EventRetrievalError>();
        retrievalError.Code.ShouldNotBe(storageError.Code);
    }
}