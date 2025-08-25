namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Performance summary for all tests
/// </summary>
public class TestPerformanceSummary
{
    public int TotalTests { get; set; }
    public int SuccessfulTests { get; set; }
    public int FailedTests { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan AverageExecutionTime { get; set; }
    public TimeSpan MinExecutionTime { get; set; }
    public TimeSpan MaxExecutionTime { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
}