namespace GamificationEngine.Application.DTOs;

/// <summary>
/// Data transfer object for rule information
/// </summary>
public sealed class RuleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IEnumerable<string> Triggers { get; set; } = new List<string>();
    public IEnumerable<ConditionDto> Conditions { get; set; } = new List<ConditionDto>();
    public IEnumerable<RewardDto> Rewards { get; set; } = new List<RewardDto>();
    public IEnumerable<SpendingDto> Spendings { get; set; } = new List<SpendingDto>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Data transfer object for creating a rule
/// </summary>
public sealed class CreateRuleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public IEnumerable<string> Triggers { get; set; } = new List<string>();
    public IEnumerable<ConditionDto> Conditions { get; set; } = new List<ConditionDto>();
    public IEnumerable<RewardDto> Rewards { get; set; } = new List<RewardDto>();
    public IEnumerable<SpendingDto> Spendings { get; set; } = new List<SpendingDto>();
}

/// <summary>
/// Data transfer object for updating a rule
/// </summary>
public sealed class UpdateRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IEnumerable<string> Triggers { get; set; } = new List<string>();
    public IEnumerable<ConditionDto> Conditions { get; set; } = new List<ConditionDto>();
    public IEnumerable<RewardDto> Rewards { get; set; } = new List<RewardDto>();
    public IEnumerable<SpendingDto> Spendings { get; set; } = new List<SpendingDto>();
}

/// <summary>
/// Data transfer object for condition information
/// </summary>
public sealed class ConditionDto
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Data transfer object for reward information
/// </summary>
public sealed class RewardDto
{
    public string Type { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public long? Amount { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Data transfer object for spending information
/// </summary>
public sealed class SpendingDto
{
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
}
