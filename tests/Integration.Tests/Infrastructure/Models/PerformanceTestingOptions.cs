namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Options for configuring performance testing
/// </summary>
public class PerformanceTestingOptions
{
    /// <summary>
    /// Whether to enable performance testing infrastructure
    /// </summary>
    public bool EnablePerformanceTesting { get; set; } = true;

    /// <summary>
    /// Whether to enable load testing capabilities
    /// </summary>
    public bool EnableLoadTesting { get; set; } = true;

    /// <summary>
    /// Whether to enable stress testing capabilities
    /// </summary>
    public bool EnableStressTesting { get; set; } = true;

    /// <summary>
    /// Whether to enable baseline testing capabilities
    /// </summary>
    public bool EnableBaselineTesting { get; set; } = true;

    /// <summary>
    /// Whether to enable test execution monitoring
    /// </summary>
    public bool EnableTestExecutionMonitoring { get; set; } = true;

    /// <summary>
    /// Default concurrency for load tests
    /// </summary>
    public int DefaultLoadTestConcurrency { get; set; } = 10;

    /// <summary>
    /// Default duration for load tests
    /// </summary>
    public TimeSpan DefaultLoadTestDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Default number of iterations for baseline tests
    /// </summary>
    public int DefaultBaselineTestIterations { get; set; } = 100;

    /// <summary>
    /// Whether to enable real-time performance reporting
    /// </summary>
    public bool EnableRealTimeReporting { get; set; } = false;

    /// <summary>
    /// Whether to enable performance metrics export
    /// </summary>
    public bool EnableMetricsExport { get; set; } = true;

    /// <summary>
    /// Performance thresholds for alerts
    /// </summary>
    public PerformanceThresholds Thresholds { get; set; } = new();
}