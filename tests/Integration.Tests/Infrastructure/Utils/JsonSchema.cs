using System.Text.Json;

namespace GamificationEngine.Integration.Tests.Infrastructure.Utils;

/// <summary>
/// Represents a JSON schema for validation
/// </summary>
public class JsonSchema
{
    public JsonValueKind? ExpectedType { get; set; }
    public List<string>? RequiredProperties { get; set; }
    public Dictionary<string, JsonSchema>? PropertySchemas { get; set; }
    public JsonSchema? ArrayItemSchema { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public int? MinArrayLength { get; set; }
    public int? MaxArrayLength { get; set; }
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Creates a schema for an object with specific properties
    /// </summary>
    public static JsonSchema Object(Dictionary<string, JsonSchema> properties, List<string>? requiredProperties = null)
    {
        return new JsonSchema
        {
            ExpectedType = JsonValueKind.Object,
            PropertySchemas = properties,
            RequiredProperties = requiredProperties
        };
    }

    /// <summary>
    /// Creates a schema for an array with specific item schema
    /// </summary>
    public static JsonSchema Array(JsonSchema itemSchema, int? minLength = null, int? maxLength = null)
    {
        return new JsonSchema
        {
            ExpectedType = JsonValueKind.Array,
            ArrayItemSchema = itemSchema,
            MinArrayLength = minLength,
            MaxArrayLength = maxLength
        };
    }

    /// <summary>
    /// Creates a schema for a string with length constraints
    /// </summary>
    public static JsonSchema String(int? minLength = null, int? maxLength = null)
    {
        return new JsonSchema
        {
            ExpectedType = JsonValueKind.String,
            MinLength = minLength,
            MaxLength = maxLength
        };
    }

    /// <summary>
    /// Creates a schema for a number with value constraints
    /// </summary>
    public static JsonSchema Number(double? minValue = null, double? maxValue = null)
    {
        return new JsonSchema
        {
            ExpectedType = JsonValueKind.Number,
            MinValue = minValue,
            MaxValue = maxValue
        };
    }

    /// <summary>
    /// Creates a schema for a boolean
    /// </summary>
    public static JsonSchema Boolean()
    {
        return new JsonSchema
        {
            ExpectedType = JsonValueKind.True
        };
    }

    /// <summary>
    /// Creates an optional property schema
    /// </summary>
    public JsonSchema Optional()
    {
        IsRequired = false;
        return this;
    }
}