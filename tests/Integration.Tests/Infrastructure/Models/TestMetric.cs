namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Represents a test metric
/// </summary>
public class TestMetric
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string? Unit { get; set; }
    public Dictionary<string, object> Tags { get; set; } = new();
    public DateTime Timestamp { get; set; }
}