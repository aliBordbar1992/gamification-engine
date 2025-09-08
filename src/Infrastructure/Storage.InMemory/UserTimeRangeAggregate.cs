namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// Represents aggregated user data for a specific time range
/// </summary>
public class UserTimeRangeAggregate
{
    private readonly Dictionary<string, long> _pointsByCategory = new(StringComparer.Ordinal);
    private readonly HashSet<string> _badges = new(StringComparer.Ordinal);
    private readonly HashSet<string> _trophies = new(StringComparer.Ordinal);

    public UserTimeRangeAggregate(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
    }

    public string UserId { get; }

    public IReadOnlyDictionary<string, long> PointsByCategory => _pointsByCategory;
    public IReadOnlyCollection<string> Badges => _badges;
    public IReadOnlyCollection<string> Trophies => _trophies;

    /// <summary>
    /// Adds points to the specified category
    /// </summary>
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
    /// Adds a badge to the collection
    /// </summary>
    public void AddBadge(string badgeId)
    {
        if (string.IsNullOrWhiteSpace(badgeId)) throw new ArgumentException("badgeId cannot be empty", nameof(badgeId));
        _badges.Add(badgeId);
    }

    /// <summary>
    /// Adds a trophy to the collection
    /// </summary>
    public void AddTrophy(string trophyId)
    {
        if (string.IsNullOrWhiteSpace(trophyId)) throw new ArgumentException("trophyId cannot be empty", nameof(trophyId));
        _trophies.Add(trophyId);
    }
}
