namespace GamificationEngine.Domain.Errors;

public abstract class DomainError
{
    public string Code { get; }
    public string Message { get; }

    protected DomainError(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public override string ToString() => $"{Code}: {Message}";
}