namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Configuration options for test database
/// </summary>
public class TestDatabaseOptions
{
    /// <summary>
    /// Type of database to use for testing
    /// </summary>
    public string DatabaseType { get; set; } = "InMemory";

    /// <summary>
    /// Connection string for the test database
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Whether to enable database logging
    /// </summary>
    public bool EnableLogging { get; set; } = false;

    /// <summary>
    /// Whether to enable sensitive data logging
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;
}