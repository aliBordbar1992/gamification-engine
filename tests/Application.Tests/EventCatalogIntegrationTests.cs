using GamificationEngine.Application.Services;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace GamificationEngine.Application.Tests;

/// <summary>
/// Integration tests for event catalog functionality
/// </summary>
public class EventCatalogIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EventCatalogIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetEventCatalog_ShouldReturnEventDefinitions()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var eventDefinitionRepository = scope.ServiceProvider.GetRequiredService<IEventDefinitionRepository>();

        // Seed some test data
        var eventDefinition1 = new EventDefinition("USER_COMMENTED", "User commented on a post",
            new Dictionary<string, string> { { "commentId", "string" }, { "postId", "string" } });
        var eventDefinition2 = new EventDefinition("USER_PURCHASED_PRODUCT", "User purchased a product",
            new Dictionary<string, string> { { "productId", "string" }, { "amount", "number" } });

        await eventDefinitionRepository.AddAsync(eventDefinition1);
        await eventDefinitionRepository.AddAsync(eventDefinition2);

        // Act
        var response = await _client.GetAsync("/api/events/catalog");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var eventDefinitions = JsonSerializer.Deserialize<EventDefinitionDto[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventDefinitions.ShouldNotBeNull();
        eventDefinitions.Length.ShouldBeGreaterThanOrEqualTo(2);

        var userCommentedEvent = eventDefinitions.FirstOrDefault(ed => ed.Id == "USER_COMMENTED");
        userCommentedEvent.ShouldNotBeNull();
        userCommentedEvent.Description.ShouldBe("User commented on a post");
        userCommentedEvent.PayloadSchema.ShouldNotBeNull();
        userCommentedEvent.PayloadSchema.ShouldContainKey("commentId");
        userCommentedEvent.PayloadSchema.ShouldContainKey("postId");
    }

    [Fact]
    public async Task GetEventCatalog_WithNoEventDefinitions_ShouldReturnEmptyArray()
    {
        // Act
        var response = await _client.GetAsync("/api/events/catalog");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var eventDefinitions = JsonSerializer.Deserialize<EventDefinitionDto[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventDefinitions.ShouldNotBeNull();
        eventDefinitions.Length.ShouldBe(0);
    }

    [Fact]
    public async Task IngestEvent_WithValidEventType_ShouldSucceed()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var eventDefinitionRepository = scope.ServiceProvider.GetRequiredService<IEventDefinitionRepository>();

        // Seed event definition
        var eventDefinition = new EventDefinition("USER_COMMENTED", "User commented on a post",
            new Dictionary<string, string> { { "commentId", "string" }, { "postId", "string" } });
        await eventDefinitionRepository.AddAsync(eventDefinition);

        var eventData = new
        {
            eventType = "USER_COMMENTED",
            userId = "user123",
            attributes = new
            {
                commentId = "comment456",
                postId = "post789"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", eventData);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task IngestEvent_WithUnknownEventType_ShouldReturnBadRequest()
    {
        // Arrange
        var eventData = new
        {
            eventType = "UNKNOWN_EVENT",
            userId = "user123",
            attributes = new
            {
                someField = "someValue"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", eventData);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task IngestEvent_WithInvalidPayload_ShouldReturnBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var eventDefinitionRepository = scope.ServiceProvider.GetRequiredService<IEventDefinitionRepository>();

        // Seed event definition
        var eventDefinition = new EventDefinition("USER_COMMENTED", "User commented on a post",
            new Dictionary<string, string> { { "commentId", "string" }, { "postId", "string" } });
        await eventDefinitionRepository.AddAsync(eventDefinition);

        var eventData = new
        {
            eventType = "USER_COMMENTED",
            userId = "user123",
            attributes = new
            {
                commentId = "comment456"
                // Missing required postId field
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", eventData);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}

/// <summary>
/// DTO for event definition in tests
/// </summary>
public class EventDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> PayloadSchema { get; set; } = new();
}
