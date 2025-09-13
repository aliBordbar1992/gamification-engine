namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data transfer object for event definition information
/// </summary>
public sealed class EventDefinitionDto
{
    /// <summary>
    /// The event definition ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Description of the event
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Payload schema for the event
    /// </summary>
    public IReadOnlyDictionary<string, string> PayloadSchema { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// Data transfer object for creating an event definition
/// </summary>
public sealed class CreateEventDefinitionDto
{
    /// <summary>
    /// The event definition ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Description of the event
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Payload schema for the event
    /// </summary>
    public Dictionary<string, string>? PayloadSchema { get; set; }
}

/// <summary>
/// Data transfer object for updating an event definition
/// </summary>
public sealed class UpdateEventDefinitionDto
{
    /// <summary>
    /// Description of the event
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Payload schema for the event
    /// </summary>
    public Dictionary<string, string>? PayloadSchema { get; set; }
}
