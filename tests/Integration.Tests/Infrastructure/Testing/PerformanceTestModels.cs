namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Options for configuring load tests
/// </summary>
public class LoadTestOptions
{
    public int Concurrency { get; set; } = 10;
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableMetricsCollection { get; set; } = true;
}

/// <summary>
/// Options for configuring stress tests
/// </summary>
public class StressTestOptions
{
    public int InitialConcurrency { get; set; } = 1;
    public int MaxConcurrency { get; set; } = 100;
    public int ConcurrencyStep { get; set; } = 5;
    public TimeSpan StepDuration { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan StepDelay { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public double SuccessRateThreshold { get; set; } = 0.95; // 95%
}

/// <summary>
/// Options for configuring baseline tests
/// </summary>
public class BaselineTestOptions
{
    public int Iterations { get; set; } = 100;
    public TimeSpan DelayBetweenIterations { get; set; } = TimeSpan.FromMilliseconds(100);
    public bool EnableProgressLogging { get; set; } = true;
    public bool EnableMetricsCollection { get; set; } = true;
}

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

/// <summary>
/// Result of a performance comparison
/// </summary>
public class PerformanceComparison
{
    public PerformanceBaseline Baseline { get; set; } = null!;
    public LoadTestResult CurrentResult { get; set; } = null!;
    public TimeSpan ResponseTimeChange { get; set; }
    public double ResponseTimeChangePercentage { get; set; }
    public double SuccessRateChange { get; set; }
    public bool IsWithinTolerance { get; set; }
}

/// <summary>
/// Result of a single request in a load test
/// </summary>
public class RequestResult
{
    public int RequestId { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public Exception? Exception { get; set; }
}