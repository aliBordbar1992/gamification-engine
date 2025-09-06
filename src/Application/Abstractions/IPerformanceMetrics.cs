namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Interface for performance metrics collection
/// </summary>
public interface IPerformanceMetrics
{
    /// <summary>
    /// Records a timing measurement
    /// </summary>
    /// <param name="operation">The operation being measured</param>
    /// <param name="duration">The duration of the operation</param>
    /// <param name="tags">Optional tags for additional context</param>
    void RecordTiming(string operation, TimeSpan duration, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Records a counter increment
    /// </summary>
    /// <param name="metric">The metric name</param>
    /// <param name="value">The value to increment by</param>
    /// <param name="tags">Optional tags for additional context</param>
    void IncrementCounter(string metric, long value = 1, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Records a gauge value
    /// </summary>
    /// <param name="metric">The metric name</param>
    /// <param name="value">The gauge value</param>
    /// <param name="tags">Optional tags for additional context</param>
    void RecordGauge(string metric, double value, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Records a histogram value
    /// </summary>
    /// <param name="metric">The metric name</param>
    /// <param name="value">The histogram value</param>
    /// <param name="tags">Optional tags for additional context</param>
    void RecordHistogram(string metric, double value, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Creates a timer for measuring operation duration
    /// </summary>
    /// <param name="operation">The operation name</param>
    /// <param name="tags">Optional tags for additional context</param>
    /// <returns>A disposable timer</returns>
    IDisposable StartTimer(string operation, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Gets current performance statistics
    /// </summary>
    /// <returns>Performance statistics</returns>
    PerformanceStatistics GetStatistics();

    /// <summary>
    /// Resets all metrics
    /// </summary>
    void Reset();
}

/// <summary>
/// Performance statistics
/// </summary>
public record PerformanceStatistics(
    IDictionary<string, TimingStatistics> Timings,
    IDictionary<string, CounterStatistics> Counters,
    IDictionary<string, GaugeStatistics> Gauges,
    IDictionary<string, HistogramStatistics> Histograms,
    DateTime GeneratedAt
);

/// <summary>
/// Timing statistics for an operation
/// </summary>
public record TimingStatistics(
    long Count,
    TimeSpan TotalDuration,
    TimeSpan AverageDuration,
    TimeSpan MinDuration,
    TimeSpan MaxDuration,
    TimeSpan P95Duration,
    TimeSpan P99Duration
);

/// <summary>
/// Counter statistics
/// </summary>
public record CounterStatistics(
    long TotalCount,
    long CountPerMinute,
    DateTime LastIncrement
);

/// <summary>
/// Gauge statistics
/// </summary>
public record GaugeStatistics(
    double CurrentValue,
    double MinValue,
    double MaxValue,
    double AverageValue,
    DateTime LastUpdate
);

/// <summary>
/// Histogram statistics
/// </summary>
public record HistogramStatistics(
    long Count,
    double Sum,
    double Average,
    double Min,
    double Max,
    double P95,
    double P99
);
