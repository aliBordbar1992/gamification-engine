namespace GamificationEngine.Domain.Entities;

/// <summary>
/// Represents a level that users can achieve based on point thresholds
/// </summary>
public sealed class Level
{
    // EF Core requires a parameterless constructor
    private Level() { }

    public Level(string id, string name, string category, long minPoints)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("ID cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category cannot be empty", nameof(category));
        if (minPoints < 0) throw new ArgumentException("Minimum points cannot be negative", nameof(minPoints));

        Id = id;
        Name = name;
        Category = category;
        MinPoints = minPoints;
    }

    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public long MinPoints { get; private set; }

    /// <summary>
    /// Updates the level information
    /// </summary>
    public void UpdateInfo(string name, string category, long minPoints)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category cannot be empty", nameof(category));
        if (minPoints < 0) throw new ArgumentException("Minimum points cannot be negative", nameof(minPoints));

        Name = name;
        Category = category;
        MinPoints = minPoints;
    }

    /// <summary>
    /// Updates the minimum points threshold for this level
    /// </summary>
    public void UpdateMinPoints(long minPoints)
    {
        if (minPoints < 0) throw new ArgumentException("Minimum points cannot be negative", nameof(minPoints));
        MinPoints = minPoints;
    }

    /// <summary>
    /// Checks if a given point amount qualifies for this level
    /// </summary>
    public bool QualifiesForLevel(long points)
    {
        return points >= MinPoints;
    }

    /// <summary>
    /// Validates the level configuration
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(Category) &&
               MinPoints >= 0;
    }
}
