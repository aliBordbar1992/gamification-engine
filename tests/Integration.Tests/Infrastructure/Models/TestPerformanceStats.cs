namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Represents performance statistics for a test operation
/// </summary>
public class TestPerformanceStats
{
    private readonly List<TimeSpan> _durations = new();
    private readonly List<bool> _results = new();

    public int TotalOperations => _durations.Count;
    public int SuccessfulOperations => _results.Count(r => r);
    public int FailedOperations => _results.Count(r => !r);
    public double SuccessRate => TotalOperations > 0 ? (double)SuccessfulOperations / TotalOperations : 0;

    public TimeSpan AverageDuration => _durations.Count > 0
        ? TimeSpan.FromMilliseconds(_durations.Average(d => d.TotalMilliseconds))
        : TimeSpan.Zero;

    public TimeSpan MinDuration => _durations.Count > 0 ? _durations.Min() : TimeSpan.Zero;
    public TimeSpan MaxDuration => _durations.Count > 0 ? _durations.Max() : TimeSpan.Zero;

    internal void RecordOperation(TimeSpan duration, bool success)
    {
        _durations.Add(duration);
        _results.Add(success);
    }
}