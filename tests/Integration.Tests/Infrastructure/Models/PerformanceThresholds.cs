namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Performance thresholds for testing
/// </summary>
public class PerformanceThresholds
{
    /// <summary>
    /// Maximum acceptable response time for API endpoints (in milliseconds)
    /// </summary>
    public int MaxResponseTimeMs { get; set; } = 1000;

    /// <summary>
    /// Maximum acceptable test execution time (in milliseconds)
    /// </summary>
    public int MaxTestExecutionTimeMs { get; set; } = 30000;

    /// <summary>
    /// Minimum acceptable success rate for tests (0.0 to 1.0)
    /// </summary>
    public double MinSuccessRate { get; set; } = 0.95;

    /// <summary>
    /// Maximum acceptable memory usage (in MB)
    /// </summary>
    public int MaxMemoryUsageMb { get; set; } = 512;

    /// <summary>
    /// Maximum acceptable CPU usage percentage
    /// </summary>
    public double MaxCpuUsagePercent { get; set; } = 80.0;
}