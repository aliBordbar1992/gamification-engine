namespace GamificationEngine.Domain.Users;

/// <summary>
/// Represents the current state of a user in the gamification system
/// </summary>
public class UserState
{
    // EF Core requires a parameterless constructor
    protected UserState() { }

    public UserState(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId cannot be empty", nameof(userId));
        UserId = userId;
        PointsByCategory = new Dictionary<string, long>();
        Badges = new HashSet<string>();
        Trophies = new HashSet<string>();
    }

    public string UserId { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, long> PointsByCategory { get; set; } = new Dictionary<string, long>();
    public IReadOnlyCollection<string> Badges { get; set; } = new HashSet<string>();
    public IReadOnlyCollection<string> Trophies { get; set; } = new HashSet<string>();
}
