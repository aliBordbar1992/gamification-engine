namespace GamificationEngine.Domain.Errors;

/// <summary>
/// Error raised when a parameter is invalid
/// </summary>
public class InvalidParameterError : DomainError
{
    public InvalidParameterError(string message) : base("INVALID_PARAMETER", message)
    {
    }
}