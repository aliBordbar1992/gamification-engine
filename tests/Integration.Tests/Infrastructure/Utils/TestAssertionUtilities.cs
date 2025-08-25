using Shouldly;

namespace GamificationEngine.Integration.Tests.Infrastructure.Utils;

/// <summary>
/// Utility class providing general-purpose assertion helpers for integration tests
/// This class focuses on HTTP response validation and generic collection assertions
/// </summary>
public static class TestAssertionUtilities
{
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
    /// Asserts that an HTTP response contains a header with any value
    /// </summary>
    public static void AssertHeaderExists(HttpResponseMessage response, string headerName)
    {
        response.Headers.Contains(headerName).ShouldBeTrue($"Response should contain header '{headerName}'");
    }

    /// <summary>
    /// Asserts that an HTTP response does not contain a specific header
    /// </summary>
    public static void AssertHeaderDoesNotExist(HttpResponseMessage response, string headerName)
    {
        response.Headers.Contains(headerName).ShouldBeFalse($"Response should not contain header '{headerName}'");
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
    /// Asserts that a collection does not contain an item matching the predicate
    /// </summary>
    public static void AssertCollectionDoesNotContain<T>(IEnumerable<T> collection, Func<T, bool> predicate, string collectionName = "Collection")
    {
        collection.ShouldNotBeNull();
        collection.Any(predicate).ShouldBeFalse($"{collectionName} should not contain an item matching the predicate");
    }

    /// <summary>
    /// Asserts that a collection is empty
    /// </summary>
    public static void AssertCollectionIsEmpty<T>(IEnumerable<T> collection, string collectionName = "Collection")
    {
        collection.ShouldNotBeNull();
        collection.Any().ShouldBeFalse($"{collectionName} should be empty");
    }

    /// <summary>
    /// Asserts that a collection is not empty
    /// </summary>
    public static void AssertCollectionIsNotEmpty<T>(IEnumerable<T> collection, string collectionName = "Collection")
    {
        collection.ShouldNotBeNull();
        collection.Any().ShouldBeTrue($"{collectionName} should not be empty");
    }

    /// <summary>
    /// Asserts that all items in a collection match the predicate
    /// </summary>
    public static void AssertAllItemsMatch<T>(IEnumerable<T> collection, Func<T, bool> predicate, string collectionName = "Collection")
    {
        collection.ShouldNotBeNull();
        collection.All(predicate).ShouldBeTrue($"All items in {collectionName} should match the predicate");
    }

    /// <summary>
    /// Asserts that a collection has exactly one item matching the predicate
    /// </summary>
    public static void AssertSingleItemMatches<T>(IEnumerable<T> collection, Func<T, bool> predicate, string collectionName = "Collection")
    {
        collection.ShouldNotBeNull();
        collection.Count(predicate).ShouldBe(1, $"{collectionName} should contain exactly one item matching the predicate");
    }

    /// <summary>
    /// Asserts that an HTTP response is successful (2xx status code)
    /// </summary>
    public static void AssertSuccessResponse(HttpResponseMessage response)
    {
        response.ShouldNotBeNull();
        response.IsSuccessStatusCode.ShouldBeTrue($"Expected success status, got {response.StatusCode}");
    }

    /// <summary>
    /// Asserts that an HTTP response indicates a client error (4xx status code)
    /// </summary>
    public static void AssertClientErrorResponse(HttpResponseMessage response)
    {
        response.ShouldNotBeNull();
        var statusCode = (int)response.StatusCode;
        statusCode.ShouldBeGreaterThanOrEqualTo(400, $"Expected client error status (4xx), got {response.StatusCode}");
        statusCode.ShouldBeLessThan(500, $"Expected client error status (4xx), got {response.StatusCode}");
    }

    /// <summary>
    /// Asserts that an HTTP response indicates a server error (5xx status code)
    /// </summary>
    public static void AssertServerErrorResponse(HttpResponseMessage response)
    {
        response.ShouldNotBeNull();
        var statusCode = (int)response.StatusCode;
        statusCode.ShouldBeGreaterThanOrEqualTo(500, $"Expected server error status (5xx), got {response.StatusCode}");
        statusCode.ShouldBeLessThan(600, $"Expected server error status (5xx), got {response.StatusCode}");
    }

    /// <summary>
    /// Asserts that an HTTP response has the expected content length
    /// </summary>
    public static void AssertContentLength(HttpResponseMessage response, long expectedLength)
    {
        response.ShouldNotBeNull();
        response.Content.Headers.ContentLength.ShouldBe(expectedLength,
            $"Response should have content length {expectedLength}");
    }

    /// <summary>
    /// Asserts that an HTTP response has content
    /// </summary>
    public static void AssertHasContent(HttpResponseMessage response)
    {
        response.ShouldNotBeNull();
        response.Content.ShouldNotBeNull("Response should have content");
    }

    /// <summary>
    /// Asserts that an HTTP response has no content
    /// </summary>
    public static void AssertNoContent(HttpResponseMessage response)
    {
        response.ShouldNotBeNull();
        response.Content.ShouldBeNull("Response should not have content");
    }
}