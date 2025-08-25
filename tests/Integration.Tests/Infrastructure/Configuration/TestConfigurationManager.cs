using GamificationEngine.Integration.Tests.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GamificationEngine.Integration.Tests.Infrastructure.Configuration;

/// <summary>
/// Manages test-specific configuration including environment overrides, test settings, and configuration validation
/// </summary>
public class TestConfigurationManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TestConfigurationManager> _logger;
    private readonly Dictionary<string, string> _environmentOverrides;

    public TestConfigurationManager(IConfiguration? configuration = null, ILogger<TestConfigurationManager>? logger = null)
    {
        _configuration = configuration ?? CreateDefaultTestConfiguration();
        _logger = logger ?? NullLogger<TestConfigurationManager>.Instance;
        _environmentOverrides = new Dictionary<string, string>();

        LoadEnvironmentOverrides();
    }

    /// <summary>
    /// Gets the test configuration instance
    /// </summary>
    public IConfiguration Configuration => _configuration;

    /// <summary>
    /// Gets test-specific settings
    /// </summary>
    public TestSettings TestSettings => _configuration.GetSection("TestSettings").Get<TestSettings>() ?? new TestSettings();

    /// <summary>
    /// Gets database connection strings
    /// </summary>
    public DatabaseSettings DatabaseSettings => _configuration.GetSection("ConnectionStrings").Get<DatabaseSettings>() ?? new DatabaseSettings();

    /// <summary>
    /// Gets event queue configuration
    /// </summary>
    public EventQueueSettings EventQueueSettings => _configuration.GetSection("EventQueue").Get<EventQueueSettings>() ?? new EventQueueSettings();

    /// <summary>
    /// Gets event retention configuration
    /// </summary>
    public EventRetentionSettings EventRetentionSettings => _configuration.GetSection("EventRetention").Get<EventRetentionSettings>() ?? new EventRetentionSettings();

    /// <summary>
    /// Gets logging configuration
    /// </summary>
    public LoggingSettings LoggingSettings => _configuration.GetSection("Logging").Get<LoggingSettings>() ?? new LoggingSettings();

    /// <summary>
    /// Overrides a configuration value for testing
    /// </summary>
    public void OverrideConfiguration(string key, string value)
    {
        _environmentOverrides[key] = value;
        _logger.LogInformation("Configuration override set: {Key} = {Value}", key, value);
    }

    /// <summary>
    /// Gets a configuration value with support for overrides
    /// </summary>
    public string? GetValue(string key)
    {
        if (_environmentOverrides.TryGetValue(key, out var overrideValue))
        {
            return overrideValue;
        }
        return _configuration[key];
    }

    /// <summary>
    /// Gets a configuration section
    /// </summary>
    public IConfigurationSection GetSection(string key)
    {
        return _configuration.GetSection(key);
    }

    /// <summary>
    /// Validates that required test configuration is present
    /// </summary>
    public bool ValidateConfiguration(out List<string> errors)
    {
        errors = new List<string>();

        // Validate test settings
        if (string.IsNullOrEmpty(TestSettings.DatabaseProvider))
        {
            errors.Add("TestSettings:DatabaseProvider is required");
        }

        // Validate database connection strings based on provider
        if (TestSettings.DatabaseProvider?.ToLowerInvariant() == "postgresql")
        {
            if (string.IsNullOrEmpty(DatabaseSettings.TestPostgreSql))
            {
                errors.Add("ConnectionStrings:TestPostgreSql is required when using PostgreSQL provider");
            }
        }

        // Validate event queue settings
        if (EventQueueSettings.ProcessingInterval <= TimeSpan.Zero)
        {
            errors.Add("EventQueue:ProcessingInterval must be greater than zero");
        }

        if (EventQueueSettings.MaxConcurrentProcessing <= 0)
        {
            errors.Add("EventQueue:MaxConcurrentProcessing must be greater than zero");
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Creates a configuration builder with test-specific settings
    /// </summary>
    public IConfigurationBuilder CreateConfigurationBuilder()
    {
        var builder = new ConfigurationBuilder();

        // Add base configuration files
        var testConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Testing.json");
        if (File.Exists(testConfigPath))
        {
            builder.AddJsonFile(testConfigPath, optional: false);
        }

        // Add environment-specific configuration
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Testing";
        var envConfigPath = Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{environment}.json");
        if (File.Exists(envConfigPath))
        {
            builder.AddJsonFile(envConfigPath, optional: true);
        }

        // Add environment variables with TEST_ prefix
        builder.AddEnvironmentVariables("TEST_");

        // Add command line arguments
        builder.AddCommandLine(Environment.GetCommandLineArgs());

        return builder;
    }

    /// <summary>
    /// Loads environment-specific overrides
    /// </summary>
    private void LoadEnvironmentOverrides()
    {
        var testPrefix = "TEST_";
        var environmentVariables = Environment.GetEnvironmentVariables();

        foreach (var key in environmentVariables.Keys)
        {
            var keyStr = key.ToString();
            if (keyStr?.StartsWith(testPrefix, StringComparison.OrdinalIgnoreCase) == true)
            {
                var configKey = keyStr[testPrefix.Length..].Replace("_", ":");
                var value = environmentVariables[key]?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    _environmentOverrides[configKey] = value;
                    _logger.LogDebug("Loaded environment override: {Key} = {Value}", configKey, value);
                }
            }
        }
    }

    /// <summary>
    /// Creates default test configuration when none is provided
    /// </summary>
    private static IConfiguration CreateDefaultTestConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["TestSettings:UseInMemoryDatabase"] = "true",
                ["TestSettings:DatabaseProvider"] = "InMemory",
                ["TestSettings:EnableDetailedLogging"] = "true",
                ["TestSettings:TestTimeoutSeconds"] = "30",
                ["TestSettings:EnablePerformanceTesting"] = "true",
                ["TestSettings:EnableLoadTesting"] = "true",
                ["TestSettings:EnableStressTesting"] = "true",
                ["TestSettings:EnableBaselineTesting"] = "true",
                ["TestSettings:EnableTestExecutionMonitoring"] = "true",
                ["EventQueue:ProcessingInterval"] = "00:00:01",
                ["EventQueue:MaxConcurrentProcessing"] = "2",
                ["EventRetention:RetentionDays"] = "30",
                ["EventRetention:BatchSize"] = "100"
            })
            .Build();
    }
}