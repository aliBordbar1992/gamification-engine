namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Event retention configuration settings
/// </summary>
public class EventRetentionSettings
{
    public int RetentionDays { get; set; } = 30;
    public int BatchSize { get; set; } = 100;
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(24);
}