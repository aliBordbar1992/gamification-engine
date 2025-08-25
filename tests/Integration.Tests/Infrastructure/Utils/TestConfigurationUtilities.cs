using GamificationEngine.Integration.Tests.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Utils;

/// <summary>
/// Utility methods for test configuration management
/// </summary>
public static class TestConfigurationUtilities
{
    /// <summary>
    /// Sets up test environment variables
    /// </summary>
    public static void SetupTestEnvironment()
    {
        // Set test environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        // Set test-specific configuration
        Environment.SetEnvironmentVariable("TEST_DATABASE_PROVIDER", "InMemory");
        Environment.SetEnvironmentVariable("TEST_ENABLE_DETAILED_LOGGING", "true");
        Environment.SetEnvironmentVariable("TEST_TIMEOUT_SECONDS", "30");

        // Set test database connection strings
        Environment.SetEnvironmentVariable("TEST_CONNECTIONSTRINGS__DEFAULT_CONNECTION", "Data Source=:memory:;Cache=Shared");
        Environment.SetEnvironmentVariable("TEST_CONNECTIONSTRINGS__TEST_POSTGRESQL", "Host=localhost;Port=5432;Database=gamification_test;Username=test_user;Password=test_password");

        // Set event queue settings
        Environment.SetEnvironmentVariable("TEST_EVENTQUEUE__PROCESSING_INTERVAL", "00:00:01");
        Environment.SetEnvironmentVariable("TEST_EVENTQUEUE__MAX_CONCURRENT_PROCESSING", "2");

        // Set event retention settings
        Environment.SetEnvironmentVariable("TEST_EVENTRETENTION__RETENTION_DAYS", "30");
        Environment.SetEnvironmentVariable("TEST_EVENTRETENTION__BATCH_SIZE", "100");

        // Set logging settings
        Environment.SetEnvironmentVariable("TEST_LOGGING__DEFAULT_LEVEL", "Information");
        Environment.SetEnvironmentVariable("TEST_LOGGING__MICROSOFT_ASPNETCORE_LEVEL", "Warning");
        Environment.SetEnvironmentVariable("TEST_LOGGING__MICROSOFT_ENTITYFRAMEWORK_LEVEL", "Information");
    }

    /// <summary>
    /// Cleans up test environment variables
    /// </summary>
    public static void CleanupTestEnvironment()
    {
        var testPrefix = "TEST_";
        var environmentVariables = Environment.GetEnvironmentVariables();

        foreach (var key in environmentVariables.Keys)
        {
            var keyStr = key.ToString();
            if (keyStr?.StartsWith(testPrefix, StringComparison.OrdinalIgnoreCase) == true)
            {
                Environment.SetEnvironmentVariable(keyStr, null);
            }
        }

        // Reset ASPNETCORE_ENVIRONMENT if it was set to Testing
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    /// <summary>
    /// Creates a test configuration builder with common test settings
    /// </summary>
    public static IConfigurationBuilder CreateTestConfigurationBuilder(
        string? environment = null,
        bool includeEnvironmentVariables = true,
        bool includeCommandLine = true)
    {
        var builder = new ConfigurationBuilder();

        // Add base test configuration
        var testConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Testing.json");
        if (File.Exists(testConfigPath))
        {
            builder.AddJsonFile(testConfigPath, optional: false);
        }

        // Add environment-specific configuration
        environment ??= Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Testing";
        var envConfigPath = Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{environment}.json");
        if (File.Exists(envConfigPath))
        {
            builder.AddJsonFile(envConfigPath, optional: true);
        }

        // Add environment variables with TEST_ prefix
        if (includeEnvironmentVariables)
        {
            builder.AddEnvironmentVariables("TEST_");
        }

        // Add command line arguments
        if (includeCommandLine)
        {
            builder.AddCommandLine(Environment.GetCommandLineArgs());
        }

        return builder;
    }

    /// <summary>
    /// Creates a test configuration with specific overrides
    /// </summary>
    public static IConfiguration CreateTestConfiguration(Dictionary<string, string>? overrides = null)
    {
        var builder = CreateTestConfigurationBuilder();

        if (overrides != null)
        {
            builder.AddInMemoryCollection(overrides);
        }

        return builder.Build();
    }

    /// <summary>
    /// Configures test services with configuration and monitoring
    /// </summary>
    public static IServiceCollection ConfigureTestServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var testSettings = configuration.GetSection("TestSettings").Get<TestSettings>() ?? new TestSettings();
        var loggingSettings = configuration.GetSection("Logging").Get<LoggingSettings>() ?? new LoggingSettings();

        // Add test logging
        services.AddTestLogging(testSettings, loggingSettings);

        // Add test monitoring
        services.AddTestMonitoring(testSettings);

        // Add configuration
        services.AddSingleton(configuration);
        services.AddSingleton(testSettings);
        services.AddSingleton(loggingSettings);

        return services;
    }

