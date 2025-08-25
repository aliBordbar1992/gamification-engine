using System.Diagnostics;
using Microsoft.Extensions.Logging;
using GamificationEngine.Integration.Tests.Infrastructure.Configuration;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Monitors test execution time and provides performance tracking for individual tests
/// </summary>
public class TestExecutionMonitor : ITestExecutionMonitor
{
    private readonly ITestMetricsCollector _metricsCollector;
    private readonly ITestPerformanceMonitor _performanceMonitor;
    private readonly ILogger<TestExecutionMonitor> _logger;
    private readonly Dictionary<string, TestExecutionStats> _testStats;

    public TestExecutionMonitor(
        ITestMetricsCollector metricsCollector,
        ITestPerformanceMonitor performanceMonitor,
        ILogger<TestExecutionMonitor> logger)
    {
        _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
        _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _testStats = new Dictionary<string, TestExecutionStats>();
    }

    /// <summary>
    /// Starts monitoring a test execution
    /// </summary>
    public ITestExecutionTracker StartTestExecution(string testName, string? testCategory = null)
    {
        var tracker = new TestExecutionTracker(this, testName, testCategory);
        _logger.LogDebug("Started monitoring test execution: {TestName}", testName);
        return tracker;
    }

    /// <summary>
    /// Records test execution completion
    /// </summary>
    internal void RecordTestExecution(string testName, string? testCategory, TimeSpan duration, bool success, Exception? exception = null)
    {
        lock (_testStats)
        {
            if (!_testStats.ContainsKey(testName))
            {
                _testStats[testName] = new TestExecutionStats(testName, testCategory);
            }

            var stats = _testStats[testName];
            stats.RecordExecution(duration, success, exception);
        }

        // Record metrics
        _metricsCollector.RecordMetric("test_execution_duration", duration.TotalMilliseconds, "ms",
            new Dictionary<string, object>
            {
                ["test_name"] = testName,
                ["test_category"] = testCategory ?? "unknown",
                ["success"] = success
            });

        if (success)
        {
            _metricsCollector.IncrementCounter("successful_tests", 1,
                new Dictionary<string, object> { ["test_category"] = testCategory ?? "unknown" });
        }
        else
        {
            _metricsCollector.IncrementCounter("failed_tests", 1,
                new Dictionary<string, object> { ["test_category"] = testCategory ?? "unknown" });
        }

        _logger.LogDebug("Test execution recorded: {TestName} took {Duration}ms, Success: {Success}",
            testName, duration.TotalMilliseconds, success);
    }

    /// <summary>
    /// Gets execution statistics for a specific test
    /// </summary>
    public TestExecutionStats GetTestStats(string testName)
    {
        lock (_testStats)
        {
            return _testStats.TryGetValue(testName, out var stats) ? stats : new TestExecutionStats(testName, null);
        }
    }

