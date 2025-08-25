namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Performance baseline established from baseline tests
/// </summary>
public class PerformanceBaseline
{
    public PerformanceBaseline(IReadOnlyList<TimeSpan> executionTimes, int successCount, int totalIterations)
    {
        ExecutionTimes = executionTimes;
        SuccessCount = successCount;
        TotalIterations = totalIterations;
    }

    public IReadOnlyList<TimeSpan> ExecutionTimes { get; }
    public int SuccessCount { get; }
    public int TotalIterations { get; }

    public double SuccessRate => TotalIterations > 0 ? (double)SuccessCount / TotalIterations : 0;
    public TimeSpan AverageExecutionTime => ExecutionTimes.Count > 0
        ? TimeSpan.FromMilliseconds(ExecutionTimes.Average(t => t.TotalMilliseconds))
        : TimeSpan.Zero;

    public TimeSpan MinExecutionTime => ExecutionTimes.Count > 0 ? ExecutionTimes.Min() : TimeSpan.Zero;
    public TimeSpan MaxExecutionTime => ExecutionTimes.Count > 0 ? ExecutionTimes.Max() : TimeSpan.Zero;

    public TimeSpan Percentile95 => CalculatePercentile(0.95);
    public TimeSpan Percentile99 => CalculatePercentile(0.99);

    private TimeSpan CalculatePercentile(double percentile)
    {
        if (ExecutionTimes.Count == 0) return TimeSpan.Zero;

        var sortedTimes = ExecutionTimes.Select(t => t.TotalMilliseconds).OrderBy(t => t).ToList();
        var index = (int)Math.Ceiling(percentile * sortedTimes.Count) - 1;
        index = Math.Max(0, Math.Min(index, sortedTimes.Count - 1));

        return TimeSpan.FromMilliseconds(sortedTimes[index]);
    }
}