using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace GamificationEngine.Integration.Tests.Infrastructure;

/// <summary>
/// Base class for all integration tests providing common setup and teardown functionality
/// </summary>
public abstract class IntegrationTestBase : IAsyncDisposable
{
    protected WebApplicationFactory<Program> Factory { get; private set; }
    protected IServiceScope ServiceScope { get; private set; }
    protected IServiceProvider Services { get; private set; }

    protected IntegrationTestBase()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
{
    // Add test-specific configuration if available
    var testConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "tests", "Integration.Tests", "appsettings.Testing.json");
    config.AddJsonFile(testConfigPath, optional: true);

    // Override with environment-specific settings
    config.AddEnvironmentVariables("TEST_");
});

                builder.ConfigureServices(services =>
                {
                    // Configure test-specific services here
                    ConfigureTestServices(services);
                });
            });

        ServiceScope = Factory.Services.CreateScope();
        Services = ServiceScope.ServiceProvider;
    }

    /// <summary>
    /// Override this method to configure test-specific services
    /// </summary>
    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Default implementation - override in derived classes
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

        await ValueTask.CompletedTask;
    }
}