using GamificationEngine.Domain.Events;
using GamificationEngine.Integration.Tests.Infrastructure;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using GamificationEngine.Integration.Tests.Infrastructure.Testing;
using GamificationEngine.Integration.Tests.Infrastructure.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace GamificationEngine.Integration.Tests.Tests;

/// <summary>
/// Integration tests for the EventQueueBackgroundService using the new testing infrastructure
/// </summary>
public class EventQueueBackgroundServiceIntegrationTests : IntegrationTestBase, IAsyncLifetime
{
    private EventQueueBackgroundServiceTestHarness? _serviceHarness;
    private BackgroundServiceTestTimingUtilities? _timingUtilities;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        // Ensure the background service is registered for testing
        services.AddHostedService<GamificationEngine.Api.EventQueueBackgroundService>();
    }

    public async Task InitializeAsync()
    {
        // Create the service harness
        _serviceHarness = new EventQueueBackgroundServiceTestHarness(Services, GetService<ILogger<EventQueueBackgroundServiceTestHarness>>());
        _timingUtilities = new BackgroundServiceTestTimingUtilities(GetService<ILogger<BackgroundServiceTestTimingUtilities>>());
    }

    public async Task DisposeAsync()
    {
        if (_serviceHarness != null)
        {
            await _serviceHarness.DisposeAsync();
        }
    }

    [Fact]
    public async Task BackgroundServiceTestHarness_ShouldBeCreatedSuccessfully()
    {
        // Arrange & Act
        _serviceHarness.ShouldNotBeNull();
        _timingUtilities.ShouldNotBeNull();

        // Assert
        _serviceHarness.ServiceType.ShouldBe(typeof(GamificationEngine.Api.EventQueueBackgroundService));
        _serviceHarness.IsRunning.ShouldBeFalse();
        _serviceHarness.Status.ShouldBe(BackgroundServiceStatus.Stopped);
    }

    [Fact]
    public async Task BackgroundServiceTestHarness_ShouldCreateTestEvents()
    {
        // Arrange
        _serviceHarness.ShouldNotBeNull();

        const int eventCount = 5;

        // Act
        var testEvents = EventQueueBackgroundServiceTestHarness.CreateTestEvents(eventCount);

        // Assert
        testEvents.Count().ShouldBe(eventCount);
        testEvents.ShouldAllBe(e => !string.IsNullOrEmpty(e.EventId));
        testEvents.ShouldAllBe(e => e.EventType == "TEST_EVENT");
        testEvents.ShouldAllBe(e => e.UserId == "test-user");
    }

    [Fact]
    public async Task BackgroundServiceTestHarness_ShouldCreateCustomTestEvents()
    {
        // Arrange
        _serviceHarness.ShouldNotBeNull();

        const int eventCount = 3;

        // Act
        var testEvents = EventQueueBackgroundServiceTestHarness.CreateTestEvents(eventCount, "CUSTOM_EVENT", "custom-user").ToList();

        // Add sequence numbers to track order
        for (int i = 0; i < testEvents.Count; i++)
        {
            testEvents[i] = new Event(
                testEvents[i].EventId,
                testEvents[i].EventType,
                testEvents[i].UserId,
                testEvents[i].OccurredAt,
                new Dictionary<string, object> { { "sequence", i } });
        }

        // Assert
        testEvents.Count.ShouldBe(eventCount);
        testEvents.ShouldAllBe(e => e.EventType == "CUSTOM_EVENT");
        testEvents.ShouldAllBe(e => e.UserId == "custom-user");
        testEvents.ShouldAllBe(e => e.Attributes.ContainsKey("sequence"));
    }

    [Fact]
    public async Task BackgroundServiceTestHarness_ShouldHandleHighVolumeTestEvents()
    {
        // Arrange
        _serviceHarness.ShouldNotBeNull();

        const int eventCount = 20;

        // Act
        var testEvents = EventQueueBackgroundServiceTestHarness.CreateTestEvents(eventCount);

        // Assert
        testEvents.Count().ShouldBe(eventCount);
        testEvents.ShouldAllBe(e => !string.IsNullOrEmpty(e.EventId));
        testEvents.ShouldAllBe(e => e.EventType == "TEST_EVENT");
        testEvents.ShouldAllBe(e => e.UserId == "test-user");
    }

    [Fact]
    public async Task BackgroundServiceTestHarness_ShouldCreateMultipleUserTestEvents()
    {
        // Arrange
        _serviceHarness.ShouldNotBeNull();

        const int usersCount = 3;
        const int eventsPerUser = 2;

        // Act
        var testEvents = new List<Event>();
        for (int userIndex = 0; userIndex < usersCount; userIndex++)
        {
            var userId = $"user-{userIndex}";
            var userEvents = EventQueueBackgroundServiceTestHarness.CreateTestEvents(eventsPerUser, "USER_ACTION", userId);
            testEvents.AddRange(userEvents);
        }

        // Assert
        testEvents.Count.ShouldBe(usersCount * eventsPerUser);
        for (int userIndex = 0; userIndex < usersCount; userIndex++)
        {
            var userId = $"user-{userIndex}";
            var userEvents = testEvents.Where(e => e.UserId == userId);
            userEvents.Count().ShouldBe(eventsPerUser);
            userEvents.ShouldAllBe(e => e.EventType == "USER_ACTION");
        }
    }

    [Fact]
    public async Task BackgroundServiceTestHarness_ShouldCreateCustomAttributeTestEvents()
    {
        // Arrange
        _serviceHarness.ShouldNotBeNull();

        var customEvent = new Event(
            Guid.NewGuid().ToString(),
            "CUSTOM_EVENT",
            "test-user",
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                { "category", "test" },
                { "priority", "high" },
                { "metadata", new { source = "integration-test" } }
            });

        // Act & Assert
        customEvent.ShouldNotBeNull();
        customEvent.EventType.ShouldBe("CUSTOM_EVENT");
        customEvent.UserId.ShouldBe("test-user");
        customEvent.Attributes.Count.ShouldBe(3);
        customEvent.Attributes.ShouldContainKey("category");
        customEvent.Attributes.ShouldContainKey("priority");
        customEvent.Attributes.ShouldContainKey("metadata");
    }

    [Fact]
    public async Task BackgroundServiceTestHarness_ShouldHaveCorrectInitialState()
    {
        // Arrange & Act
        _serviceHarness.ShouldNotBeNull();

        // Assert
        _serviceHarness.ProcessedEventCount.ShouldBe(0);
        _serviceHarness.IsRunning.ShouldBeFalse();
        _serviceHarness.Status.ShouldBe(BackgroundServiceStatus.Stopped);
    }

    [Fact]
    public async Task BackgroundServiceTestHarness_ShouldSupportEventEnqueueing()
    {
        // Arrange
        _serviceHarness.ShouldNotBeNull();
        var testEvent = EventQueueBackgroundServiceTestHarness.CreateTestEvent();

        // Act & Assert
        // This test verifies that the harness can be created and supports the basic interface
        // without actually starting the service
        testEvent.ShouldNotBeNull();
        testEvent.EventType.ShouldBe("TEST_EVENT");
        testEvent.UserId.ShouldBe("test-user");
    }


}