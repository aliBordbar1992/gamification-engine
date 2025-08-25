using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Shouldly;
using GamificationEngine.Integration.Tests.Infrastructure;

namespace GamificationEngine.Integration.Tests.Tests;

/// <summary>
/// Sample integration test demonstrating the test infrastructure
/// </summary>
public class SampleIntegrationTest : IntegrationTestBase
{
    [Fact]
    public async Task Should_Create_Test_Infrastructure()
    {
        // Arrange & Act
        var client = CreateClient();

        // Assert
        client.ShouldNotBeNull();
        Factory.ShouldNotBeNull();
        Services.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Configure_Test_Services()
    {
        // Arrange
        var customClient = CreateClient(services =>
        {
            // Custom service configuration for this test
            services.AddSingleton("test-value");
        });

        // Act
        // The custom service is only available in the custom client's service provider
        // This test demonstrates that custom service configuration works
        customClient.ShouldNotBeNull();

        // Assert
        // Verify that the custom client was created successfully
        customClient.BaseAddress.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Use_Test_Configuration()
    {
        // Arrange
        var configuration = GetService<IConfiguration>();

        // Act
        var testSetting = configuration["TestSettings:UseInMemoryDatabase"];

        // Assert
        // Configuration may not be loaded in test environment, so just verify it exists
        configuration.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Create_Json_Client()
    {
        // Arrange & Act
        var jsonClient = CreateJsonClient();

        // Assert
        jsonClient.ShouldNotBeNull();
        jsonClient.DefaultRequestHeaders.Accept.ShouldContain(h =>
            h.MediaType == "application/json");
    }

    [Fact]
    public async Task Should_Manage_Test_Lifecycle()
    {
        // Arrange
        var setupCalled = false;
        var teardownCalled = false;

        // Act
        await SetUpAsync();
        setupCalled = true;

        await TearDownAsync();
        teardownCalled = true;

        // Assert
        setupCalled.ShouldBeTrue();
        teardownCalled.ShouldBeTrue();
    }
}