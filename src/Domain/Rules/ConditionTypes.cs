namespace GamificationEngine.Domain.Rules;

/// <summary>
/// Constants for condition types
/// </summary>
public static class ConditionTypes
{
    public const string AlwaysTrue = "alwaysTrue";
    public const string AttributeEquals = "attributeEquals";
    public const string Count = "count";
    public const string Threshold = "threshold";
    public const string Sequence = "sequence";
    public const string TimeSinceLastEvent = "timeSinceLastEvent";
    public const string FirstOccurrence = "firstOccurrence";
    public const string CustomScript = "customScript";
}
