using GamificationEngine.Domain.Rewards;

namespace GamificationEngine.Domain.Rules;

/// <summary>
/// Represents a rule that can be evaluated against events
/// </summary>
public class Rule
{
    // EF Core requires a parameterless constructor
    protected Rule() { }

    public Rule(string ruleId, string name, string[] triggers, IReadOnlyCollection<Condition> conditions, IReadOnlyCollection<Reward> rewards)
    {
        if (string.IsNullOrWhiteSpace(ruleId)) throw new ArgumentException("ruleId cannot be empty", nameof(ruleId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name cannot be empty", nameof(name));
        if (triggers == null || triggers.Length == 0) throw new ArgumentException("triggers cannot be null or empty", nameof(triggers));
        if (conditions == null || conditions.Count == 0) throw new ArgumentException("conditions cannot be null or empty", nameof(conditions));
        if (rewards == null || rewards.Count == 0) throw new ArgumentException("rewards cannot be null or empty", nameof(rewards));

        RuleId = ruleId;
        Name = name;
        Triggers = triggers;
        Conditions = conditions;
        Rewards = rewards;
    }

    public string RuleId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string[] Triggers { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<Condition> Conditions { get; set; } = new List<Condition>();
    public IReadOnlyCollection<Reward> Rewards { get; set; } = new List<Reward>();
}