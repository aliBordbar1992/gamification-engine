namespace GamificationEngine.Domain.Rules;

public abstract class Condition
{
    public string Type { get; }

    protected Condition(string type)
    {
        Type = type;
    }
}