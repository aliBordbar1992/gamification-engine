namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Database connection string settings
/// </summary>
public class DatabaseSettings
{
    public string DefaultConnection { get; set; } = string.Empty;
    public string TestPostgreSql { get; set; } = string.Empty;
}