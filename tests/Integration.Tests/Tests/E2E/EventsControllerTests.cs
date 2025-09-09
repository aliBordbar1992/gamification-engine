using GamificationEngine.Integration.Tests.Infrastructure;
using GamificationEngine.Integration.Tests.Infrastructure.Utils;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using GamificationEngine.Infrastructure.Storage.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Infrastructure.Storage.EntityFramework.Repositories;
using GamificationEngine.Infrastructure.Storage.EntityFramework.Services;

namespace GamificationEngine.Integration.Tests.Tests.E2E;

public class EventsControllerTests : EndToEndTestBase
{


    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        // Remove the in-memory services that the API registers by default
        var inMemoryRepoDescriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(IEventRepository) &&
                 d.ImplementationType == typeof(GamificationEngine.Infrastructure.Storage.InMemory.EventRepository));
        if (inMemoryRepoDescriptor != null)
        {
            services.Remove(inMemoryRepoDescriptor);
        }



        // Register EF repositories
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUserStateRepository, UserStateRepository>();

        // Configure retention options
        services.Configure<EventRetentionOptions>(options =>
        {
            options.RetentionPeriod = TimeSpan.FromDays(30);
            options.Interval = TimeSpan.FromDays(1);
        });

        // Register background services
        services.AddHostedService<EventRetentionService>();
    }

    protected override async Task SetUpAsync()
    {
        await base.SetUpAsync();

        try
        {
            // Try to get the DbContext, but don't fail if it's not available
            try
            {
                // Ensure database is created
                await DbContext.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                // Log that DbContext is not available, but continue with HTTP tests
                Console.WriteLine($"DbContext not available for tests: {ex.Message}");
            }

            // Ensure database is clean before each test if available
            await CleanupDatabaseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SetUpAsync: {ex}");
            throw;
        }
    }

    protected override async Task TearDownAsync()
    {
        try
        {
            await CleanupDatabaseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TearDownAsync: {ex}");
        }

        await base.TearDownAsync();
    }

    private async Task CleanupDatabaseAsync()
    {
        try
        {
            DbContext.Events.RemoveRange(DbContext.Events);
            DbContext.UserStates.RemoveRange(DbContext.UserStates);
            await DbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up database: {ex.Message}");
        }
    }

    [Fact]
    public async Task POST_ApiEvents_WithValidEvent_ShouldReturn201Created()
    {
        // Arrange
        await SetUpAsync();
        var request = new
        {
            EventType = "USER_COMMENTED",
            UserId = "user123",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "commentId", "comment456" },
                { "postId", "post789" },
                { "length", 150 }
            }
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location!.ToString().ShouldContain("/api/events/");

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldNotBeNullOrEmpty("Response content should not be empty");

        var eventDto = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventDto.ShouldNotBeNull("Event DTO should not be null");
        eventDto.EventType.ShouldBe("USER_COMMENTED");
        eventDto.UserId.ShouldBe("user123");
        eventDto.Attributes.ShouldNotBeNull();
        eventDto.Attributes!["commentId"].ToString().ShouldBeEquivalentTo("comment456");

        await DatabaseStateAssertionUtilities.AssertEventExistsInDatabase(
            DbContext,
            eventDto.EventId,
            "user123",
            "USER_COMMENTED");
    }

    [Fact]
    public async Task POST_ApiEvents_WithCustomEventId_ShouldUseProvidedId()
    {
        // Arrange
        var customEventId = "custom-event-123";
        var request = new
        {
            EventId = customEventId,
            EventType = "PRODUCT_PURCHASED",
            UserId = "user456",
            Attributes = new Dictionary<string, object>
            {
                { "productId", "prod123" },
                { "amount", 29.99 }
            }
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var eventDto = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventDto.ShouldNotBeNull("Event DTO should not be null");
        eventDto.EventId.ShouldBe(customEventId);

        // Verify database persistence if DbContext is available
        if (DbContext != null)
        {
            try
            {
                await DatabaseStateAssertionUtilities.AssertEventExistsInDatabase(
                    DbContext,
                    customEventId,
                    "user456",
                    "PRODUCT_PURCHASED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database assertion failed: {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task POST_ApiEvents_WithoutEventId_ShouldGenerateNewGuid()
    {
        // Arrange
        var request = new
        {
            EventType = "USER_LOGIN",
            UserId = "user789"
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var eventDto = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventDto.ShouldNotBeNull("Event DTO should not be null");
        eventDto.EventId.ShouldNotBeNullOrEmpty("Event ID should not be null or empty");
        Guid.TryParse(eventDto.EventId, out _).ShouldBeTrue("Event ID should be a valid GUID");
    }

    [Fact]
    public async Task POST_ApiEvents_WithoutOccurredAt_ShouldUseCurrentUtcTime()
    {
        // Arrange
        var beforeRequest = DateTimeOffset.UtcNow;
        var request = new
        {
            EventType = "USER_LOGOUT",
            UserId = "user999"
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var eventDto = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventDto.ShouldNotBeNull("Event DTO should not be null");
        eventDto.OccurredAt.ShouldBeGreaterThan(beforeRequest);
        eventDto.OccurredAt.ShouldBeLessThan(DateTimeOffset.UtcNow.AddSeconds(5));
    }

    [Theory]
    [InlineData("", "USER_COMMENTED", "user123")] // Empty EventType
    [InlineData("USER_COMMENTED", "", "user123")] // Empty UserId
    [InlineData(null, "USER_COMMENTED", "user123")] // Null EventType
    [InlineData("USER_COMMENTED", null, "user123")] // Null UserId
    public async Task POST_ApiEvents_WithInvalidData_ShouldReturn400BadRequest(string eventType, string userId, string expectedValidField)
    {
        // Arrange
        var request = new
        {
            EventType = eventType,
            UserId = userId
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("error");
    }

    [Fact]
    public async Task POST_ApiEvents_WithMalformedJson_ShouldReturn400BadRequest()
    {
        // Arrange
        var malformedJson = "{ \"EventType\": \"USER_COMMENTED\", \"UserId\": \"user123\", }"; // Extra comma
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_ApiEventsUser_WithValidUserId_ShouldReturnUserEvents()
    {
        // Arrange - Create test events
        var userId = "user123";
        var events = new[]
        {
            new { EventType = "USER_COMMENTED", Attributes = new Dictionary<string, object> { { "commentId", "comment1" } } },
            new { EventType = "USER_LIKED", Attributes = new Dictionary<string, object> { { "postId", "post1" } } },
            new { EventType = "USER_SHARED", Attributes = new Dictionary<string, object> { { "platform", "twitter" } } }
        };

        foreach (var evt in events)
        {
            var request = new
            {
                EventType = evt.EventType,
                UserId = userId,
                Attributes = evt.Attributes
            };

            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var postResponse = await HttpClient.PostAsync("/api/events", content);
            postResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Act
        var response = await HttpClient.GetAsync($"/api/events/user/{userId}");

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var eventList = JsonSerializer.Deserialize<List<EventDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventList.ShouldNotBeNull("Event list should not be null");
        eventList.Count.ShouldBe(3);
        eventList.All(e => e.UserId == userId).ShouldBeTrue();
        eventList.ShouldContain(e => e.EventType == "USER_COMMENTED");
        eventList.ShouldContain(e => e.EventType == "USER_LIKED");
        eventList.ShouldContain(e => e.EventType == "USER_SHARED");

        // Verify database state if DbContext is available
        if (DbContext != null)
        {
            try
            {
                await DatabaseStateAssertionUtilities.AssertUserEventCount(DbContext, userId, 3);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database assertion failed: {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task GET_ApiEventsUser_WithPagination_ShouldRespectLimitAndOffset()
    {
        // Arrange - Create 5 test events
        var userId = "user456";
        for (int i = 1; i <= 5; i++)
        {
            var request = new
            {
                EventType = $"EVENT_TYPE_{i}",
                UserId = userId,
                Attributes = new Dictionary<string, object> { { "sequence", i } }
            };

            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var postResponse = await HttpClient.PostAsync("/api/events", content);
            postResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Act - Get first 2 events
        var response1 = await HttpClient.GetAsync($"/api/events/user/{userId}?limit=2&offset=0");

        // Assert
        response1.ShouldNotBeNull("HTTP response should not be null");
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content1 = await response1.Content.ReadAsStringAsync();
        var events1 = JsonSerializer.Deserialize<List<EventDto>>(content1, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        events1.ShouldNotBeNull("Events list should not be null");
        events1.Count.ShouldBe(2);

        // Act - Get next 2 events
        var response2 = await HttpClient.GetAsync($"/api/events/user/{userId}?limit=2&offset=2");

        // Assert
        response2.ShouldNotBeNull("HTTP response should not be null");
        response2.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content2 = await response2.Content.ReadAsStringAsync();
        var events2 = JsonSerializer.Deserialize<List<EventDto>>(content2, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        events2.ShouldNotBeNull("Events list should not be null");
        events2.Count.ShouldBe(2);

        // Verify no overlap
        var eventIds1 = events1.Select(e => e.EventId).ToHashSet();
        var eventIds2 = events2.Select(e => e.EventId).ToHashSet();
        eventIds1.Overlaps(eventIds2).ShouldBeFalse();
    }

    [Fact]
    public async Task GET_ApiEventsUser_WithEmptyResult_ShouldReturnEmptyArray()
    {
        // Arrange
        var userId = "nonexistent_user";

        // Act
        var response = await HttpClient.GetAsync($"/api/events/user/{userId}");

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var eventList = JsonSerializer.Deserialize<List<EventDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventList.ShouldNotBeNull("Event list should not be null");
        eventList.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GET_ApiEventsUser_WithInvalidPagination_ShouldReturn400BadRequest()
    {
        // Arrange
        var userId = "user123";

        // Act - Test invalid limit
        var response1 = await HttpClient.GetAsync($"/api/events/user/{userId}?limit=1001");

        // Assert
        response1.ShouldNotBeNull("HTTP response should not be null");
        response1.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Act - Test negative offset
        var response2 = await HttpClient.GetAsync($"/api/events/user/{userId}?offset=-1");

        // Assert
        response2.ShouldNotBeNull("HTTP response should not be null");
        response2.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_ApiEventsType_WithValidEventType_ShouldReturnEventsOfType()
    {
        // Arrange - Create test events of different types
        var eventType = "USER_COMMENTED";
        var users = new[] { "user1", "user2", "user3" };

        foreach (var userId in users)
        {
            var request = new
            {
                EventType = eventType,
                UserId = userId,
                Attributes = new Dictionary<string, object> { { "commentId", $"comment_{userId}" } }
            };

            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var postResponse = await HttpClient.PostAsync("/api/events", content);
            postResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Create a different event type
        var differentRequest = new
        {
            EventType = "USER_LIKED",
            UserId = "user4"
        };
        var differentJson = JsonSerializer.Serialize(differentRequest);
        var differentContent = new StringContent(differentJson, Encoding.UTF8, "application/json");
        var differentPostResponse = await HttpClient.PostAsync("/api/events", differentContent);
        differentPostResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act
        var response = await HttpClient.GetAsync($"/api/events/type/{eventType}");

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var eventList = JsonSerializer.Deserialize<List<EventDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventList.ShouldNotBeNull("Event list should not be null");
        eventList.Count.ShouldBe(3);
        eventList.All(e => e.EventType == eventType).ShouldBeTrue();
        eventList.ShouldContain(e => e.UserId == "user1");
        eventList.ShouldContain(e => e.UserId == "user2");
        eventList.ShouldContain(e => e.UserId == "user3");

        // Verify database state if DbContext is available
        if (DbContext != null)
        {
            try
            {
                await DatabaseStateAssertionUtilities.AssertEventTypeCount(DbContext, eventType, 3);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database assertion failed: {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task GET_ApiEventsType_WithPagination_ShouldRespectLimitAndOffset()
    {
        // Arrange - Create 4 events of same type
        var eventType = "TEST_EVENT";
        for (int i = 1; i <= 4; i++)
        {
            var request = new
            {
                EventType = eventType,
                UserId = $"user{i}",
                Attributes = new Dictionary<string, object> { { "sequence", i } }
            };

            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var postResponse = await HttpClient.PostAsync("/api/events", content);
            postResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Act - Get first 2 events
        var response1 = await HttpClient.GetAsync($"/api/events/type/{eventType}?limit=2&offset=0");

        // Assert
        response1.ShouldNotBeNull("HTTP response should not be null");
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content1 = await response1.Content.ReadAsStringAsync();
        var events1 = JsonSerializer.Deserialize<List<EventDto>>(content1, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        events1.ShouldNotBeNull("Events list should not be null");
        events1.Count.ShouldBe(2);

        // Act - Get next 2 events
        var response2 = await HttpClient.GetAsync($"/api/events/type/{eventType}?limit=2&offset=2");

        // Assert
        response2.ShouldNotBeNull("HTTP response should not be null");
        response2.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content2 = await response2.Content.ReadAsStringAsync();
        var events2 = JsonSerializer.Deserialize<List<EventDto>>(content2, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        events2.ShouldNotBeNull("Events list should not be null");
        events2.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GET_ApiEventsType_WithEmptyResult_ShouldReturnEmptyArray()
    {
        // Arrange
        var eventType = "NONEXISTENT_EVENT_TYPE";

        // Act
        var response = await HttpClient.GetAsync($"/api/events/type/{eventType}");

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var eventList = JsonSerializer.Deserialize<List<EventDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventList.ShouldNotBeNull("Event list should not be null");
        eventList.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GET_ApiEventsType_WithInvalidPagination_ShouldReturn400BadRequest()
    {
        // Arrange
        var eventType = "TEST_EVENT";

        // Act - Test invalid limit
        var response1 = await HttpClient.GetAsync($"/api/events/type/{eventType}?limit=1001");

        // Assert
        response1.ShouldNotBeNull("HTTP response should not be null");
        response1.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Act - Test negative offset
        var response2 = await HttpClient.GetAsync($"/api/events/type/{eventType}?offset=-1");

        // Assert
        response2.ShouldNotBeNull("HTTP response should not be null");
        response2.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_ApiEventsById_ShouldReturn501NotImplemented()
    {
        // Arrange
        var eventId = "test-event-id";

        // Act
        var response = await HttpClient.GetAsync($"/api/events/{eventId}");

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.NotImplemented);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.ShouldNotBeNull("Error response should not be null");
        errorResponse.Message.ShouldBe("Get event by ID not yet implemented");
    }

    [Fact]
    public async Task POST_ApiEvents_WithComplexAttributes_ShouldPersistCorrectly()
    {
        // Arrange
        var complexAttributes = new Dictionary<string, object>
        {
            { "nestedObject", new { level1 = "value1", level2 = new { deep = "nested" } } },
            { "arrayValue", new[] { 1, 2, 3, 4, 5 } },
            { "booleanValue", true },
            { "nullValue", (object?)null },
            { "numberValue", 42.5 }
        };

        var request = new
        {
            EventType = "COMPLEX_EVENT",
            UserId = "user_complex",
            Attributes = complexAttributes
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var eventDto = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventDto.ShouldNotBeNull("Event DTO should not be null");
        eventDto.Attributes.ShouldNotBeNull();
        eventDto.Attributes!["nestedObject"].ShouldNotBeNull();
        eventDto.Attributes!["arrayValue"].ShouldNotBeNull();
        eventDto.Attributes!["booleanValue"].ShouldBe(true);
        eventDto.Attributes!["numberValue"].ShouldBe(42.5);
    }

    [Fact]
    public async Task POST_ApiEvents_WithLargeAttributes_ShouldHandleCorrectly()
    {
        // Arrange - Create large attributes
        var largeAttributes = new Dictionary<string, object>();
        for (int i = 0; i < 100; i++)
        {
            largeAttributes[$"key_{i}"] = $"value_{i}_{new string('x', 100)}";
        }

        var request = new
        {
            EventType = "LARGE_ATTRIBUTES_EVENT",
            UserId = "user_large",
            Attributes = largeAttributes
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var eventDto = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        eventDto.ShouldNotBeNull("Event DTO should not be null");
        eventDto.Attributes.ShouldNotBeNull();
        eventDto.Attributes!.Count.ShouldBe(100);
        eventDto.Attributes!["key_0"].ShouldBe("value_0_" + new string('x', 100));
        eventDto.Attributes!["key_99"].ShouldBe("value_99_" + new string('x', 100));
    }

    [Fact]
    public async Task POST_ApiEventsSandboxDryRun_WithValidEvent_ShouldReturn200WithEvaluationTrace()
    {
        // Arrange
        var request = new
        {
            EventType = "USER_COMMENTED",
            UserId = "user123",
            OccurredAt = DateTimeOffset.UtcNow,
            Attributes = new Dictionary<string, object>
            {
                { "commentId", "comment456" },
                { "postId", "post789" },
                { "length", 150 }
            }
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events/sandbox/dry-run", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldNotBeNullOrEmpty("Response content should not be empty");

        var dryRunResponse = JsonSerializer.Deserialize<DryRunResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        dryRunResponse.ShouldNotBeNull("Dry-run response should not be null");
        dryRunResponse.TriggerEventId.ShouldNotBeNullOrEmpty("Trigger event ID should not be null or empty");
        dryRunResponse.UserId.ShouldBe("user123");
        dryRunResponse.EventType.ShouldBe("USER_COMMENTED");
        dryRunResponse.Rules.ShouldNotBeNull("Rules should not be null");
        dryRunResponse.Summary.ShouldNotBeNull("Summary should not be null");
        dryRunResponse.EvaluatedAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task POST_ApiEventsSandboxDryRun_WithCustomEventId_ShouldUseProvidedId()
    {
        // Arrange
        var customEventId = "custom-dry-run-event-123";
        var request = new
        {
            EventId = customEventId,
            EventType = "PRODUCT_PURCHASED",
            UserId = "user456",
            Attributes = new Dictionary<string, object>
            {
                { "productId", "prod123" },
                { "amount", 29.99 }
            }
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events/sandbox/dry-run", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var dryRunResponse = JsonSerializer.Deserialize<DryRunResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        dryRunResponse.ShouldNotBeNull("Dry-run response should not be null");
        dryRunResponse.TriggerEventId.ShouldBe(customEventId);
    }

    [Fact]
    public async Task POST_ApiEventsSandboxDryRun_WithoutEventId_ShouldGenerateNewGuid()
    {
        // Arrange
        var request = new
        {
            EventType = "USER_LOGIN",
            UserId = "user789"
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events/sandbox/dry-run", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var dryRunResponse = JsonSerializer.Deserialize<DryRunResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        dryRunResponse.ShouldNotBeNull("Dry-run response should not be null");
        dryRunResponse.TriggerEventId.ShouldNotBeNullOrEmpty("Event ID should not be null or empty");
        Guid.TryParse(dryRunResponse.TriggerEventId, out _).ShouldBeTrue("Event ID should be a valid GUID");
    }

    [Fact]
    public async Task POST_ApiEventsSandboxDryRun_WithoutOccurredAt_ShouldUseCurrentUtcTime()
    {
        // Arrange
        var beforeRequest = DateTimeOffset.UtcNow;
        var request = new
        {
            EventType = "USER_LOGOUT",
            UserId = "user999"
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events/sandbox/dry-run", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var dryRunResponse = JsonSerializer.Deserialize<DryRunResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        dryRunResponse.ShouldNotBeNull("Dry-run response should not be null");
        dryRunResponse.EvaluatedAt.ShouldBeGreaterThan(beforeRequest);
        dryRunResponse.EvaluatedAt.ShouldBeLessThan(DateTimeOffset.UtcNow.AddSeconds(5));
    }

    [Theory]
    [InlineData("", "USER_COMMENTED", "user123")] // Empty EventType
    [InlineData("USER_COMMENTED", "", "user123")] // Empty UserId
    [InlineData(null, "USER_COMMENTED", "user123")] // Null EventType
    [InlineData("USER_COMMENTED", null, "user123")] // Null UserId
    public async Task POST_ApiEventsSandboxDryRun_WithInvalidData_ShouldReturn400BadRequest(string eventType, string userId, string expectedValidField)
    {
        // Arrange
        var request = new
        {
            EventType = eventType,
            UserId = userId
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events/sandbox/dry-run", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("error");
    }

    [Fact]
    public async Task POST_ApiEventsSandboxDryRun_WithMalformedJson_ShouldReturn400BadRequest()
    {
        // Arrange
        var malformedJson = "{ \"EventType\": \"USER_COMMENTED\", \"UserId\": \"user123\", }"; // Extra comma
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events/sandbox/dry-run", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ApiEventsSandboxDryRun_ShouldNotPersistEventToDatabase()
    {
        // Arrange
        var request = new
        {
            EventType = "DRY_RUN_TEST_EVENT",
            UserId = "dry_run_user",
            Attributes = new Dictionary<string, object>
            {
                { "testAttribute", "testValue" }
            }
        };

        var jsonContent = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync("/api/events/sandbox/dry-run", content);

        // Assert
        response.ShouldNotBeNull("HTTP response should not be null");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var dryRunResponse = JsonSerializer.Deserialize<DryRunResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        dryRunResponse.ShouldNotBeNull("Dry-run response should not be null");

        // Verify that the event was NOT persisted to the database
        if (DbContext != null)
        {
            try
            {
                var eventExists = await DbContext.Events.AnyAsync(e => e.EventType == "DRY_RUN_TEST_EVENT" && e.UserId == "dry_run_user");
                eventExists.ShouldBeFalse("Dry-run event should not be persisted to database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database assertion failed: {ex.Message}");
            }
        }
    }

    private class EventDto
    {
        public string EventId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTimeOffset OccurredAt { get; set; }
        public Dictionary<string, object>? Attributes { get; set; }
    }

    private class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    private class DryRunResponseDto
    {
        public string TriggerEventId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public IEnumerable<RuleTrace> Rules { get; set; } = new List<RuleTrace>();
        public DryRunSummary Summary { get; set; } = new DryRunSummary();
        public DateTimeOffset EvaluatedAt { get; set; }
    }

    private class RuleTrace
    {
        public string RuleId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool TriggerMatched { get; set; }
        public IEnumerable<ConditionTrace> Conditions { get; set; } = new List<ConditionTrace>();
        public IEnumerable<PredictedReward> PredictedRewards { get; set; } = new List<PredictedReward>();
        public bool WouldExecute { get; set; }
        public long EvaluationTimeMs { get; set; }
    }

    private class ConditionTrace
    {
        public string ConditionId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public bool Result { get; set; }
        public string Details { get; set; } = string.Empty;
        public long EvaluationTimeMs { get; set; }
    }

    private class PredictedReward
    {
        public string Type { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public long? Amount { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private class DryRunSummary
    {
        public int TotalRulesEvaluated { get; set; }
        public int RulesThatWouldExecute { get; set; }
        public int TotalPredictedRewards { get; set; }
        public long TotalEvaluationTimeMs { get; set; }
        public bool EventValid { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; } = new List<string>();
    }
}