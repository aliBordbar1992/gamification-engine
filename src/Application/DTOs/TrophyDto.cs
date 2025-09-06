namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data transfer object for trophy information
/// </summary>
public sealed class TrophyDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public bool Visible { get; set; }
}

/// <summary>
/// Data transfer object for creating a trophy
/// </summary>
public sealed class CreateTrophyDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public bool Visible { get; set; } = true;
}

/// <summary>
/// Data transfer object for updating a trophy
/// </summary>
public sealed class UpdateTrophyDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public bool Visible { get; set; }
}
