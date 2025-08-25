namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Configuration options for test infrastructure
/// </summary>
public class TestInfrastructureOptions
{
    /// <summary>
    /// Default timeout for test operations
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Default polling interval for wait operations
    /// </summary>
    public TimeSpan DefaultPollInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Whether to enable detailed logging
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable performance monitoring
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = false;
}