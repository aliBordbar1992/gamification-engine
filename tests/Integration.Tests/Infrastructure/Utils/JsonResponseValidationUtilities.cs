using Shouldly;
using System.Text.Json;

namespace GamificationEngine.Integration.Tests.Infrastructure.Utils;

/// <summary>
/// Utility class providing JSON response validation utilities for integration tests
/// </summary>
public static class JsonResponseValidationUtilities
{
    /// <summary>
    /// Asserts that a JSON response matches the expected schema structure
    /// </summary>
    public static void AssertJsonSchema(HttpResponseMessage response, JsonSchema expectedSchema)
    {
        response.ShouldNotBeNull();
        response.IsSuccessStatusCode.ShouldBeTrue($"Expected success status, got {response.StatusCode}");

        var content = response.Content.ReadAsStringAsync().Result;
        content.ShouldNotBeNullOrEmpty();

        var jsonDoc = JsonDocument.Parse(content);
        AssertJsonElementSchema(jsonDoc.RootElement, expectedSchema);
    }

    /// <summary>
    /// Asserts that a JSON element matches the expected schema structure
    /// </summary>
    public static void AssertJsonElementSchema(JsonElement element, JsonSchema expectedSchema)
    {
        expectedSchema.ShouldNotBeNull();

        // Check if element type matches expected type
        if (expectedSchema.ExpectedType.HasValue)
        {
            element.ValueKind.ShouldBe(expectedSchema.ExpectedType.Value,
                $"JSON element should be of type {expectedSchema.ExpectedType.Value}");
        }

        // Check required properties for objects
        if (element.ValueKind == JsonValueKind.Object && expectedSchema.RequiredProperties != null)
        {
            foreach (var requiredProperty in expectedSchema.RequiredProperties)
            {
                element.TryGetProperty(requiredProperty, out var property).ShouldBeTrue(
                    $"JSON object should contain required property '{requiredProperty}'");
            }
        }

        // Check property schemas for objects
        if (element.ValueKind == JsonValueKind.Object && expectedSchema.PropertySchemas != null)
        {
            foreach (var propertySchema in expectedSchema.PropertySchemas)
            {
                if (element.TryGetProperty(propertySchema.Key, out var property))
                {
                    AssertJsonElementSchema(property, propertySchema.Value);
                }
                else if (propertySchema.Value.IsRequired)
                {
                    throw new ShouldAssertException($"JSON object should contain required property '{propertySchema.Key}'");
                }
            }
        }

        // Check array schemas for arrays
        if (element.ValueKind == JsonValueKind.Array && expectedSchema.ArrayItemSchema != null)
        {
            var array = element.EnumerateArray().ToList();
            foreach (var item in array)
            {
                AssertJsonElementSchema(item, expectedSchema.ArrayItemSchema);
            }
        }

        // Check minimum/maximum values for numbers
        if (element.ValueKind == JsonValueKind.Number && expectedSchema.MinValue.HasValue)
        {
            var value = element.GetDouble();
            value.ShouldBeGreaterThanOrEqualTo(expectedSchema.MinValue.Value,
                $"JSON number should be greater than or equal to {expectedSchema.MinValue.Value}");
        }

        if (element.ValueKind == JsonValueKind.Number && expectedSchema.MaxValue.HasValue)
        {
            var value = element.GetDouble();
            value.ShouldBeLessThanOrEqualTo(expectedSchema.MaxValue.Value,
                $"JSON number should be less than or equal to {expectedSchema.MaxValue.Value}");
        }

        // Check minimum/maximum lengths for strings
        if (element.ValueKind == JsonValueKind.String && expectedSchema.MinLength.HasValue)
        {
            var value = element.GetString();
            value!.Length.ShouldBeGreaterThanOrEqualTo(expectedSchema.MinLength.Value,
                $"JSON string should have length greater than or equal to {expectedSchema.MinLength.Value}");
        }

        if (element.ValueKind == JsonValueKind.String && expectedSchema.MaxLength.HasValue)
        {
            var value = element.GetString();
            value!.Length.ShouldBeLessThanOrEqualTo(expectedSchema.MaxLength.Value,
                $"JSON string should have length less than or equal to {expectedSchema.MaxLength.Value}");
        }

        // Check array length constraints
        if (element.ValueKind == JsonValueKind.Array && expectedSchema.MinArrayLength.HasValue)
        {
            var arrayLength = element.GetArrayLength();
            arrayLength.ShouldBeGreaterThanOrEqualTo(expectedSchema.MinArrayLength.Value,
                $"JSON array should have length greater than or equal to {expectedSchema.MinArrayLength.Value}");
        }

        if (element.ValueKind == JsonValueKind.Array && expectedSchema.MaxArrayLength.HasValue)
        {
            var arrayLength = element.GetArrayLength();
            arrayLength.ShouldBeLessThanOrEqualTo(expectedSchema.MaxArrayLength.Value,
                $"JSON array should have length less than or equal to {expectedSchema.MaxArrayLength.Value}");
        }
    }

