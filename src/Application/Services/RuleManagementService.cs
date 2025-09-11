using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for managing rules
/// </summary>
public class RuleManagementService : IRuleManagementService
{
    private readonly IRuleRepository _ruleRepository;

    public RuleManagementService(IRuleRepository ruleRepository)
    {
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
    }

    public async Task<Result<IEnumerable<RuleDto>, string>> GetAllRulesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rules = await _ruleRepository.GetAllAsync();
            var ruleDtos = rules.Select(MapToDto);
            return Result<IEnumerable<RuleDto>, string>.Success(ruleDtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RuleDto>, string>.Failure($"Failed to get all rules: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<RuleDto>, string>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rules = await _ruleRepository.GetAllActiveAsync();
            var ruleDtos = rules.Select(MapToDto);
            return Result<IEnumerable<RuleDto>, string>.Success(ruleDtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RuleDto>, string>.Failure($"Failed to get active rules: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<RuleDto>, string>> GetRulesByTriggerAsync(string eventType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(eventType))
                return Result<IEnumerable<RuleDto>, string>.Failure("Event type cannot be empty");

            var rules = await _ruleRepository.GetByTriggerAsync(eventType);
            var ruleDtos = rules.Select(MapToDto);
            return Result<IEnumerable<RuleDto>, string>.Success(ruleDtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RuleDto>, string>.Failure($"Failed to get rules by trigger: {ex.Message}");
        }
    }

    public async Task<Result<RuleDto, string>> GetRuleByIdAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ruleId))
                return Result<RuleDto, string>.Failure("Rule ID cannot be empty");

            var rule = await _ruleRepository.GetByIdAsync(ruleId);
            if (rule == null)
                return Result<RuleDto, string>.Failure("Rule not found");

            return Result<RuleDto, string>.Success(MapToDto(rule));
        }
        catch (Exception ex)
        {
            return Result<RuleDto, string>.Failure($"Failed to get rule by ID: {ex.Message}");
        }
    }

    public async Task<Result<RuleDto, string>> CreateRuleAsync(CreateRuleDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Id))
                return Result<RuleDto, string>.Failure("Rule ID cannot be empty");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return Result<RuleDto, string>.Failure("Rule name cannot be empty");

            if (!dto.Triggers.Any())
                return Result<RuleDto, string>.Failure("Rule must have at least one trigger");

            // A rule must have either rewards or spendings (or both)
            var hasRewards = dto.Rewards != null && dto.Rewards.Any();
            var hasSpendings = dto.Spendings != null && dto.Spendings.Any();
            if (!hasRewards && !hasSpendings)
                return Result<RuleDto, string>.Failure("Rule must have at least one reward or spending");

            // Check if rule already exists
            var exists = await _ruleRepository.ExistsAsync(dto.Id);
            if (exists)
                return Result<RuleDto, string>.Failure("Rule with this ID already exists");

            // TODO: Implement rule creation logic
            // This would involve creating the domain Rule object from the DTO
            // For now, return a placeholder
            return Result<RuleDto, string>.Failure("Rule creation not yet implemented");
        }
        catch (Exception ex)
        {
            return Result<RuleDto, string>.Failure($"Failed to create rule: {ex.Message}");
        }
    }

    public async Task<Result<RuleDto, string>> UpdateRuleAsync(string ruleId, UpdateRuleDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ruleId))
                return Result<RuleDto, string>.Failure("Rule ID cannot be empty");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return Result<RuleDto, string>.Failure("Rule name cannot be empty");

            if (!dto.Triggers.Any())
                return Result<RuleDto, string>.Failure("Rule must have at least one trigger");

            // A rule must have either rewards or spendings (or both)
            var hasRewards = dto.Rewards != null && dto.Rewards.Any();
            var hasSpendings = dto.Spendings != null && dto.Spendings.Any();
            if (!hasRewards && !hasSpendings)
                return Result<RuleDto, string>.Failure("Rule must have at least one reward or spending");

            var rule = await _ruleRepository.GetByIdAsync(ruleId);
            if (rule == null)
                return Result<RuleDto, string>.Failure("Rule not found");

            // TODO: Implement rule update logic
            // This would involve updating the domain Rule object from the DTO
            // For now, return a placeholder
            return Result<RuleDto, string>.Failure("Rule update not yet implemented");
        }
        catch (Exception ex)
        {
            return Result<RuleDto, string>.Failure($"Failed to update rule: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> DeleteRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ruleId))
                return Result<bool, string>.Failure("Rule ID cannot be empty");

            var exists = await _ruleRepository.ExistsAsync(ruleId);
            if (!exists)
                return Result<bool, string>.Failure("Rule not found");

            await _ruleRepository.DeleteAsync(ruleId);
            return Result<bool, string>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool, string>.Failure($"Failed to delete rule: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> ActivateRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ruleId))
                return Result<bool, string>.Failure("Rule ID cannot be empty");

            var rule = await _ruleRepository.GetByIdAsync(ruleId);
            if (rule == null)
                return Result<bool, string>.Failure("Rule not found");

            // TODO: Implement rule activation logic
            // This would involve updating the rule's IsActive property
            // For now, return a placeholder
            return Result<bool, string>.Failure("Rule activation not yet implemented");
        }
        catch (Exception ex)
        {
            return Result<bool, string>.Failure($"Failed to activate rule: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> DeactivateRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ruleId))
                return Result<bool, string>.Failure("Rule ID cannot be empty");

            var rule = await _ruleRepository.GetByIdAsync(ruleId);
            if (rule == null)
                return Result<bool, string>.Failure("Rule not found");

            // TODO: Implement rule deactivation logic
            // This would involve updating the rule's IsActive property
            // For now, return a placeholder
            return Result<bool, string>.Failure("Rule deactivation not yet implemented");
        }
        catch (Exception ex)
        {
            return Result<bool, string>.Failure($"Failed to deactivate rule: {ex.Message}");
        }
    }

    private static RuleDto MapToDto(Rule rule)
    {
        return new RuleDto
        {
            Id = rule.RuleId,
            Name = rule.Name,
            Description = rule.Description ?? string.Empty,
            IsActive = rule.IsActive,
            Triggers = rule.Triggers,
            Conditions = rule.Conditions.Select(c => new ConditionDto
            {
                Type = c.Type,
                Parameters = new Dictionary<string, object>() // TODO: Map condition parameters
            }),
            Rewards = rule.Rewards.Select(r => new RewardDto
            {
                Type = r.Type,
                TargetId = r.RewardId,
                Amount = null, // TODO: Extract amount from parameters if needed
                Parameters = r.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            }),
            Spendings = rule.Spendings.Select(s => new SpendingDto
            {
                Category = s.Category,
                Type = s.Type.ToString(),
                Attributes = s.Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            }),
            CreatedAt = rule.CreatedAt.DateTime,
            UpdatedAt = rule.UpdatedAt.DateTime
        };
    }
}
