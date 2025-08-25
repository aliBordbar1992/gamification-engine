namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Options for configuring stress tests
/// </summary>
public class StressTestOptions
{
    public int InitialConcurrency { get; set; } = 1;
    public int MaxConcurrency { get; set; } = 100;
    public int ConcurrencyStep { get; set; } = 5;
    public TimeSpan StepDuration { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan StepDelay { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public double SuccessRateThreshold { get; set; } = 0.95; // 95%
}