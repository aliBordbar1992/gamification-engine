namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Options for configuring load tests
/// </summary>
public class LoadTestOptions
{
    public int Concurrency { get; set; } = 10;
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableMetricsCollection { get; set; } = true;
}