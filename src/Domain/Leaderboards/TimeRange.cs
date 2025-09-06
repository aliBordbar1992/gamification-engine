namespace GamificationEngine.Domain.Leaderboards;

/// <summary>
/// Represents a time range for leaderboard queries
/// </summary>
public static class TimeRange
{
    public const string Daily = "daily";
    public const string Weekly = "weekly";
    public const string Monthly = "monthly";
    public const string AllTime = "alltime";

    /// <summary>
    /// Gets all valid time ranges
    /// </summary>
    public static readonly string[] AllRanges = { Daily, Weekly, Monthly, AllTime };

    /// <summary>
    /// Validates if a time range is valid
    /// </summary>
    public static bool IsValid(string timeRange)
    {
        return !string.IsNullOrWhiteSpace(timeRange) && AllRanges.Contains(timeRange);
    }

    /// <summary>
    /// Gets the start date for a given time range
    /// </summary>
    public static DateTime GetStartDate(string timeRange, DateTime? referenceDate = null)
    {
        var refDate = referenceDate ?? DateTime.UtcNow;

        return timeRange switch
        {
            Daily => refDate.Date,
            Weekly => refDate.Date.AddDays(-(int)refDate.DayOfWeek),
            Monthly => new DateTime(refDate.Year, refDate.Month, 1),
            AllTime => DateTime.MinValue,
            _ => throw new ArgumentException($"Invalid time range: {timeRange}", nameof(timeRange))
        };
    }

    /// <summary>
    /// Gets the end date for a given time range
    /// </summary>
    public static DateTime GetEndDate(string timeRange, DateTime? referenceDate = null)
    {
        var refDate = referenceDate ?? DateTime.UtcNow;

        return timeRange switch
        {
            Daily => refDate.Date.AddDays(1).AddTicks(-1),
            Weekly => refDate.Date.AddDays(-(int)refDate.DayOfWeek).AddDays(7).AddTicks(-1),
            Monthly => new DateTime(refDate.Year, refDate.Month, 1).AddMonths(1).AddTicks(-1),
            AllTime => DateTime.MaxValue,
            _ => throw new ArgumentException($"Invalid time range: {timeRange}", nameof(timeRange))
        };
    }
}
