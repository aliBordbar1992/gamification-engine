namespace GamificationEngine.Domain.Errors;

/// <summary>
/// Error raised when event storage fails
/// </summary>
public class EventStorageError : DomainError
{
    public EventStorageError(string message) : base("EVENT_STORAGE_ERROR", message)
    {
    }
}