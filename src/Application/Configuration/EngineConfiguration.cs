namespace GamificationEngine.Application.Configuration;

public sealed class EngineConfiguration
{
    public EngineSettings Engine { get; set; } = new();
    public List<EventDefinition> Events { get; set; } = new();
    public List<PointCategory> PointCategories { get; set; } = new();
    public List<BadgeDefinition> Badges { get; set; } = new();
    public List<TrophyDefinition> Trophies { get; set; } = new();
    public List<LevelDefinition> Levels { get; set; } = new();
    public List<RuleDefinition> Rules { get; set; } = new();
}

public sealed class EngineSettings
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Timezone { get; set; } = "UTC";
    public RetentionSettings Retention { get; set; } = new();
    public EventValidationSettings EventValidation { get; set; } = new();
}

public sealed class RetentionSettings
{
    public int EventsDays { get; set; }
    public int RewardsDays { get; set; }
}

public sealed class EventValidationSettings
{
    public bool Enabled { get; set; } = true;
    public bool RejectUnknownEvents { get; set; } = true;
    public bool ValidatePayloadSchema { get; set; } = true;
}

public sealed class EventDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string>? PayloadSchema { get; set; }
}

public sealed class PointCategory
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Aggregation { get; set; } = string.Empty;
}

public sealed class BadgeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public bool Visible { get; set; }
}

public sealed class TrophyDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public bool Visible { get; set; }
}

public sealed class LevelDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LevelCriteria Criteria { get; set; } = new();
}

public sealed class LevelCriteria
{
    public string Category { get; set; } = string.Empty;
    public long MinPoints { get; set; }
}

public sealed class RuleDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<RuleTrigger> Triggers { get; set; } = new();
    public List<RuleCondition> Conditions { get; set; } = new();
    public List<RuleReward> Rewards { get; set; } = new();
    public RuleMetadata? Metadata { get; set; }
}

public sealed class RuleTrigger
{
    public string Event { get; set; } = string.Empty;
}

public sealed class RuleCondition
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public sealed class RuleReward
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public sealed class RuleMetadata
{
    public bool? OneTime { get; set; }
    public bool? Repeatable { get; set; }
    public bool? NotifyPlatform { get; set; }
    public string? NotifyTemplate { get; set; }
    public bool? WebhookOnAward { get; set; }
}