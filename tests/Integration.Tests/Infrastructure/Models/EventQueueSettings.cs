namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Event queue configuration settings
/// </summary>
public class EventQueueSettings
{
    public TimeSpan ProcessingInterval { get; set; } = TimeSpan.FromSeconds(1);
    public int MaxConcurrentProcessing { get; set; } = 2;
    public int MaxQueueSize { get; set; } = 1000;
    public bool EnableRetryOnFailure { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
}