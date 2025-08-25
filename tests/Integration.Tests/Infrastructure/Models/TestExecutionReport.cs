namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Comprehensive test execution report
/// </summary>
public class TestExecutionReport
{
    public DateTime GeneratedAt { get; set; }
    public TestPerformanceSummary Summary { get; set; } = null!;
    public IReadOnlyDictionary<string, TestCategoryStats> CategoryStats { get; set; } = null!;
    public IReadOnlyDictionary<string, TestExecutionStats> TestStats { get; set; } = null!;
    public List<string> Recommendations { get; set; } = new();
}