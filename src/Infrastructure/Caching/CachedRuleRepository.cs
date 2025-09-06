using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Infrastructure.Caching;

/// <summary>
/// Cached implementation of the rule repository
/// </summary>
public class CachedRuleRepository : IRuleRepository
{
    private readonly IRuleRepository _innerRepository;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _defaultCacheExpiration;

    private const string RuleCachePrefix = "rule:";
    private const string AllRulesCacheKey = "rules:all";
    private const string RulesByTriggerCachePrefix = "rules:trigger:";

    public CachedRuleRepository(IRuleRepository innerRepository, ICacheService cacheService, TimeSpan? defaultCacheExpiration = null)
    {
        _innerRepository = innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _defaultCacheExpiration = defaultCacheExpiration ?? TimeSpan.FromMinutes(15);
    }

    public async Task<Rule?> GetByIdAsync(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
            throw new ArgumentException("Rule ID cannot be empty", nameof(ruleId));

        var cacheKey = $"{RuleCachePrefix}{ruleId}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _innerRepository.GetByIdAsync(ruleId),
            _defaultCacheExpiration);
    }

    public async Task<IEnumerable<Rule>> GetAllAsync()
    {
        return await _cacheService.GetOrSetAsync(
            AllRulesCacheKey,
            async () => await _innerRepository.GetAllAsync(),
            _defaultCacheExpiration) ?? Enumerable.Empty<Rule>();
    }

    public async Task<IEnumerable<Rule>> GetAllActiveAsync()
    {
        return await _cacheService.GetOrSetAsync(
            "rules:active",
            async () => await _innerRepository.GetAllActiveAsync(),
            _defaultCacheExpiration) ?? Enumerable.Empty<Rule>();
    }

    public async Task<IEnumerable<Rule>> GetByTriggerAsync(string triggerEventType)
    {
        if (string.IsNullOrWhiteSpace(triggerEventType))
            throw new ArgumentException("Trigger event type cannot be empty", nameof(triggerEventType));

        var cacheKey = $"{RulesByTriggerCachePrefix}{triggerEventType}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _innerRepository.GetByTriggerAsync(triggerEventType),
            _defaultCacheExpiration) ?? Enumerable.Empty<Rule>();
    }

    public async Task StoreAsync(Rule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        await _innerRepository.StoreAsync(rule);

        // Invalidate related cache entries
        await InvalidateRuleCacheAsync(rule);
    }

    public async Task UpdateAsync(Rule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        await _innerRepository.UpdateAsync(rule);

        // Invalidate related cache entries
        await InvalidateRuleCacheAsync(rule);
    }

    public async Task DeleteAsync(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
            throw new ArgumentException("Rule ID cannot be empty", nameof(ruleId));

        // Get the rule first to know what triggers to invalidate
        var rule = await GetByIdAsync(ruleId);

        await _innerRepository.DeleteAsync(ruleId);

        // Invalidate cache entries
        await InvalidateRuleCacheAsync(rule);
    }

    public async Task<bool> ExistsAsync(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
            throw new ArgumentException("Rule ID cannot be empty", nameof(ruleId));

        var cacheKey = $"{RuleCachePrefix}{ruleId}";

        if (await _cacheService.ExistsAsync(cacheKey))
        {
            return true;
        }

        return await _innerRepository.ExistsAsync(ruleId);
    }

    private async Task InvalidateRuleCacheAsync(Rule? rule)
    {
        var keysToRemove = new List<string>();

        if (rule != null)
        {
            // Remove the specific rule cache entry
            keysToRemove.Add($"{RuleCachePrefix}{rule.RuleId}");

            // Remove trigger-specific cache entries
            foreach (var trigger in rule.Triggers)
            {
                keysToRemove.Add($"{RulesByTriggerCachePrefix}{trigger}");
            }
        }

        // Always remove the "all rules" cache entry
        keysToRemove.Add(AllRulesCacheKey);
        keysToRemove.Add("rules:active");

        if (keysToRemove.Any())
        {
            await _cacheService.RemoveManyAsync(keysToRemove);
        }
    }
}
