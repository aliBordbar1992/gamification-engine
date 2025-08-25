using GamificationEngine.Integration.Tests.Infrastructure.Models;

namespace GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

/// <summary>
/// Interface for collecting test metrics
/// </summary>
public interface ITestMetricsCollector
{
    /// <summary>
    /// Records a test metric
    /// </summary>
    void RecordMetric(string name, double value, string? unit = null, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Records a test counter
    /// </summary>
    void IncrementCounter(string name, int increment = 1, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Records a test duration
    /// </summary>
    IDisposable RecordDuration(string name, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Gets all collected metrics
    /// </summary>
    IReadOnlyList<TestMetric> GetMetrics();

    /// <summary>
    /// Gets metrics for a specific name
    /// </summary>
    IEnumerable<TestMetric> GetMetricsByName(string name);

    /// <summary>
    /// Clears all collected metrics
    /// </summary>
    void ClearMetrics();
}