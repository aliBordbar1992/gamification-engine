namespace GamificationEngine.Application.Configuration;

public sealed class EngineConfiguration
{
    public EngineSettings Engine { get; set; } = new();
    public List<EventDefinition> Events { get; set; } = new();
    public List<PointCategory> PointCategories { get; set; } = new();
    public List<BadgeDefinition> Badges { get; set; } = new();
}

public sealed class EngineSettings
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Timezone { get; set; } = "UTC";
    public RetentionSettings Retention { get; set; } = new();
}

public sealed class RetentionSettings
{
    public int EventsDays { get; set; }
    public int RewardsDays { get; set; }
}

public sealed class EventDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
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