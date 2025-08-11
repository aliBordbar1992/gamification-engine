namespace GamificationEngine.Domain.Rewards;

public abstract class Reward
{
    public string Type { get; }

    protected Reward(string type)
    {
        Type = type;
    }
}