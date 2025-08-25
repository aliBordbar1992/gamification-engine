using GamificationEngine.Integration.Tests.Infrastructure.Testing;

namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Result of a performance comparison
/// </summary>
public class PerformanceComparison
{
    public PerformanceBaseline Baseline { get; set; } = null!;
    public LoadTestResult CurrentResult { get; set; } = null!;
    public TimeSpan ResponseTimeChange { get; set; }
    public double ResponseTimeChangePercentage { get; set; }
    public double SuccessRateChange { get; set; }
    public bool IsWithinTolerance { get; set; }
}