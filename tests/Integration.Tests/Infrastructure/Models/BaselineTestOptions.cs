namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Options for configuring baseline tests
/// </summary>
public class BaselineTestOptions
{
    public int Iterations { get; set; } = 100;
    public TimeSpan DelayBetweenIterations { get; set; } = TimeSpan.FromMilliseconds(100);
    public bool EnableProgressLogging { get; set; } = true;
    public bool EnableMetricsCollection { get; set; } = true;
}