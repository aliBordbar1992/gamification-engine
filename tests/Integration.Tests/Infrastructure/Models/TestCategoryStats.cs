namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Statistics for a test category
/// </summary>
public class TestCategoryStats
{
    private readonly List<TestExecutionStats> _testStats = new();

    public TestCategoryStats(string categoryName)
    {
        CategoryName = categoryName;
    }

    public string CategoryName { get; }

    public int TotalTests => _testStats.Count;
    public int TotalExecutions => _testStats.Sum(s => s.TotalExecutions);
    public int SuccessfulExecutions => _testStats.Sum(s => s.SuccessfulExecutions);
    public int FailedExecutions => _testStats.Sum(s => s.FailedExecutions);

    public double SuccessRate => TotalExecutions > 0 ? (double)SuccessfulExecutions / TotalExecutions : 0;

    public TimeSpan AverageExecutionTime => _testStats.Any()
        ? TimeSpan.FromMilliseconds(_testStats.Average(s => s.AverageExecutionTime.TotalMilliseconds))
        : TimeSpan.Zero;

    public TimeSpan MinExecutionTime => _testStats.Any()
        ? _testStats.Min(s => s.MinExecutionTime)
        : TimeSpan.Zero;

    public TimeSpan MaxExecutionTime => _testStats.Any()
        ? _testStats.Max(s => s.MaxExecutionTime)
        : TimeSpan.Zero;

    internal void AddTestStats(TestExecutionStats testStats)
    {
        _testStats.Add(testStats);
    }
}