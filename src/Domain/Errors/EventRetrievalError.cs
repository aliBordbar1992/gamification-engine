namespace GamificationEngine.Domain.Errors;

/// <summary>
/// Error raised when event retrieval fails
/// </summary>
public class EventRetrievalError : DomainError
{
    public EventRetrievalError(string message) : base("EVENT_RETRIEVAL_ERROR", message)
    {
    }
}