using GamificationEngine.Domain.Rewards;

namespace GamificationEngine.Domain.Rules;

public sealed class Rule
{
    public string RuleId { get; }
    public string Name { get; }
    public IReadOnlyCollection<string> Triggers { get; }
    public IReadOnlyCollection<Condition> Conditions { get; }
    public IReadOnlyCollection<Reward> Rewards { get; }

    public Rule(string ruleId, string name, IEnumerable<string> triggers, IEnumerable<Condition> conditions, IEnumerable<Reward> rewards)
    {
        if (string.IsNullOrWhiteSpace(ruleId)) throw new ArgumentException("ruleId cannot be empty", nameof(ruleId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name cannot be empty", nameof(name));

        RuleId = ruleId;
        Name = name;
        Triggers = triggers?.ToArray() ?? Array.Empty<string>();
        Conditions = conditions?.ToArray() ?? Array.Empty<Condition>();
        Rewards = rewards?.ToArray() ?? Array.Empty<Reward>();
    }
}