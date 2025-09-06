namespace GamificationEngine.Domain.Entities;

/// <summary>
/// Represents a point category that defines how points are aggregated and used
/// </summary>
public sealed class PointCategory
{
    // EF Core requires a parameterless constructor
    private PointCategory() { }

    public PointCategory(string id, string name, string description, string aggregation)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("ID cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty", nameof(description));
        if (string.IsNullOrWhiteSpace(aggregation)) throw new ArgumentException("Aggregation cannot be empty", nameof(aggregation));

        Id = id;
        Name = name;
        Description = description;
        Aggregation = aggregation;
    }

    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Aggregation { get; private set; } = string.Empty;

    /// <summary>
    /// Updates the point category information
    /// </summary>
    public void UpdateInfo(string name, string description, string aggregation)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty", nameof(description));
        if (string.IsNullOrWhiteSpace(aggregation)) throw new ArgumentException("Aggregation cannot be empty", nameof(aggregation));

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
               !string.IsNullOrWhiteSpace(Aggregation) &&
               IsValidAggregation(Aggregation);
    }

    private static bool IsValidAggregation(string aggregation)
    {
        return aggregation.ToLowerInvariant() switch
        {
            "sum" => true,
            "max" => true,
            "min" => true,
            "avg" => true,
            "count" => true,
            _ => false
        };
    }
}
