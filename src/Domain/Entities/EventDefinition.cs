using System.Collections.ObjectModel;

namespace GamificationEngine.Domain.Entities;

/// <summary>
/// Represents an event definition from the configuration catalog
/// </summary>
public class EventDefinition
{
    // EF Core requires a parameterless constructor
    protected EventDefinition() { }

    public EventDefinition(string id, string description, IReadOnlyDictionary<string, string>? payloadSchema = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty", nameof(description));

        Id = id;
        Description = description;
        PayloadSchema = payloadSchema?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>();
    }

    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> PayloadSchema { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Validates if the provided attributes match the expected payload schema
    /// </summary>
    /// <param name="attributes">The event attributes to validate</param>
    /// <returns>True if the attributes match the schema, false otherwise</returns>
    public bool ValidatePayload(IDictionary<string, object>? attributes)
    {
        if (PayloadSchema == null || !PayloadSchema.Any())
            return true; // No schema defined, accept any payload

        if (attributes == null)
            return false; // Schema defined but no attributes provided

        // Check if all required schema fields are present
        foreach (var schemaField in PayloadSchema)
        {
            if (!attributes.ContainsKey(schemaField.Key))
                return false; // Required field missing

            // Basic type validation could be added here
            // For now, we just check presence
        }

        return true;
    }
}
