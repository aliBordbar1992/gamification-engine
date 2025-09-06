namespace GamificationEngine.Domain.Webhooks;

/// <summary>
/// Represents a webhook configuration for external notifications
/// </summary>
public class Webhook
{
    // EF Core requires a parameterless constructor
    protected Webhook() { }

    public Webhook(string id, string name, string url, string secret, IEnumerable<string> eventTypes)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("ID cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty", nameof(url));
        if (!Uri.TryCreate(url, UriKind.Absolute, out _)) throw new ArgumentException("Invalid URL", nameof(url));
        if (eventTypes == null || !eventTypes.Any()) throw new ArgumentException("Event types cannot be empty", nameof(eventTypes));

        Id = id;
        Name = name;
        Url = url;
        Secret = secret ?? string.Empty;
        _eventTypes = new HashSet<string>(eventTypes, StringComparer.OrdinalIgnoreCase);
        IsActive = true;
        RetryCount = 3;
        TimeoutSeconds = 30;
        CreatedAt = DateTime.UtcNow;
    }

    private readonly HashSet<string> _eventTypes = new(StringComparer.OrdinalIgnoreCase);

    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public IReadOnlyCollection<string> EventTypes => _eventTypes;
    public bool IsActive { get; set; }
    public int RetryCount { get; set; }
    public int TimeoutSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public string? LastError { get; set; }

    /// <summary>
    /// Adds an event type to the webhook
    /// </summary>
    /// <param name="eventType">The event type to add</param>
    public void AddEventType(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("Event type cannot be empty", nameof(eventType));
        _eventTypes.Add(eventType);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes an event type from the webhook
    /// </summary>
    /// <param name="eventType">The event type to remove</param>
    public void RemoveEventType(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("Event type cannot be empty", nameof(eventType));
        _eventTypes.Remove(eventType);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the webhook URL
    /// </summary>
    /// <param name="url">The new URL</param>
    public void UpdateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be empty", nameof(url));
        if (!Uri.TryCreate(url, UriKind.Absolute, out _)) throw new ArgumentException("Invalid URL", nameof(url));

        Url = url;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the webhook secret
    /// </summary>
    /// <param name="secret">The new secret</param>
    public void UpdateSecret(string secret)
    {
        Secret = secret ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the webhook
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the webhook
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a successful webhook trigger
    /// </summary>
    public void RecordTrigger()
    {
        LastTriggeredAt = DateTime.UtcNow;
        LastError = null;
    }

    /// <summary>
    /// Records a webhook error
    /// </summary>
    /// <param name="error">The error message</param>
    public void RecordError(string error)
    {
        LastError = error;
        UpdatedAt = DateTime.UtcNow;
    }
}
