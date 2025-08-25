namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Result of a load test
/// </summary>
public class LoadTestResult
{
    public LoadTestResult(IReadOnlyList<RequestResult> results, TimeSpan totalDuration, LoadTestOptions options)
    {
        Results = results;
        TotalDuration = totalDuration;
        Options = options;
    }

    public IReadOnlyList<RequestResult> Results { get; }
    public TimeSpan TotalDuration { get; }
    public LoadTestOptions Options { get; }

    public int TotalRequests => Results.Count;
    public int SuccessfulRequests => Results.Count(r => r.Success);
    public int FailedRequests => Results.Count(r => !r.Success);
    public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests : 0;

    public TimeSpan AverageResponseTime => Results.Count > 0
        ? TimeSpan.FromMilliseconds(Results.Average(r => r.Duration.TotalMilliseconds))
        : TimeSpan.Zero;

    public TimeSpan MinResponseTime => Results.Count > 0 ? Results.Min(r => r.Duration) : TimeSpan.Zero;
    public TimeSpan MaxResponseTime => Results.Count > 0 ? Results.Max(r => r.Duration) : TimeSpan.Zero;

    public TimeSpan Percentile95 => CalculatePercentile(0.95);
    public TimeSpan Percentile99 => CalculatePercentile(0.99);

    private TimeSpan CalculatePercentile(double percentile)
    {
        if (Results.Count == 0) return TimeSpan.Zero;

        var sortedDurations = Results.Select(r => r.Duration.TotalMilliseconds).OrderBy(d => d).ToList();
        var index = (int)Math.Ceiling(percentile * sortedDurations.Count) - 1;
        index = Math.Max(0, Math.Min(index, sortedDurations.Count - 1));

        return TimeSpan.FromMilliseconds(sortedDurations[index]);
    }
}