    /// <summary>
    /// Asserts that a JSON response contains a specific property path with expected value
    /// </summary>
    public static void AssertJsonPropertyPath<T>(HttpResponseMessage response, string propertyPath, T expectedValue)
    {
        response.ShouldNotBeNull();

        var content = response.Content.ReadAsStringAsync().Result;
        content.ShouldNotBeNullOrEmpty();

        var jsonDoc = JsonDocument.Parse(content);
        var element = GetJsonElementByPath(jsonDoc.RootElement, propertyPath);

        element.ValueKind.ShouldNotBe(JsonValueKind.Undefined, $"Property path '{propertyPath}' should exist in JSON response");

        var actualValue = JsonSerializer.Deserialize<T>(element.GetRawText());
        actualValue.ShouldBe(expectedValue, $"Property path '{propertyPath}' should have value '{expectedValue}'");
    }

    /// <summary>
    /// Asserts that a JSON response contains a specific property path
    /// </summary>
    public static void AssertJsonPropertyPathExists(HttpResponseMessage response, string propertyPath)
    {
        response.ShouldNotBeNull();

        var content = response.Content.ReadAsStringAsync().Result;
        content.ShouldNotBeNullOrEmpty();

        var jsonDoc = JsonDocument.Parse(content);
        var element = GetJsonElementByPath(jsonDoc.RootElement, propertyPath);

        element.ValueKind.ShouldNotBe(JsonValueKind.Undefined, $"Property path '{propertyPath}' should exist in JSON response");
    }

    /// <summary>
    /// Asserts that a JSON response does not contain a specific property path
    /// </summary>
    public static void AssertJsonPropertyPathDoesNotExist(HttpResponseMessage response, string propertyPath)
    {
        response.ShouldNotBeNull();

        var content = response.Content.ReadAsStringAsync().Result;
        content.ShouldNotBeNullOrEmpty();

        var jsonDoc = JsonDocument.Parse(content);
        var element = GetJsonElementByPath(jsonDoc.RootElement, propertyPath);

        element.ValueKind.ShouldBe(JsonValueKind.Undefined, $"Property path '{propertyPath}' should not exist in JSON response");
    }

    /// <summary>
    /// Asserts that a JSON response contains an array at the specified path with expected count
    /// </summary>
    public static void AssertJsonArrayAtPath(HttpResponseMessage response, string propertyPath, int expectedCount)
    {
        response.ShouldNotBeNull();

        var content = response.Content.ReadAsStringAsync().Result;
        content.ShouldNotBeNullOrEmpty();

        var jsonDoc = JsonDocument.Parse(content);
        var element = GetJsonElementByPath(jsonDoc.RootElement, propertyPath);

        element.ValueKind.ShouldNotBe(JsonValueKind.Undefined, $"Property path '{propertyPath}' should exist in JSON response");
        element.ValueKind.ShouldBe(JsonValueKind.Array, $"Property path '{propertyPath}' should be an array");
        element.GetArrayLength().ShouldBe(expectedCount, $"Array at path '{propertyPath}' should have {expectedCount} elements");
    }

    /// <summary>
    /// Asserts that a JSON response contains an object at the specified path
    /// </summary>
    public static void AssertJsonObjectAtPath(HttpResponseMessage response, string propertyPath)
    {
        response.ShouldNotBeNull();

        var content = response.Content.ReadAsStringAsync().Result;
        content.ShouldNotBeNullOrEmpty();

        var jsonDoc = JsonDocument.Parse(content);
        var element = GetJsonElementByPath(jsonDoc.RootElement, propertyPath);

        element.ValueKind.ShouldNotBe(JsonValueKind.Undefined, $"Property path '{propertyPath}' should exist in JSON response");
        element.ValueKind.ShouldBe(JsonValueKind.Object, $"Property path '{propertyPath}' should be an object");
    }

    /// <summary>
    /// Asserts that a JSON response contains a string at the specified path with expected value
    /// </summary>
    public static void AssertJsonStringAtPath(HttpResponseMessage response, string propertyPath, string expectedValue)
    {
        response.ShouldNotBeNull();

        var content = response.Content.ReadAsStringAsync().Result;
        content.ShouldNotBeNullOrEmpty();

        var jsonDoc = JsonDocument.Parse(content);
        var element = GetJsonElementByPath(jsonDoc.RootElement, propertyPath);

        element.ValueKind.ShouldNotBe(JsonValueKind.Undefined, $"Property path '{propertyPath}' should exist in JSON response");
        element.ValueKind.ShouldBe(JsonValueKind.String, $"Property path '{propertyPath}' should be a string");
        element.GetString().ShouldBe(expectedValue, $"String at path '{propertyPath}' should have value '{expectedValue}'");
    }

