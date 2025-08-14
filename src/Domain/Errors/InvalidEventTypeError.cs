namespace GamificationEngine.Domain.Errors;

/// <summary>
/// Error raised when an event type is invalid
/// </summary>
public class InvalidEventTypeError : DomainError
{
    public InvalidEventTypeError(string message) : base("INVALID_EVENT_TYPE", message)
    {
    }
}