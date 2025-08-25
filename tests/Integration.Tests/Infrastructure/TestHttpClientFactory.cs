using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GamificationEngine.Integration.Tests.Infrastructure;

/// <summary>
/// Factory for creating HTTP clients configured for integration testing
/// </summary>
public static class TestHttpClientFactory
{
    /// <summary>
    /// Creates an HTTP client with default configuration for testing
    /// </summary>
    public static HttpClient CreateClient(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    /// <summary>
    /// Creates an HTTP client with custom configuration
    /// </summary>
    public static HttpClient CreateClient(
        WebApplicationFactory<Program> factory,
        Action<IServiceCollection> configureServices)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(configureServices);
        }).CreateClient();
    }

    /// <summary>
    /// Creates an HTTP client with custom headers
    /// </summary>
    public static HttpClient CreateClientWithHeaders(
        WebApplicationFactory<Program> factory,
        Dictionary<string, string> headers)
    {
        var client = CreateClient(factory);

        foreach (var header in headers)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        return client;
    }

    /// <summary>
    /// Creates an HTTP client with authentication headers
    /// </summary>
    public static HttpClient CreateAuthenticatedClient(
        WebApplicationFactory<Program> factory,
        string apiKey = "test-api-key")
    {
        var client = CreateClient(factory);
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        return client;
    }

    /// <summary>
    /// Creates an HTTP client with content type headers
    /// </summary>
    public static HttpClient CreateJsonClient(WebApplicationFactory<Program> factory)
    {
        var client = CreateClient(factory);
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}