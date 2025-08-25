using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Configuration;

/// <summary>
/// Configures test monitoring, metrics collection, and performance tracking
/// </summary>
public static class TestMonitoringConfiguration
{
    /// <summary>
    /// Adds test monitoring services to the service collection
    /// </summary>
    public static IServiceCollection AddTestMonitoring(
        this IServiceCollection services,
        TestSettings testSettings)
    {
        if (testSettings.EnableDetailedLogging)
        {
            services.AddSingleton<ITestMetricsCollector, TestMetricsCollector>();
            services.AddSingleton<ITestPerformanceMonitor, TestPerformanceMonitor>();
        }

        return services;
    }

    /// <summary>
    /// Configures test monitoring with custom settings
    /// </summary>
    public static IServiceCollection AddTestMonitoring(
        this IServiceCollection services,
        Action<TestMonitoringOptions> configureOptions)
    {
        var options = new TestMonitoringOptions();
        configureOptions(options);

        services.AddSingleton(options);
        services.AddSingleton<ITestMetricsCollector, TestMetricsCollector>();
        services.AddSingleton<ITestPerformanceMonitor, TestPerformanceMonitor>();

        return services;
    }
}

/// <summary>
/// Options for configuring test monitoring
/// </summary>
public class TestMonitoringOptions
{
    public bool EnableMetricsCollection { get; set; } = true;
    public bool EnablePerformanceMonitoring { get; set; } = true;
    public bool EnableTestExecutionTracking { get; set; } = true;
    public TimeSpan MetricsCollectionInterval { get; set; } = TimeSpan.FromSeconds(1);
    public int MaxMetricsHistorySize { get; set; } = 1000;
    public bool EnableRealTimeReporting { get; set; } = false;
}

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

/// <summary>
/// Interface for monitoring test performance
/// </summary>
public interface ITestPerformanceMonitor
{
    /// <summary>
    /// Starts monitoring a test operation
    /// </summary>
    ITestOperationMonitor StartOperation(string operationName);

    /// <summary>
    /// Gets performance statistics for an operation
    /// </summary>
    TestPerformanceStats GetPerformanceStats(string operationName);

    /// <summary>
    /// Gets all performance statistics
    /// </summary>
    IReadOnlyDictionary<string, TestPerformanceStats> GetAllPerformanceStats();

    /// <summary>
    /// Clears all performance data
    /// </summary>
    void ClearPerformanceData();
}

/// <summary>
/// Implementation of test metrics collector
/// </summary>
public class TestMetricsCollector : ITestMetricsCollector
{
    private readonly List<TestMetric> _metrics;
    private readonly Dictionary<string, int> _counters;
    private readonly TestMonitoringOptions _options;
    private readonly ILogger<TestMetricsCollector> _logger;

    public TestMetricsCollector(TestMonitoringOptions options, ILogger<TestMetricsCollector> logger)
    {
        _options = options;
        _logger = logger;
        _metrics = new List<TestMetric>();
        _counters = new Dictionary<string, int>();
    }

    public void RecordMetric(string name, double value, string? unit = null, Dictionary<string, object>? tags = null)
    {
        var metric = new TestMetric
        {
            Name = name,
            Value = value,
            Unit = unit,
            Tags = tags ?? new Dictionary<string, object>(),
            Timestamp = DateTime.UtcNow
        };

        lock (_metrics)
        {
            _metrics.Add(metric);

            // Maintain history size limit
            if (_metrics.Count > _options.MaxMetricsHistorySize)
            {
                _metrics.RemoveAt(0);
            }
        }

        _logger.LogDebug("Metric recorded: {Name} = {Value} {Unit}", name, value, unit);
    }

    public void IncrementCounter(string name, int increment = 1, Dictionary<string, object>? tags = null)
    {
        lock (_counters)
        {
            if (!_counters.ContainsKey(name))
            {
                _counters[name] = 0;
            }
            _counters[name] += increment;
        }

        // Also record as a metric for historical tracking
        RecordMetric(name, _counters[name], "count", tags);
    }

    public IDisposable RecordDuration(string name, Dictionary<string, object>? tags = null)
    {
        return new TestDurationRecorder(this, name, tags);
    }

    public IReadOnlyList<TestMetric> GetMetrics()
    {
        lock (_metrics)
        {
            return _metrics.ToList().AsReadOnly();
        }
    }

    public IEnumerable<TestMetric> GetMetricsByName(string name)
    {
        lock (_metrics)
        {
            return _metrics.Where(m => m.Name == name).ToList();
        }
    }

    public void ClearMetrics()
    {
        lock (_metrics)
        {
            _metrics.Clear();
        }
        lock (_counters)
        {
            _counters.Clear();
        }
    }

    private class TestDurationRecorder : IDisposable
    {
        private readonly TestMetricsCollector _collector;
        private readonly string _name;
        private readonly Dictionary<string, object>? _tags;
        private readonly Stopwatch _stopwatch;

        public TestDurationRecorder(TestMetricsCollector collector, string name, Dictionary<string, object>? tags)
        {
            _collector = collector;
            _name = name;
            _tags = tags;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _collector.RecordMetric($"{_name}_duration", _stopwatch.ElapsedMilliseconds, "ms", _tags);
        }
    }
}

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

/// <summary>
/// Interface for monitoring individual test operations
/// </summary>
public interface ITestOperationMonitor : IDisposable
{
    /// <summary>
    /// Marks the operation as successful
    /// </summary>
    void MarkSuccess();

    /// <summary>
    /// Marks the operation as failed
    /// </summary>
    void MarkFailure();
}

/// <summary>
/// Implementation of test operation monitor
/// </summary>
public class TestOperationMonitor : ITestOperationMonitor
{
    private readonly TestPerformanceMonitor _monitor;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    public TestOperationMonitor(TestPerformanceMonitor monitor, string operationName)
    {
        _monitor = monitor;
        _operationName = operationName;
        _stopwatch = Stopwatch.StartNew();
    }

    public void MarkSuccess()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordOperation(_operationName, _stopwatch.Elapsed, true);
        }
    }

    public void MarkFailure()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordOperation(_operationName, _stopwatch.Elapsed, false);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordOperation(_operationName, _stopwatch.Elapsed, true);
            _disposed = true;
        }
    }
}

/// <summary>
/// Represents a test metric
/// </summary>
public class TestMetric
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string? Unit { get; set; }
    public Dictionary<string, object> Tags { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Represents performance statistics for a test operation
/// </summary>
public class TestPerformanceStats
{
    private readonly List<TimeSpan> _durations = new();
    private readonly List<bool> _results = new();

    public int TotalOperations => _durations.Count;
    public int SuccessfulOperations => _results.Count(r => r);
    public int FailedOperations => _results.Count(r => !r);
    public double SuccessRate => TotalOperations > 0 ? (double)SuccessfulOperations / TotalOperations : 0;

    public TimeSpan AverageDuration => _durations.Count > 0
        ? TimeSpan.FromMilliseconds(_durations.Average(d => d.TotalMilliseconds))
        : TimeSpan.Zero;

    public TimeSpan MinDuration => _durations.Count > 0 ? _durations.Min() : TimeSpan.Zero;
    public TimeSpan MaxDuration => _durations.Count > 0 ? _durations.Max() : TimeSpan.Zero;

    internal void RecordOperation(TimeSpan duration, bool success)
    {
        _durations.Add(duration);
        _results.Add(success);
    }
}