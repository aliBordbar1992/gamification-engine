namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Result of a stress test
/// </summary>
public class StressTestResult
{
    public StressTestResult(IReadOnlyList<LoadTestResult> loadTestResults, TimeSpan totalDuration, StressTestOptions options)
    {
        LoadTestResults = loadTestResults;
        TotalDuration = totalDuration;
        Options = options;
    }

    public IReadOnlyList<LoadTestResult> LoadTestResults { get; }
    public TimeSpan TotalDuration { get; }
    public StressTestOptions Options { get; }

    public int BreakingPoint => LoadTestResults.LastOrDefault()?.Options.Concurrency ?? 0;
    public int TotalRequests => LoadTestResults.Sum(r => r.TotalRequests);
    public double OverallSuccessRate => TotalRequests > 0 ? (double)LoadTestResults.Sum(r => r.SuccessfulRequests) / TotalRequests : 0;
}