    /// <summary>
    /// Asserts that a JSON response contains a number at the specified path with expected value
    /// </summary>
    public static void AssertJsonNumberAtPath(HttpResponseMessage response, string propertyPath, double expectedValue)
    {
        response.ShouldNotBeNull();

        var content = response.Content.ReadAsStringAsync().Result;
        content.ShouldNotBeNullOrEmpty();

        var jsonDoc = JsonDocument.Parse(content);
        var element = GetJsonElementByPath(jsonDoc.RootElement, propertyPath);

        element.ValueKind.ShouldNotBe(JsonValueKind.Undefined, $"Property path '{propertyPath}' should exist in JSON response");
        element.ValueKind.ShouldBe(JsonValueKind.Number, $"Property path '{propertyPath}' should be a number");
        element.GetDouble().ShouldBe(expectedValue, $"Number at path '{propertyPath}' should have value {expectedValue}");
    }

    /// <summary>
    /// Asserts that a JSON response contains a boolean at the specified path with expected value
    /// </summary>
    public static void AssertJsonBooleanAtPath(HttpResponseMessage response, string propertyPath, bool expectedValue)
    {
        response.ShouldNotBeNull();

        var content = response.Content.ReadAsStringAsync().Result;
        content.ShouldNotBeNullOrEmpty();

        var jsonDoc = JsonDocument.Parse(content);
        var element = GetJsonElementByPath(jsonDoc.RootElement, propertyPath);

        element.ValueKind.ShouldNotBe(JsonValueKind.Undefined, $"Property path '{propertyPath}' should exist in JSON response");
        element.ValueKind.ShouldBe(JsonValueKind.True, $"Property path '{propertyPath}' should be a boolean");
        element.GetBoolean().ShouldBe(expectedValue, $"Boolean at path '{propertyPath}' should have value {expectedValue}");
    }

    /// <summary>
    /// Gets a JSON element by navigating through a property path (e.g., "data.items[0].name")
    /// </summary>
    private static JsonElement GetJsonElementByPath(JsonElement root, string propertyPath)
    {
        var pathParts = propertyPath.Split('.');
        var current = root;

        foreach (var part in pathParts)
        {
            if (part.Contains('[') && part.Contains(']'))
            {
                // Handle array indexing (e.g., "items[0]")
                var bracketIndex = part.IndexOf('[');
                var propertyName = part.Substring(0, bracketIndex);
                var indexStr = part.Substring(bracketIndex + 1, part.IndexOf(']') - bracketIndex - 1);

                if (!int.TryParse(indexStr, out var index))
                {
                    throw new ArgumentException($"Invalid array index in property path: {part}");
                }

                if (!current.TryGetProperty(propertyName, out current))
                {
                    return default;
                }

                if (current.ValueKind != JsonValueKind.Array)
                {
                    throw new InvalidOperationException($"Property '{propertyName}' is not an array");
                }

                var array = current.EnumerateArray().ToList();
                if (index < 0 || index >= array.Count)
                {
                    return default;
                }

                current = array[index];
            }
            else
            {
                // Handle regular property access
                if (!current.TryGetProperty(part, out current))
                {
                    return default;
                }
            }
        }

        return current;
    }

    /// <summary>
    /// Asserts that a JSON response is valid and can be parsed
    /// </summary>
    public static void AssertJsonResponseIsValid(HttpResponseMessage response)
    {
        response.ShouldNotBeNull();
        response.IsSuccessStatusCode.ShouldBeTrue($"Expected success status, got {response.StatusCode}");

        var content = response.Content.ReadAsStringAsync().Result;
        content.ShouldNotBeNullOrEmpty();

        // Verify it's valid JSON
        Should.NotThrow(() => JsonDocument.Parse(content), "Response should be valid JSON");
    }

    /// <summary>
    /// Asserts that a JSON response has the expected content type
    /// </summary>
    public static void AssertJsonContentType(HttpResponseMessage response, string expectedContentType = "application/json")
    {
        response.ShouldNotBeNull();
        response.Content.Headers.ContentType?.MediaType.ShouldBe(expectedContentType,
            $"Response should have content type '{expectedContentType}'");
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
            content.ShouldNotBeNullOrEmpty();
            content.ShouldContain(expectedErrorMessage);
        }
    }

    /// <summary>
    /// Asserts that a JSON response contains the expected properties (basic validation)
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
}