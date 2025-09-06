namespace GamificationEngine.Domain.Errors;

/// <summary>
/// Error raised when rule evaluation fails
/// </summary>
public class RuleEvaluationError : DomainError
{
    public RuleEvaluationError(string message) : base("RULE_EVALUATION_ERROR", message)
    {
    }
}
