using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using GamificationEngine.Integration.Tests.Infrastructure.Testing;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Monitoring;

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