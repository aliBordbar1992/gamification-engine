namespace GamificationEngine.Domain.Errors;

/// <summary>
/// Error that occurs when executing a spending fails
/// </summary>
public sealed class SpendingExecutionError : DomainError
{
    public SpendingExecutionError(string message) : base("SPENDING_EXECUTION_ERROR", message)
    {
    }

    public SpendingExecutionError(string message, Exception innerException) : base("SPENDING_EXECUTION_ERROR", $"{message}: {innerException.Message}")
    {
    }
}
