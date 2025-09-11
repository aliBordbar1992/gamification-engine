namespace GamificationEngine.Domain.Rules;

/// <summary>
/// Represents a spending action defined in a rule
/// </summary>
public sealed class RuleSpending
{
    // EF Core requires a parameterless constructor
    private RuleSpending() { }

    public RuleSpending(string category, RuleSpendingType type, Dictionary<string, string> attributes)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category cannot be empty", nameof(category));
        if (attributes == null) throw new ArgumentNullException(nameof(attributes));

        Category = category;
        Type = type;
        Attributes = attributes;
    }

    public string Category { get; private set; } = string.Empty;
    public RuleSpendingType Type { get; private set; }
    public Dictionary<string, string> Attributes { get; private set; } = new();

    /// <summary>
    /// Validates the spending configuration
    /// </summary>
    /// <returns>True if the spending is valid</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Category)) return false;
        if (Attributes == null) return false;

        // Validate required attributes based on type
        return Type switch
        {
            RuleSpendingType.Transaction => Attributes.ContainsKey("amount"),
            RuleSpendingType.Transfer => Attributes.ContainsKey("source") &&
                                       Attributes.ContainsKey("destination") &&
                                       Attributes.ContainsKey("amount"),
            _ => false
        };
    }

    /// <summary>
    /// Gets the amount attribute value
    /// </summary>
    /// <returns>The amount as a string (attribute name or literal value)</returns>
    public string GetAmountAttribute()
    {
        return Attributes.TryGetValue("amount", out var amount) ? amount : "0";
    }

    /// <summary>
    /// Gets the source user attribute for transfers
    /// </summary>
    /// <returns>The source attribute name</returns>
    public string GetSourceAttribute()
    {
        return Attributes.TryGetValue("source", out var source) ? source : string.Empty;
    }

    /// <summary>
    /// Gets the destination user attribute for transfers
    /// </summary>
    /// <returns>The destination attribute name</returns>
    public string GetDestinationAttribute()
    {
        return Attributes.TryGetValue("destination", out var destination) ? destination : string.Empty;
    }
}

/// <summary>
/// Types of rule spendings
/// </summary>
public enum RuleSpendingType
{
    /// <summary>
    /// User made a transaction (spent points)
    /// </summary>
    Transaction = 0,

    /// <summary>
    /// User transferred points to another user
    /// </summary>
    Transfer = 1
}
