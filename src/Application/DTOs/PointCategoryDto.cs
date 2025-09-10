using GamificationEngine.Shared;

namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data transfer object for point category information
/// </summary>
public sealed class PointCategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Aggregation { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for creating a point category
/// </summary>
public sealed class CreatePointCategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Aggregation { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for updating a point category
/// </summary>
public sealed class UpdatePointCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Aggregation { get; set; } = string.Empty;
}
