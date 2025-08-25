using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

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