    /// <summary>
    /// Creates a test host builder with configuration
    /// </summary>
    public static IHostBuilder CreateTestHostBuilder(
        IConfiguration? configuration = null,
        Action<IServiceCollection>? configureServices = null)
    {
        configuration ??= CreateTestConfiguration();

        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                // Clear existing configuration
                config.Sources.Clear();

                // Add our test configuration
                config.AddConfiguration(configuration);
            })
            .ConfigureServices((context, services) =>
            {
                // Configure test services
                services.ConfigureTestServices(context.Configuration);

                // Allow custom service configuration
                configureServices?.Invoke(services);
            });
    }

    /// <summary>
    /// Validates test configuration and returns any errors
    /// </summary>
    public static List<string> ValidateTestConfiguration(IConfiguration configuration)
    {
        var errors = new List<string>();

        // Only validate sections that are present and have values

        // Validate test settings
        var testSettings = configuration.GetSection("TestSettings").Get<TestSettings>();
        if (testSettings != null)
        {
            if (string.IsNullOrEmpty(testSettings.DatabaseProvider))
            {
                errors.Add("TestSettings:DatabaseProvider is required");
            }

            if (testSettings.TestTimeoutSeconds <= 0)
            {
                errors.Add("TestSettings:TestTimeoutSeconds must be greater than 0");
            }
        }

        // Validate database connection strings
        var databaseSettings = configuration.GetSection("ConnectionStrings").Get<DatabaseSettings>();
        if (databaseSettings != null)
        {
            if (testSettings?.DatabaseProvider?.ToLowerInvariant() == "postgresql")
            {
                if (string.IsNullOrEmpty(databaseSettings.TestPostgreSql))
                {
                    errors.Add("ConnectionStrings:TestPostgreSql is required when using PostgreSQL provider");
                }
            }
        }

        // Validate event queue settings
        var eventQueueSettings = configuration.GetSection("EventQueue").Get<EventQueueSettings>();
        if (eventQueueSettings != null)
        {
            if (eventQueueSettings.ProcessingInterval <= TimeSpan.Zero)
            {
                errors.Add("EventQueue:ProcessingInterval must be greater than zero");
            }

            if (eventQueueSettings.MaxConcurrentProcessing <= 0)
            {
                errors.Add("EventQueue:MaxConcurrentProcessing must be greater than zero");
            }
        }

        return errors;
    }

    /// <summary>
    /// Creates a test configuration for specific test scenarios
    /// </summary>
    public static IConfiguration CreateScenarioConfiguration(string scenario, Dictionary<string, string>? overrides = null)
    {
        var baseConfig = CreateTestConfigurationBuilder().Build();
        var scenarioOverrides = new Dictionary<string, string>();

        // Add scenario-specific overrides
        switch (scenario.ToLowerInvariant())
        {
            case "performance":
                scenarioOverrides["TestSettings:EnableDetailedLogging"] = "false";
                scenarioOverrides["TestSettings:EnableParallelExecution"] = "true";
                scenarioOverrides["TestSettings:MaxParallelTests"] = "4";
                scenarioOverrides["EventQueue:ProcessingInterval"] = "00:00:00.100";
                scenarioOverrides["EventQueue:MaxConcurrentProcessing"] = "4";
                break;

            case "integration":
                scenarioOverrides["TestSettings:DatabaseProvider"] = "PostgreSQL";
                scenarioOverrides["TestSettings:EnableDetailedLogging"] = "true";
                scenarioOverrides["EventQueue:ProcessingInterval"] = "00:00:05";
                break;

            case "unit":
                scenarioOverrides["TestSettings:DatabaseProvider"] = "InMemory";
                scenarioOverrides["TestSettings:EnableDetailedLogging"] = "false";
                scenarioOverrides["EventQueue:ProcessingInterval"] = "00:00:00.001";
                break;

            case "debug":
                scenarioOverrides["TestSettings:EnableDetailedLogging"] = "true";
                scenarioOverrides["Logging:DefaultLevel"] = "Debug";
                scenarioOverrides["Logging:MicrosoftEntityFrameworkLevel"] = "Debug";
                break;
        }

        // Add custom overrides
        if (overrides != null)
        {
            foreach (var kvp in overrides)
            {
                scenarioOverrides[kvp.Key] = kvp.Value;
            }
        }

        // Create configuration with all overrides
        var builder = new ConfigurationBuilder();
        builder.AddConfiguration(baseConfig);
        builder.AddInMemoryCollection(scenarioOverrides);

        return builder.Build();
    }

    /// <summary>
    /// Gets test configuration value with fallback
    /// </summary>
    public static T GetTestConfigurationValue<T>(
        IConfiguration configuration,
        string key,
        T defaultValue)
    {
        var value = configuration[key];
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Creates a test configuration file for a specific environment
    /// </summary>
    public static void CreateTestConfigurationFile(string environment, Dictionary<string, object> settings)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{environment}.json");
        var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(configPath, json);
    }
}