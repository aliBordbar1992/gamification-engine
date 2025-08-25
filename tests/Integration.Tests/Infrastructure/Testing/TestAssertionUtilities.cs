using System.Text.Json;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;
using Shouldly;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Utility class providing custom assertion helpers for integration tests
/// </summary>
public static class TestAssertionUtilities
{
    /// <summary>
    /// Asserts that a JSON response contains the expected properties
    /// </summary>
    public static void AssertJsonResponse(HttpResponseMessage response, string expectedContentType = "application/json")
    {
        response.ShouldNotBeNull();
        response.IsSuccessStatusCode.ShouldBeTrue($"Expected success status, got {response.StatusCode}");
        response.Content.Headers.ContentType?.MediaType.ShouldBe(expectedContentType);

        var content = response.Content.ReadAsStringAsync().Result;
        content.ShouldNotBeNullOrEmpty();

        // Verify it's valid JSON
        Should.NotThrow(() => JsonDocument.Parse(content), "Response should be valid JSON");
    }

    /// <summary>
    /// Asserts that a JSON response contains a specific property with expected value
    /// </summary>
    public static void AssertJsonProperty<T>(HttpResponseMessage response, string propertyName, T expectedValue)
    {
        var content = response.Content.ReadAsStringAsync().Result;
        var jsonDoc = JsonDocument.Parse(content);

        var property = jsonDoc.RootElement.GetProperty(propertyName);
        var actualValue = JsonSerializer.Deserialize<T>(property.GetRawText());

        actualValue.ShouldBe(expectedValue, $"Property '{propertyName}' should have value '{expectedValue}'");
    }

    /// <summary>
    /// Asserts that a JSON response contains an array with expected count
    /// </summary>
    public static void AssertJsonArrayCount(HttpResponseMessage response, string arrayPropertyName, int expectedCount)
    {
        var content = response.Content.ReadAsStringAsync().Result;
        var jsonDoc = JsonDocument.Parse(content);

        var array = jsonDoc.RootElement.GetProperty(arrayPropertyName);
        array.ValueKind.ShouldBe(JsonValueKind.Array);
        array.GetArrayLength().ShouldBe(expectedCount, $"Array '{arrayPropertyName}' should have {expectedCount} elements");
    }

    /// <summary>
    /// Asserts that an event has the expected properties
    /// </summary>
    public static void AssertEventProperties(Event @event, string expectedUserId, string expectedEventType)
    {
        @event.ShouldNotBeNull();
        @event.UserId.ShouldBe(expectedUserId);
        @event.EventType.ShouldBe(expectedEventType);
        @event.OccurredAt.ShouldBeGreaterThan(DateTimeOffset.MinValue);
        @event.Attributes.ShouldNotBeNull();
    }

    /// <summary>
    /// Asserts that a user state has the expected properties
    /// </summary>
    public static void AssertUserStateProperties(UserState userState, string expectedUserId, Dictionary<string, long> expectedPointsByCategory)
    {
        userState.ShouldNotBeNull();
        userState.UserId.ShouldBe(expectedUserId);
        userState.PointsByCategory.ShouldBe(expectedPointsByCategory);
        userState.Badges.ShouldNotBeNull();
    }

    /// <summary>
    /// Asserts that an HTTP response has the expected status code
    /// </summary>
    public static void AssertStatusCode(HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode)
    {
        response.StatusCode.ShouldBe(expectedStatusCode,
            $"Expected status code {expectedStatusCode}, got {response.StatusCode}");
    }

    /// <summary>
    /// Asserts that an HTTP response contains the expected header
    /// </summary>
    public static void AssertHeader(HttpResponseMessage response, string headerName, string expectedValue)
    {
        response.Headers.Contains(headerName).ShouldBeTrue($"Response should contain header '{headerName}'");
        var headerValue = response.Headers.GetValues(headerName).FirstOrDefault();
        headerValue.ShouldBe(expectedValue, $"Header '{headerName}' should have value '{expectedValue}'");
    }

    /// <summary>
    /// Asserts that a collection contains the expected number of items
    /// </summary>
    public static void AssertCollectionCount<T>(IEnumerable<T> collection, int expectedCount, string collectionName = "Collection")
    {
        collection.ShouldNotBeNull();
        collection.Count().ShouldBe(expectedCount, $"{collectionName} should contain {expectedCount} items");
    }

    /// <summary>
    /// Asserts that a collection contains an item matching the predicate
    /// </summary>
    public static void AssertCollectionContains<T>(IEnumerable<T> collection, Func<T, bool> predicate, string collectionName = "Collection")
    {
        collection.ShouldNotBeNull();
        collection.Any(predicate).ShouldBeTrue($"{collectionName} should contain an item matching the predicate");
    }

    /// <summary>
    /// Asserts that a JSON response contains an error message
    /// </summary>
    public static void AssertErrorResponse(HttpResponseMessage response, string? expectedErrorMessage = null)
    {
        response.ShouldNotBeNull();
        response.IsSuccessStatusCode.ShouldBeFalse("Response should indicate an error");

        if (expectedErrorMessage != null)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            content.ShouldContain(expectedErrorMessage);
        }
    }
}