    /// <summary>
    /// Gets execution statistics for all tests
    /// </summary>
    public IReadOnlyDictionary<string, TestExecutionStats> GetAllTestStats()
    {
        lock (_testStats)
        {
            return _testStats.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    /// <summary>
    /// Gets execution statistics grouped by test category
    /// </summary>
    public IReadOnlyDictionary<string, TestCategoryStats> GetTestCategoryStats()
    {
        lock (_testStats)
        {
            var categoryStats = new Dictionary<string, TestCategoryStats>();

            foreach (var testStat in _testStats.Values)
            {
                var category = testStat.Category ?? "unknown";
                if (!categoryStats.ContainsKey(category))
                {
                    categoryStats[category] = new TestCategoryStats(category);
                }

                categoryStats[category].AddTestStats(testStat);
            }

            return categoryStats;
        }
    }

    /// <summary>
    /// Gets performance summary for all tests
    /// </summary>
    public TestPerformanceSummary GetPerformanceSummary()
    {
        lock (_testStats)
        {
            if (!_testStats.Any())
                return new TestPerformanceSummary();

            var allDurations = _testStats.Values.SelectMany(s => s.ExecutionTimes).ToList();
            var totalTests = _testStats.Values.Sum(s => s.TotalExecutions);
            var successfulTests = _testStats.Values.Sum(s => s.SuccessfulExecutions);
            var failedTests = _testStats.Values.Sum(s => s.FailedExecutions);

            return new TestPerformanceSummary
            {
                TotalTests = totalTests,
                SuccessfulTests = successfulTests,
                FailedTests = failedTests,
                SuccessRate = totalTests > 0 ? (double)successfulTests / totalTests : 0,
                AverageExecutionTime = allDurations.Count > 0
                    ? TimeSpan.FromMilliseconds(allDurations.Average(d => d.TotalMilliseconds))
                    : TimeSpan.Zero,
                MinExecutionTime = allDurations.Count > 0 ? allDurations.Min() : TimeSpan.Zero,
                MaxExecutionTime = allDurations.Count > 0 ? allDurations.Max() : TimeSpan.Zero,
                TotalExecutionTime = allDurations.Count > 0
                    ? TimeSpan.FromMilliseconds(allDurations.Sum(d => d.TotalMilliseconds))
                    : TimeSpan.Zero
            };
        }
    }

    /// <summary>
    /// Clears all test execution statistics
    /// </summary>
    public void ClearStats()
    {
        lock (_testStats)
        {
            _testStats.Clear();
        }
        _logger.LogInformation("Test execution statistics cleared");
    }

    /// <summary>
    /// Exports test execution statistics to a report
    /// </summary>
    public TestExecutionReport GenerateReport()
    {
        var summary = GetPerformanceSummary();
        var categoryStats = GetTestCategoryStats();
        var allTestStats = GetAllTestStats();

        var report = new TestExecutionReport
        {
            GeneratedAt = DateTime.UtcNow,
            Summary = summary,
            CategoryStats = categoryStats,
            TestStats = allTestStats,
            Recommendations = GenerateRecommendations(summary, categoryStats)
        };

        _logger.LogInformation("Test execution report generated with {TestCount} tests across {CategoryCount} categories",
            summary.TotalTests, categoryStats.Count);

        return report;
    }

    private List<string> GenerateRecommendations(TestPerformanceSummary summary, IReadOnlyDictionary<string, TestCategoryStats> categoryStats)
    {
        var recommendations = new List<string>();

        if (summary.SuccessRate < 0.95)
        {
            recommendations.Add("Test success rate is below 95%. Review failing tests and improve test stability.");
        }

        if (summary.AverageExecutionTime > TimeSpan.FromSeconds(30))
        {
            recommendations.Add("Average test execution time is high. Consider optimizing slow tests or using parallel execution.");
        }

        var slowCategories = categoryStats
            .Where(kvp => kvp.Value.AverageExecutionTime > TimeSpan.FromSeconds(10))
            .Select(kvp => kvp.Key);

        if (slowCategories.Any())
        {
            recommendations.Add($"Slow test categories detected: {string.Join(", ", slowCategories)}. Consider optimization.");
        }

        var failingCategories = categoryStats
            .Where(kvp => kvp.Value.SuccessRate < 0.9)
            .Select(kvp => kvp.Key);

        if (failingCategories.Any())
        {
            recommendations.Add($"Categories with low success rates: {string.Join(", ", failingCategories)}. Investigate failures.");
        }

        return recommendations;
    }
}

/// <summary>
/// Interface for monitoring test execution
/// </summary>
public interface ITestExecutionMonitor
{
    /// <summary>
    /// Starts monitoring a test execution
    /// </summary>
    ITestExecutionTracker StartTestExecution(string testName, string? testCategory = null);

    /// <summary>
    /// Gets execution statistics for a specific test
    /// </summary>
    TestExecutionStats GetTestStats(string testName);

    /// <summary>
    /// Gets execution statistics for all tests
    /// </summary>
    IReadOnlyDictionary<string, TestExecutionStats> GetAllTestStats();

    /// <summary>
    /// Gets execution statistics grouped by test category
    /// </summary>
    IReadOnlyDictionary<string, TestCategoryStats> GetTestCategoryStats();

    /// <summary>
    /// Gets performance summary for all tests
    /// </summary>
    TestPerformanceSummary GetPerformanceSummary();

    /// <summary>
    /// Clears all test execution statistics
    /// </summary>
    void ClearStats();

    /// <summary>
    /// Exports test execution statistics to a report
    /// </summary>
    TestExecutionReport GenerateReport();
}

/// <summary>
/// Interface for tracking individual test execution
/// </summary>
public interface ITestExecutionTracker : IDisposable
{
    /// <summary>
    /// Marks the test as successful
    /// </summary>
    void MarkSuccess();

    /// <summary>
    /// Marks the test as failed
    /// </summary>
    void MarkFailure(Exception? exception = null);
}

/// <summary>
/// Implementation of test execution tracker
/// </summary>
public class TestExecutionTracker : ITestExecutionTracker
{
    private readonly TestExecutionMonitor _monitor;
    private readonly string _testName;
    private readonly string? _testCategory;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    public TestExecutionTracker(TestExecutionMonitor monitor, string testName, string? testCategory)
    {
        _monitor = monitor;
        _testName = testName;
        _testCategory = testCategory;
        _stopwatch = Stopwatch.StartNew();
    }

    public void MarkSuccess()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordTestExecution(_testName, _testCategory, _stopwatch.Elapsed, true);
        }
    }

    public void MarkFailure(Exception? exception = null)
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordTestExecution(_testName, _testCategory, _stopwatch.Elapsed, false, exception);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordTestExecution(_testName, _testCategory, _stopwatch.Elapsed, true);
            _disposed = true;
        }
    }
}

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

/// <summary>
/// Performance summary for all tests
/// </summary>
public class TestPerformanceSummary
{
    public int TotalTests { get; set; }
    public int SuccessfulTests { get; set; }
    public int FailedTests { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan AverageExecutionTime { get; set; }
    public TimeSpan MinExecutionTime { get; set; }
    public TimeSpan MaxExecutionTime { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
}

/// <summary>
/// Comprehensive test execution report
/// </summary>
public class TestExecutionReport
{
    public DateTime GeneratedAt { get; set; }
    public TestPerformanceSummary Summary { get; set; } = null!;
    public IReadOnlyDictionary<string, TestCategoryStats> CategoryStats { get; set; } = null!;
    public IReadOnlyDictionary<string, TestExecutionStats> TestStats { get; set; } = null!;
    public List<string> Recommendations { get; set; } = new();
}