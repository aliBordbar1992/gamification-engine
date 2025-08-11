namespace GamificationEngine.Domain.Users;

public sealed class UserState
{
    public string UserId { get; }

    private readonly Dictionary<string, long> _pointsByCategory = new();
    private readonly HashSet<string> _badgeIds = new();
    private readonly HashSet<string> _trophyIds = new();

    public IReadOnlyDictionary<string, long> PointsByCategory => _pointsByCategory;
    public IReadOnlyCollection<string> Badges => _badgeIds;
    public IReadOnlyCollection<string> Trophies => _trophyIds;

    public UserState(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId cannot be empty", nameof(userId));
        UserId = userId;
    }

    public void AddPoints(string category, long amount)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("category cannot be empty", nameof(category));
        if (!_pointsByCategory.TryGetValue(category, out var current)) current = 0;
        _pointsByCategory[category] = current + amount;
    }

    public bool GrantBadge(string badgeId)
    {
        if (string.IsNullOrWhiteSpace(badgeId)) throw new ArgumentException("badgeId cannot be empty", nameof(badgeId));
        return _badgeIds.Add(badgeId);
    }

    public bool GrantTrophy(string trophyId)
    {
        if (string.IsNullOrWhiteSpace(trophyId)) throw new ArgumentException("trophyId cannot be empty", nameof(trophyId));
        return _trophyIds.Add(trophyId);
    }
}
