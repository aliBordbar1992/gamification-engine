namespace GamificationEngine.Domain.Entities;

/// <summary>
/// Represents a badge that can be awarded to users
/// </summary>
public sealed class Badge
{
    // EF Core requires a parameterless constructor
    private Badge() { }

    public Badge(string id, string name, string description, string image, bool visible = true)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("ID cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty", nameof(description));
        if (string.IsNullOrWhiteSpace(image)) throw new ArgumentException("Image cannot be empty", nameof(image));

        Id = id;
        Name = name;
        Description = description;
        Image = image;
        Visible = visible;
    }

    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Image { get; private set; } = string.Empty;
    public bool Visible { get; private set; }

    /// <summary>
    /// Updates the badge information
    /// </summary>
    public void UpdateInfo(string name, string description, string image, bool visible)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty", nameof(description));
        if (string.IsNullOrWhiteSpace(image)) throw new ArgumentException("Image cannot be empty", nameof(image));

        Name = name;
        Description = description;
        Image = image;
        Visible = visible;
    }

    /// <summary>
    /// Sets the visibility of the badge
    /// </summary>
    public void SetVisibility(bool visible)
    {
        Visible = visible;
    }

    /// <summary>
    /// Validates the badge configuration
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(Description) &&
               !string.IsNullOrWhiteSpace(Image);
    }
}
