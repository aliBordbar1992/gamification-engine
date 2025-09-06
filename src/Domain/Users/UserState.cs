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
        _pointsByCategory = new Dictionary<string, long>(StringComparer.Ordinal);
        _badges = new HashSet<string>(StringComparer.Ordinal);
        _trophies = new HashSet<string>(StringComparer.Ordinal);
    }

    private readonly Dictionary<string, long> _pointsByCategory = new(StringComparer.Ordinal);
    private readonly HashSet<string> _badges = new(StringComparer.Ordinal);
    private readonly HashSet<string> _trophies = new(StringComparer.Ordinal);

    public string UserId { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, long> PointsByCategory => _pointsByCategory;
    public IReadOnlyCollection<string> Badges => _badges;
    public IReadOnlyCollection<string> Trophies => _trophies;

    /// <summary>
    /// Adds points to the specified category for the user.
    /// </summary>
    /// <param name="category">Point category identifier (e.g., "xp", "score").</param>
    /// <param name="amount">Points to add (can be negative for penalties).</param>
    public void AddPoints(string category, long amount)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("category cannot be empty", nameof(category));

        if (_pointsByCategory.TryGetValue(category, out var current))
        {
            _pointsByCategory[category] = current + amount;
        }
        else
        {
            _pointsByCategory[category] = amount;
        }
    }

    /// <summary>
    /// Grants a badge to the user if not already present.
    /// </summary>
    /// <param name="badgeId">Badge identifier.</param>
    public void GrantBadge(string badgeId)
    {
        if (string.IsNullOrWhiteSpace(badgeId)) throw new ArgumentException("badgeId cannot be empty", nameof(badgeId));
        _badges.Add(badgeId);
    }

    /// <summary>
    /// Removes a badge from the user if present.
    /// </summary>
    /// <param name="badgeId">Badge identifier.</param>
    public void RemoveBadge(string badgeId)
    {
        if (string.IsNullOrWhiteSpace(badgeId)) throw new ArgumentException("badgeId cannot be empty", nameof(badgeId));
        _badges.Remove(badgeId);
    }

    /// <summary>
    /// Grants a trophy to the user if not already present.
    /// </summary>
    /// <param name="trophyId">Trophy identifier.</param>
    public void GrantTrophy(string trophyId)
    {
        if (string.IsNullOrWhiteSpace(trophyId)) throw new ArgumentException("trophyId cannot be empty", nameof(trophyId));
        _trophies.Add(trophyId);
    }

    /// <summary>
    /// Removes a trophy from the user if present.
    /// </summary>
    /// <param name="trophyId">Trophy identifier.</param>
    public void RemoveTrophy(string trophyId)
    {
        if (string.IsNullOrWhiteSpace(trophyId)) throw new ArgumentException("trophyId cannot be empty", nameof(trophyId));
        _trophies.Remove(trophyId);
    }
}
