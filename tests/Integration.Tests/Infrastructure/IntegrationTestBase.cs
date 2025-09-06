using GamificationEngine.Infrastructure.Storage.EntityFramework;
using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Configuration;
using GamificationEngine.Integration.Tests.Infrastructure.Http;
using GamificationEngine.Integration.Tests.Infrastructure.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace GamificationEngine.Integration.Tests.Infrastructure;

/// <summary>
/// Base class for all integration tests providing common setup and teardown functionality
/// </summary>
[Trait("Category", "Integration")]
public abstract class IntegrationTestBase : IAsyncDisposable
{
    protected WebApplicationFactory<Program> Factory { get; private set; }
    protected IServiceScope ServiceScope { get; private set; }
    protected IServiceProvider Services { get; private set; }
    protected TestConfigurationManager ConfigurationManager { get; private set; }
    protected ITestMetricsCollector? MetricsCollector { get; private set; }
    protected ITestPerformanceMonitor? PerformanceMonitor { get; private set; }

    protected IntegrationTestBase()
    {
        // Set up test environment
        TestConfigurationUtilities.SetupTestEnvironment();

        // Create configuration manager
        ConfigurationManager = new TestConfigurationManager();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Clear existing configuration sources
                    config.Sources.Clear();

                    // Add test configuration
                    config.AddConfiguration(ConfigurationManager.Configuration);

                    // Add environment-specific overrides
                    config.AddEnvironmentVariables("TEST_");
                });

                builder.ConfigureServices(services =>
                {
                    // Configure test-specific services
                    services.ConfigureTestServices(ConfigurationManager.Configuration);

                    // Allow derived classes to configure additional services
                    ConfigureTestServices(services);
                });
            });

        ServiceScope = Factory.Services.CreateScope();
        Services = ServiceScope.ServiceProvider;

        // Get monitoring services if available (these are optional and may not be configured)
        try
        {
            MetricsCollector = Services.GetService<ITestMetricsCollector>();
            PerformanceMonitor = Services.GetService<ITestPerformanceMonitor>();
        }
        catch
        {
            // Monitoring services are optional, ignore if not configured
            MetricsCollector = null;
            PerformanceMonitor = null;
        }
    }

    /// <summary>
    /// Override this method to configure test-specific services
    /// </summary>
    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        return;
    }

    /// <summary>
    /// Gets a service from the test service provider
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a scoped service from the test service scope
    /// </summary>
    protected T GetScopedService<T>() where T : notnull
    {
        return ServiceScope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Creates a new HTTP client for testing
    /// </summary>
    protected HttpClient CreateClient()
    {
        return TestHttpClientFactory.CreateClient(Factory);
    }

    /// <summary>
    /// Creates a new HTTP client with custom service configuration
    /// </summary>
    protected HttpClient CreateClient(Action<IServiceCollection> configureServices)
    {
        return TestHttpClientFactory.CreateClient(Factory, configureServices);
    }

    /// <summary>
    /// Creates a new HTTP client configured for JSON requests
    /// </summary>
    protected HttpClient CreateJsonClient()
    {
        return TestHttpClientFactory.CreateJsonClient(Factory);
    }

    /// <summary>
    /// Performs test setup before each test method
    /// </summary>
    protected virtual async Task SetUpAsync()
    {
        // Override in derived classes for test-specific setup
        await Task.CompletedTask;
    }

    /// <summary>
    /// Performs test cleanup after each test method
    /// </summary>
    protected virtual async Task TearDownAsync()
    {
        // Override in derived classes for test-specific cleanup
        await Task.CompletedTask;
    }

    /// <summary>
    /// Disposes of test resources
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await TearDownAsync();

        ServiceScope?.Dispose();
        Factory?.Dispose();

        // Clean up test environment
        TestConfigurationUtilities.CleanupTestEnvironment();

        await ValueTask.CompletedTask;
    }
}