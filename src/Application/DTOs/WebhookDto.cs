namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data transfer object for webhook information
/// </summary>
public sealed class WebhookDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public IEnumerable<string> EventTypes { get; set; } = new List<string>();
    public bool IsActive { get; set; }
    public int RetryCount { get; set; }
    public int TimeoutSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public string? LastError { get; set; }
}

/// <summary>
/// Data transfer object for registering a webhook
/// </summary>
public sealed class RegisterWebhookDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public IEnumerable<string> EventTypes { get; set; } = new List<string>();
    public bool IsActive { get; set; } = true;
    public int RetryCount { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Data transfer object for updating a webhook
/// </summary>
public sealed class UpdateWebhookDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public IEnumerable<string> EventTypes { get; set; } = new List<string>();
    public bool IsActive { get; set; }
    public int RetryCount { get; set; }
    public int TimeoutSeconds { get; set; }
}

/// <summary>
/// Data transfer object for reward events sent via webhook
/// </summary>
public sealed class RewardEventDto
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string RewardType { get; set; } = string.Empty;
    public string RewardId { get; set; } = string.Empty;
    public string RewardName { get; set; } = string.Empty;
    public long? PointsAmount { get; set; }
    public string? PointCategory { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Data transfer object for webhook test results
/// </summary>
public sealed class WebhookTestResultDto
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTime TestedAt { get; set; }
}
