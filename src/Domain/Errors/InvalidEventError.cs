namespace GamificationEngine.Domain.Errors;

/// <summary>
/// Error raised when an event is invalid
/// </summary>
public class InvalidEventError : DomainError
{
    public InvalidEventError(string message) : base("INVALID_EVENT", message)
    {
    }
}