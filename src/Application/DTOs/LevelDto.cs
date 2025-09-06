namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data transfer object for level information
/// </summary>
public sealed class LevelDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long MinPoints { get; set; }
}

/// <summary>
/// Data transfer object for creating a level
/// </summary>
public sealed class CreateLevelDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long MinPoints { get; set; }
}

/// <summary>
/// Data transfer object for updating a level
/// </summary>
public sealed class UpdateLevelDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long MinPoints { get; set; }
}
