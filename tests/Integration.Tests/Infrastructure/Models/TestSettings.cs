namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Test-specific configuration settings
/// </summary>
public class TestSettings
{
    public bool UseInMemoryDatabase { get; set; } = true;
    public string DatabaseProvider { get; set; } = "InMemory";
    public bool EnableDetailedLogging { get; set; } = true;
    public int TestTimeoutSeconds { get; set; } = 30;
    public bool EnableParallelExecution { get; set; } = false;
    public int MaxParallelTests { get; set; } = 1;
    public bool EnablePerformanceTesting { get; set; } = true;
    public bool EnableLoadTesting { get; set; } = true;
    public bool EnableStressTesting { get; set; } = true;
    public bool EnableBaselineTesting { get; set; } = true;
    public bool EnableTestExecutionMonitoring { get; set; } = true;
}