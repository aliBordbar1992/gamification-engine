using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Implementation of test performance monitor
/// </summary>
public class TestPerformanceMonitor : ITestPerformanceMonitor
{
    private readonly Dictionary<string, TestPerformanceStats> _stats;
    private readonly TestMonitoringOptions _options;
    private readonly ILogger<TestPerformanceMonitor> _logger;

    public TestPerformanceMonitor(TestMonitoringOptions options, ILogger<TestPerformanceMonitor> logger)
    {
        _options = options;
        _logger = logger;
        _stats = new Dictionary<string, TestPerformanceStats>();
    }

    public ITestOperationMonitor StartOperation(string operationName)
    {
        return new TestOperationMonitor(this, operationName);
    }

    public TestPerformanceStats GetPerformanceStats(string operationName)
    {
        lock (_stats)
        {
            return _stats.TryGetValue(operationName, out var stats) ? stats : new TestPerformanceStats();
        }
    }

    public IReadOnlyDictionary<string, TestPerformanceStats> GetAllPerformanceStats()
    {
        lock (_stats)
        {
            return _stats.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    public void ClearPerformanceData()
    {
        lock (_stats)
        {
            _stats.Clear();
        }
    }

    internal void RecordOperation(string operationName, TimeSpan duration, bool success)
    {
        lock (_stats)
        {
            if (!_stats.ContainsKey(operationName))
            {
                _stats[operationName] = new TestPerformanceStats();
            }

            var stats = _stats[operationName];
            stats.RecordOperation(duration, success);
        }

        _logger.LogDebug("Operation recorded: {Operation} took {Duration}ms, Success: {Success}",
            operationName, duration.TotalMilliseconds, success);
    }
}