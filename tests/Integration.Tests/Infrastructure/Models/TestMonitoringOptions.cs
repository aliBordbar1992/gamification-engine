namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

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