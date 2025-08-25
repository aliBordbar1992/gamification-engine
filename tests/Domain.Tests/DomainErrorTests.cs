using GamificationEngine.Domain.Errors;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests;

public class DomainErrorTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetProperties()
    {
        // Arrange
        var code = "TEST_ERROR";
        var message = "This is a test error message";

        // Act
        var error = new TestDomainError(code, message);

        // Assert
        error.Code.ShouldBe(code);
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithEmptyCode_ShouldSetEmptyCode()
    {
        // Arrange
        var code = "";
        var message = "Empty code test";

        // Act
        var error = new TestDomainError(code, message);

        // Assert
        error.Code.ShouldBe("");
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldSetEmptyMessage()
    {
        // Arrange
        var code = "EMPTY_MESSAGE";
        var message = "";

        // Act
        var error = new TestDomainError(code, message);

        // Assert
        error.Code.ShouldBe(code);
        error.Message.ShouldBe("");
    }

    [Fact]
    public void Constructor_WithNullCode_ShouldSetNullCode()
    {
        // Arrange
        string? code = null;
        var message = "Null code test";

        // Act
        var error = new TestDomainError(code, message);

        // Assert
        error.Code.ShouldBeNull();
        error.Message.ShouldBe(message);
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldSetNullMessage()
    {
        // Arrange
        var code = "NULL_MESSAGE";
        string? message = null;

        // Act
        var error = new TestDomainError(code, message);

        // Assert
        error.Code.ShouldBe(code);
        error.Message.ShouldBeNull();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var code = "FORMAT_TEST";
        var message = "Testing ToString format";
        var error = new TestDomainError(code, message);
        var expected = $"{code}: {message}";

        // Act
        var result = error.ToString();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void ToString_WithEmptyCode_ShouldReturnFormattedString()
    {
        // Arrange
        var code = "";
        var message = "Empty code message";
        var error = new TestDomainError(code, message);
        var expected = $": {message}";

        // Act
        var result = error.ToString();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void ToString_WithEmptyMessage_ShouldReturnFormattedString()
    {
        // Arrange
        var code = "EMPTY_MSG";
        var message = "";
        var error = new TestDomainError(code, message);
        var expected = $"{code}: ";

        // Act
        var result = error.ToString();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var error = new TestDomainError("READONLY", "Test");

        // Act & Assert
        error.Code.ShouldBe("READONLY");
        error.Message.ShouldBe("Test");

        // Properties should be read-only (no setters)
        error.GetType().GetProperty("Code")!.CanWrite.ShouldBeFalse();
        error.GetType().GetProperty("Message")!.CanWrite.ShouldBeFalse();
    }

    // Test implementation of abstract DomainError class
    private class TestDomainError : DomainError
    {
        public TestDomainError(string code, string message) : base(code, message)
        {
        }
    }
}