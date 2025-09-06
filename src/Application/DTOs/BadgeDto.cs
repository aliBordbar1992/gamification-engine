namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data transfer object for badge information
/// </summary>
public sealed class BadgeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public bool Visible { get; set; }
}

/// <summary>
/// Data transfer object for creating a badge
/// </summary>
public sealed class CreateBadgeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public bool Visible { get; set; } = true;
}

/// <summary>
/// Data transfer object for updating a badge
/// </summary>
public sealed class UpdateBadgeDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public bool Visible { get; set; }
}
