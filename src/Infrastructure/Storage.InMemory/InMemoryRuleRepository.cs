using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rules;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of IRuleRepository for testing and development
/// </summary>
public class InMemoryRuleRepository : IRuleRepository
{
    private readonly Dictionary<string, Rule> _rules = new();
    private readonly object _lock = new();

    public Task StoreAsync(Rule rule)
    {
        if (rule == null) throw new ArgumentNullException(nameof(rule));

        lock (_lock)
        {
            _rules[rule.RuleId] = rule;
        }

        return Task.CompletedTask;
    }

    public Task<Rule?> GetByIdAsync(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId)) throw new ArgumentException("ruleId cannot be empty", nameof(ruleId));

        lock (_lock)
        {
            _rules.TryGetValue(ruleId, out var rule);
            return Task.FromResult(rule);
        }
    }

    public Task<IEnumerable<Rule>> GetByTriggerAsync(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("eventType cannot be empty", nameof(eventType));

        lock (_lock)
        {
            var matchingRules = _rules.Values
                .Where(rule => rule.IsActive && rule.ShouldTrigger(eventType))
                .ToList();

            return Task.FromResult<IEnumerable<Rule>>(matchingRules);
        }
    }

    public Task<IEnumerable<Rule>> GetAllActiveAsync()
    {
        lock (_lock)
        {
            var activeRules = _rules.Values
                .Where(rule => rule.IsActive)
                .ToList();

            return Task.FromResult<IEnumerable<Rule>>(activeRules);
        }
    }

    public Task<IEnumerable<Rule>> GetAllAsync()
    {
        lock (_lock)
        {
            var allRules = _rules.Values.ToList();
            return Task.FromResult<IEnumerable<Rule>>(allRules);
        }
    }

    public Task UpdateAsync(Rule rule)
    {
        if (rule == null) throw new ArgumentNullException(nameof(rule));

        lock (_lock)
        {
            if (!_rules.ContainsKey(rule.RuleId))
            {
                throw new InvalidOperationException($"Rule with ID '{rule.RuleId}' does not exist");
            }

            _rules[rule.RuleId] = rule;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId)) throw new ArgumentException("ruleId cannot be empty", nameof(ruleId));

        lock (_lock)
        {
            _rules.Remove(ruleId);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId)) throw new ArgumentException("ruleId cannot be empty", nameof(ruleId));

        lock (_lock)
        {
            return Task.FromResult(_rules.ContainsKey(ruleId));
        }
    }

    /// <summary>
    /// Clears all rules from the repository (useful for testing)
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _rules.Clear();
        }
    }

    /// <summary>
    /// Gets the count of rules in the repository (useful for testing)
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _rules.Count;
            }
        }
    }
}
