using GamificationEngine.Shared;

namespace GamificationEngine.Domain.Entities;

/// <summary>
/// Represents a point category that defines how points are aggregated and used
/// </summary>
public sealed class PointCategory
{
    // EF Core requires a parameterless constructor
    private PointCategory() { }

    public PointCategory(string id, string name, string description, PointCategoryAggregation aggregation)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("ID cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty", nameof(description));
        if (!IsValidAggregation(aggregation)) throw new ArgumentException("Aggregation is invalid", nameof(aggregation));

        Id = id;
        Name = name;
        Description = description;
        Aggregation = aggregation;
    }

    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public PointCategoryAggregation Aggregation { get; private set; } = PointCategoryAggregation.Sum;

    /// <summary>
    /// Updates the point category information
    /// </summary>
    public void UpdateInfo(string name, string description, PointCategoryAggregation aggregation)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty", nameof(description));
        if (!IsValidAggregation(aggregation)) throw new ArgumentException("Aggregation is invalid", nameof(aggregation));

        Name = name;
        Description = description;
        Aggregation = aggregation;
    }

    /// <summary>
    /// Validates the point category configuration
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(Description) &&
               IsValidAggregation(Aggregation);
    }

    private static bool IsValidAggregation(PointCategoryAggregation aggregation)
    {
        return aggregation switch
        {
            PointCategoryAggregation.Sum => true,
            PointCategoryAggregation.Max => true,
            PointCategoryAggregation.Min => true,
            PointCategoryAggregation.Avg => true,
            PointCategoryAggregation.Count => true,
            _ => false
        };
    }
}
