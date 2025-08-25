namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Statistics for a single test
/// </summary>
public class TestExecutionStats
{
    private readonly List<TimeSpan> _executionTimes = new();
    private readonly List<bool> _results = new();
    private readonly List<Exception?> _exceptions = new();

    public TestExecutionStats(string testName, string? category)
    {
        TestName = testName;
        Category = category;
    }

    public string TestName { get; }
    public string? Category { get; }

    public int TotalExecutions => _executionTimes.Count;
    public int SuccessfulExecutions => _results.Count(r => r);
    public int FailedExecutions => _results.Count(r => !r);
    public double SuccessRate => TotalExecutions > 0 ? (double)SuccessfulExecutions / TotalExecutions : 0;

    public TimeSpan AverageExecutionTime => _executionTimes.Count > 0
        ? TimeSpan.FromMilliseconds(_executionTimes.Average(t => t.TotalMilliseconds))
        : TimeSpan.Zero;

    public TimeSpan MinExecutionTime => _executionTimes.Count > 0 ? _executionTimes.Min() : TimeSpan.Zero;
    public TimeSpan MaxExecutionTime => _executionTimes.Count > 0 ? _executionTimes.Max() : TimeSpan.Zero;

    public IReadOnlyList<TimeSpan> ExecutionTimes => _executionTimes.AsReadOnly();
    public IReadOnlyList<Exception?> Exceptions => _exceptions.AsReadOnly();

    internal void RecordExecution(TimeSpan duration, bool success, Exception? exception)
    {
        _executionTimes.Add(duration);
        _results.Add(success);
        _exceptions.Add(exception);
    }
}