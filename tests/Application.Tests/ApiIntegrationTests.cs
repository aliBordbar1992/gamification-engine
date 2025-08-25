using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using GamificationEngine.Api;
using GamificationEngine.Api.Controllers;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Shared;
using Moq;
using Shouldly;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GamificationEngine.Application.Tests;

/// <summary>
/// Integration tests for the API layer using TestServer
/// </summary>
public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task IngestEvent_ValidEvent_ShouldReturnCreated()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "test-event-1",
            EventType = "USER_COMMENTED",
            UserId = "test-user-123",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "commentId", "comment-456" },
                { "postId", "post-789" },
                { "text", "Great post!" }
            }
        };

        var expectedEvent = new Event(
            request.EventId,
            request.EventType,
            request.UserId,
            request.OccurredAt.Value,
            request.Attributes
        );

        _factory.MockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/events", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location!.ToString().ShouldContain("/api/events/test-event-1");

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        createdEvent.ShouldNotBeNull();
        createdEvent.EventId.ShouldBe("test-event-1");
        createdEvent.EventType.ShouldBe("USER_COMMENTED");
        createdEvent.UserId.ShouldBe("test-user-123");
    }

    [Fact]
    public async Task IngestEvent_WithoutEventId_ShouldGenerateEventId()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventType = "PROFILE_COMPLETED",
            UserId = "test-user-456",
            Attributes = new Dictionary<string, object>
            {
                { "completenessPercent", 95.0 }
            }
        };

        var generatedEventId = Guid.NewGuid().ToString();
        var expectedEvent = new Event(
            generatedEventId,
            request.EventType,
            request.UserId,
            DateTimeOffset.UtcNow,
            request.Attributes
        );

        _factory.MockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/events", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        createdEvent.ShouldNotBeNull();
        createdEvent.EventId.ShouldNotBeNullOrEmpty();
        createdEvent.EventType.ShouldBe("PROFILE_COMPLETED");
        createdEvent.UserId.ShouldBe("test-user-456");
    }

    [Fact]
    public async Task IngestEvent_WithoutOccurredAt_ShouldUseCurrentTime()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventType = "USER_PURCHASED_PRODUCT",
            UserId = "test-user-789",
            Attributes = new Dictionary<string, object>
            {
                { "productId", "prod-123" },
                { "amount", 29.99m }
            }
        };

        var currentTime = DateTimeOffset.UtcNow;
        var expectedEvent = new Event(
            Guid.NewGuid().ToString(),
            request.EventType,
            request.UserId,
            currentTime,
            request.Attributes
        );

        _factory.MockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/events", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        createdEvent.ShouldNotBeNull();
        createdEvent.OccurredAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddMinutes(-1));
        createdEvent.OccurredAt.ShouldBeLessThan(DateTimeOffset.UtcNow.AddMinutes(1));
    }

    [Fact]
    public async Task IngestEvent_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange - Missing required fields
        var request = new { EventType = "" }; // Missing UserId
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/events", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserEvents_ValidUserId_ShouldReturnEvents()
    {
        // Arrange
        var userId = "test-user-events";
        var mockEvents = new List<Event>
        {
            new Event("event-1", "USER_COMMENTED", userId, DateTimeOffset.UtcNow.AddMinutes(-10)),
            new Event("event-2", "PROFILE_COMPLETED", userId, DateTimeOffset.UtcNow.AddMinutes(-5))
        };

        _factory.MockEventIngestionService.Setup(s => s.GetUserEventsAsync(userId, 100, 0))
            .ReturnsAsync(Result<IEnumerable<Event>, DomainError>.Success(mockEvents));

        // Act
        var response = await _client.GetAsync($"/api/events/user/{userId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var events = JsonSerializer.Deserialize<IEnumerable<EventDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        events.ShouldNotBeNull();
        events.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetUserEvents_WithPagination_ShouldRespectLimitAndOffset()
    {
        // Arrange
        var userId = "test-user-pagination";
        var limit = 5;
        var offset = 2;
        var mockEvents = new List<Event>
        {
            new Event("event-3", "USER_COMMENTED", userId, DateTimeOffset.UtcNow.AddMinutes(-10)),
            new Event("event-4", "PROFILE_COMPLETED", userId, DateTimeOffset.UtcNow.AddMinutes(-5))
        };

        _factory.MockEventIngestionService.Setup(s => s.GetUserEventsAsync(userId, limit, offset))
            .ReturnsAsync(Result<IEnumerable<Event>, DomainError>.Success(mockEvents));

        // Act
        var response = await _client.GetAsync($"/api/events/user/{userId}?limit={limit}&offset={offset}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        _factory.MockEventIngestionService.Verify(s => s.GetUserEventsAsync(userId, limit, offset), Times.Once);
    }

    [Fact]
    public async Task GetUserEvents_InvalidUserId_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = "invalid-user-id";
        var mockError = new InvalidUserIdError("User ID is invalid");

        _factory.MockEventIngestionService.Setup(s => s.GetUserEventsAsync(userId, 100, 0))
            .ReturnsAsync(Result<IEnumerable<Event>, DomainError>.Failure(mockError));

        // Act
        var response = await _client.GetAsync($"/api/events/user/{userId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.ShouldNotBeNull();
        errorResponse.Error.ShouldBe("User ID is invalid");
    }

    [Fact]
    public async Task GetEventsByType_ValidEventType_ShouldReturnEvents()
    {
        // Arrange
        var eventType = "USER_COMMENTED";
        var mockEvents = new List<Event>
        {
            new Event("event-1", eventType, "user-1", DateTimeOffset.UtcNow.AddMinutes(-10)),
            new Event("event-2", eventType, "user-2", DateTimeOffset.UtcNow.AddMinutes(-5))
        };

        _factory.MockEventIngestionService.Setup(s => s.GetEventsByTypeAsync(eventType, 100, 0))
            .ReturnsAsync(Result<IEnumerable<Event>, DomainError>.Success(mockEvents));

        // Act
        var response = await _client.GetAsync($"/api/events/type/{eventType}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var events = JsonSerializer.Deserialize<IEnumerable<EventDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        events.ShouldNotBeNull();
        events.Count().ShouldBe(2);
        events.All(e => e.EventType == eventType).ShouldBeTrue();
    }

    [Fact]
    public async Task GetEventsByType_WithPagination_ShouldRespectLimitAndOffset()
    {
        // Arrange
        var eventType = "PROFILE_COMPLETED";
        var limit = 3;
        var offset = 1;
        var mockEvents = new List<Event>
        {
            new Event("event-2", eventType, "user-2", DateTimeOffset.UtcNow.AddMinutes(-5))
        };

        _factory.MockEventIngestionService.Setup(s => s.GetEventsByTypeAsync(eventType, limit, offset))
            .ReturnsAsync(Result<IEnumerable<Event>, DomainError>.Success(mockEvents));

        // Act
        var response = await _client.GetAsync($"/api/events/type/{eventType}?limit={limit}&offset={offset}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        _factory.MockEventIngestionService.Verify(s => s.GetEventsByTypeAsync(eventType, limit, offset), Times.Once);
    }

    [Fact]
    public async Task GetEventsByType_InvalidEventType_ShouldReturnBadRequest()
    {
        // Arrange
        var eventType = "invalid-event-type";
        var mockError = new InvalidEventTypeError("Event type is invalid");

        _factory.MockEventIngestionService.Setup(s => s.GetEventsByTypeAsync(eventType, 100, 0))
            .ReturnsAsync(Result<IEnumerable<Event>, DomainError>.Failure(mockError));

        // Act
        var response = await _client.GetAsync($"/api/events/type/{eventType}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.ShouldNotBeNull();
        errorResponse.Error.ShouldBe("Event type is invalid");
    }

    [Fact]
    public async Task GetEvent_NotImplemented_ShouldReturnNotImplemented()
    {
        // Act
        var response = await _client.GetAsync("/api/events/non-existent-id");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotImplemented);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<NotImplementedResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.ShouldNotBeNull();
        errorResponse.Message.ShouldBe("Get event by ID not yet implemented");
    }

    [Fact]
    public async Task IngestEvent_ServiceFailure_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventType = "TEST_EVENT",
            UserId = "test-user"
        };

        var mockError = new EventStorageError("Failed to enqueue event: Test error");
        _factory.MockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Failure(mockError));

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/events", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.ShouldNotBeNull();
        errorResponse.Error.ShouldBe("Failed to enqueue event: Test error");
    }

    [Fact]
    public async Task IngestEvent_ComplexAttributes_ShouldHandleCorrectly()
    {
        // Arrange
        var request = new IngestEventRequest
        {
            EventId = "complex-event-1",
            EventType = "USER_PURCHASED_PRODUCT",
            UserId = "complex-user-123",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "productId", "prod-456" },
                { "amount", 99.99m },
                { "currency", "USD" },
                { "tags", new[] { "electronics", "gaming" } },
                { "metadata", new Dictionary<string, object>
                    {
                        { "source", "recommendation_engine" },
                        { "confidence", 0.95 }
                    }
                },
                { "isFirstPurchase", true },
                { "discountApplied", 15.0 }
            }
        };

        var expectedEvent = new Event(
            request.EventId,
            request.EventType,
            request.UserId,
            request.OccurredAt.Value,
            request.Attributes
        );

        _factory.MockEventIngestionService.Setup(s => s.IngestEventAsync(It.IsAny<Event>()))
            .ReturnsAsync(Result<Event, DomainError>.Success(expectedEvent));

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/events", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        createdEvent.ShouldNotBeNull();
        createdEvent.Attributes.ShouldNotBeNull();
        createdEvent.Attributes["productId"].ToString().ShouldBe("prod-456");
        ((JsonElement)createdEvent.Attributes["amount"]).GetDouble().ShouldBe(99.99, 0.001);
        var tags = ((JsonElement)createdEvent.Attributes["tags"]).EnumerateArray().Select(e => e.GetString()).ToArray();
        tags.ShouldBe(new[] { "electronics", "gaming" });
        ((JsonElement)createdEvent.Attributes["isFirstPurchase"]).GetBoolean().ShouldBe(true);
    }
}

/// <summary>
/// Custom web application factory for integration testing
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IEventIngestionService> MockEventIngestionService { get; }

    public CustomWebApplicationFactory()
    {
        MockEventIngestionService = new Mock<IEventIngestionService>();
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real service registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IEventIngestionService));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove the real repository and queue registrations
            var repoDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(Domain.Repositories.IEventRepository));
            if (repoDescriptor != null)
            {
                services.Remove(repoDescriptor);
            }

            var queueDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(Application.Abstractions.IEventQueue));
            if (queueDescriptor != null)
            {
                services.Remove(queueDescriptor);
            }

            // Remove the background service that depends on IEventQueue
            var backgroundServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                     d.ImplementationType == typeof(GamificationEngine.Api.EventQueueBackgroundService));
            if (backgroundServiceDescriptor != null)
            {
                services.Remove(backgroundServiceDescriptor);
            }

            // Add our mocks
            services.AddSingleton(MockEventIngestionService.Object);
        });
    }
}

// Using IngestEventRequest from GamificationEngine.Api.Controllers namespace

/// <summary>
/// Response models for testing
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}

public class NotImplementedResponse
{
    public string Message { get; set; } = string.Empty;
}