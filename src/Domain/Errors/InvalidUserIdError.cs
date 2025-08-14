namespace GamificationEngine.Domain.Errors;

/// <summary>
/// Error raised when a user ID is invalid
/// </summary>
public class InvalidUserIdError : DomainError
{
    public InvalidUserIdError(string message) : base("INVALID_USER_ID", message)
    {
    }
}