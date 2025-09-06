using GamificationEngine.Application.Abstractions;
using System.Collections.Concurrent;

namespace GamificationEngine.Infrastructure.Metrics;

/// <summary>
/// In-memory implementation of performance metrics
/// </summary>
public class InMemoryPerformanceMetrics : IPerformanceMetrics
{
    private readonly ConcurrentDictionary<string, TimingData> _timings = new();
    private readonly ConcurrentDictionary<string, CounterData> _counters = new();
    private readonly ConcurrentDictionary<string, GaugeData> _gauges = new();
    private readonly ConcurrentDictionary<string, HistogramData> _histograms = new();
    private readonly object _lock = new();

    public void RecordTiming(string operation, TimeSpan duration, IDictionary<string, string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(operation))
            throw new ArgumentException("Operation cannot be empty", nameof(operation));

        var key = GetKeyWithTags(operation, tags);

        _timings.AddOrUpdate(key,
            new TimingData(duration),
            (_, existing) => existing.AddTiming(duration));
    }

    public void IncrementCounter(string metric, long value = 1, IDictionary<string, string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(metric))
            throw new ArgumentException("Metric cannot be empty", nameof(metric));

        var key = GetKeyWithTags(metric, tags);

        _counters.AddOrUpdate(key,
            new CounterData(value),
            (_, existing) => existing.Increment(value));
    }

    public void RecordGauge(string metric, double value, IDictionary<string, string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(metric))
            throw new ArgumentException("Metric cannot be empty", nameof(metric));

        var key = GetKeyWithTags(metric, tags);

        _gauges.AddOrUpdate(key,
            new GaugeData(value),
            (_, existing) => existing.UpdateValue(value));
    }

    public void RecordHistogram(string metric, double value, IDictionary<string, string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(metric))
            throw new ArgumentException("Metric cannot be empty", nameof(metric));

        var key = GetKeyWithTags(metric, tags);

        _histograms.AddOrUpdate(key,
            new HistogramData(value),
            (_, existing) => existing.AddValue(value));
    }

    public IDisposable StartTimer(string operation, IDictionary<string, string>? tags = null)
    {
        return new PerformanceTimer(this, operation, tags);
    }

    public PerformanceStatistics GetStatistics()
    {
        lock (_lock)
        {
            var timings = _timings.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.GetStatistics());

            var counters = _counters.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.GetStatistics());

            var gauges = _gauges.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.GetStatistics());

            var histograms = _histograms.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.GetStatistics());

            return new PerformanceStatistics(timings, counters, gauges, histograms, DateTime.UtcNow);
        }
    }

    public void Reset()
    {
        _timings.Clear();
        _counters.Clear();
        _gauges.Clear();
        _histograms.Clear();
    }

    private static string GetKeyWithTags(string baseKey, IDictionary<string, string>? tags)
    {
        if (tags == null || !tags.Any())
            return baseKey;

        var tagString = string.Join(",", tags.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{baseKey}[{tagString}]";
    }

    private class PerformanceTimer : IDisposable
    {
        private readonly InMemoryPerformanceMetrics _metrics;
        private readonly string _operation;
        private readonly IDictionary<string, string>? _tags;
        private readonly DateTime _startTime;

        public PerformanceTimer(InMemoryPerformanceMetrics metrics, string operation, IDictionary<string, string>? tags)
        {
            _metrics = metrics;
            _operation = operation;
            _tags = tags;
            _startTime = DateTime.UtcNow;
        }

        public void Dispose()
        {
            var duration = DateTime.UtcNow - _startTime;
            _metrics.RecordTiming(_operation, duration, _tags);
        }
    }

    private class TimingData
    {
        private readonly List<TimeSpan> _durations = new();
        private readonly object _lock = new();

        public TimingData(TimeSpan duration)
        {
            _durations.Add(duration);
        }

        public TimingData AddTiming(TimeSpan duration)
        {
            lock (_lock)
            {
                _durations.Add(duration);
                return this;
            }
        }

        public TimingStatistics GetStatistics()
        {
            lock (_lock)
            {
                if (!_durations.Any())
                    return new TimingStatistics(0, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

                var sortedDurations = _durations.OrderBy(d => d).ToList();
                var count = _durations.Count;
                var total = _durations.Aggregate(TimeSpan.Zero, (sum, d) => sum.Add(d));
                var average = TimeSpan.FromTicks(total.Ticks / count);
                var min = sortedDurations.First();
                var max = sortedDurations.Last();
                var p95Index = (int)(count * 0.95);
                var p99Index = (int)(count * 0.99);
                var p95 = sortedDurations[Math.Min(p95Index, count - 1)];
                var p99 = sortedDurations[Math.Min(p99Index, count - 1)];

                return new TimingStatistics(count, total, average, min, max, p95, p99);
            }
        }
    }

    private class CounterData
    {
        private long _totalCount;
        private DateTime _lastIncrement;

        public CounterData(long initialValue)
        {
            _totalCount = initialValue;
            _lastIncrement = DateTime.UtcNow;
        }

        public CounterData Increment(long value)
        {
            Interlocked.Add(ref _totalCount, value);
            _lastIncrement = DateTime.UtcNow;
            return this;
        }

        public CounterStatistics GetStatistics()
        {
            var now = DateTime.UtcNow;
            var timeSinceLastIncrement = now - _lastIncrement;
            var countPerMinute = timeSinceLastIncrement.TotalMinutes > 0
                ? _totalCount / timeSinceLastIncrement.TotalMinutes
                : 0;

            return new CounterStatistics(_totalCount, (long)countPerMinute, _lastIncrement);
        }
    }

    private class GaugeData
    {
        private double _currentValue;
        private double _minValue;
        private double _maxValue;
        private double _sumValue;
        private int _updateCount;
        private DateTime _lastUpdate;

        public GaugeData(double initialValue)
        {
            _currentValue = _minValue = _maxValue = _sumValue = initialValue;
            _updateCount = 1;
            _lastUpdate = DateTime.UtcNow;
        }

        public GaugeData UpdateValue(double value)
        {
            _currentValue = value;
            _minValue = Math.Min(_minValue, value);
            _maxValue = Math.Max(_maxValue, value);
            _sumValue += value;
            _updateCount++;
            _lastUpdate = DateTime.UtcNow;
            return this;
        }

        public GaugeStatistics GetStatistics()
        {
            var average = _updateCount > 0 ? _sumValue / _updateCount : 0;
            return new GaugeStatistics(_currentValue, _minValue, _maxValue, average, _lastUpdate);
        }
    }

    private class HistogramData
    {
        private readonly List<double> _values = new();
        private readonly object _lock = new();

        public HistogramData(double initialValue)
        {
            _values.Add(initialValue);
        }

        public HistogramData AddValue(double value)
        {
            lock (_lock)
            {
                _values.Add(value);
                return this;
            }
        }

        public HistogramStatistics GetStatistics()
        {
            lock (_lock)
            {
                if (!_values.Any())
                    return new HistogramStatistics(0, 0, 0, 0, 0, 0, 0);

                var sortedValues = _values.OrderBy(v => v).ToList();
                var count = _values.Count;
                var sum = _values.Sum();
                var average = sum / count;
                var min = sortedValues.First();
                var max = sortedValues.Last();
                var p95Index = (int)(count * 0.95);
                var p99Index = (int)(count * 0.99);
                var p95 = sortedValues[Math.Min(p95Index, count - 1)];
                var p99 = sortedValues[Math.Min(p99Index, count - 1)];

                return new HistogramStatistics(count, sum, average, min, max, p95, p99);
            }
        }
    }
}
