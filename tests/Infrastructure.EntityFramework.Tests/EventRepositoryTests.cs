using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GamificationEngine.Infrastructure.Storage.EntityFramework;
using GamificationEngine.Infrastructure.Storage.EntityFramework.Repositories;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;
using Xunit;
using Shouldly;

namespace GamificationEngine.Infrastructure.EntityFramework.Tests;

/// <summary>
/// Tests for Event repository using EF Core
/// </summary>
public class EventRepositoryTests
{
    private readonly DbContextOptions<GamificationEngineDbContext> _options;
    private readonly ILogger<EventRepository> _logger;

    public EventRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<GamificationEngineDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _logger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<EventRepository>();
    }

    private GamificationEngineDbContext CreateContext()
    {
        var context = new GamificationEngineDbContext(_options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task StoreAsync_ShouldStoreEventSuccessfully()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EventRepository(context, _logger);
        var @event = new Event("test-event-1", "USER_COMMENTED", "user-1", DateTimeOffset.UtcNow);

        // Act
        await repository.StoreAsync(@event);

        // Assert
        var storedEvent = await context.Events.FirstOrDefaultAsync(e => e.EventId == "test-event-1");
        storedEvent.ShouldNotBeNull();
        storedEvent.EventType.ShouldBe("USER_COMMENTED");
        storedEvent.UserId.ShouldBe("user-1");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEvent_WhenEventExists()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EventRepository(context, _logger);
        var @event = new Event("test-event-1", "USER_COMMENTED", "user-1", DateTimeOffset.UtcNow);
        await repository.StoreAsync(@event);

        // Act
        var result = await repository.GetByIdAsync("test-event-1");

        // Assert
        result.ShouldNotBeNull();
        result.EventId.ShouldBe("test-event-1");
        result.EventType.ShouldBe("USER_COMMENTED");
        result.UserId.ShouldBe("user-1");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEventDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EventRepository(context, _logger);

        // Act
        var result = await repository.GetByIdAsync("non-existent-event");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserEvents()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EventRepository(context, _logger);
        var events = new[]
        {
            new Event("test-event-1", "USER_COMMENTED", "user-1", DateTimeOffset.UtcNow),
            new Event("test-event-2", "USER_LIKED", "user-2", DateTimeOffset.UtcNow),
            new Event("test-event-3", "USER_SHARED", "user-1", DateTimeOffset.UtcNow)
        };

        foreach (var @event in events)
        {
            await repository.StoreAsync(@event);
        }

        // Act
        var result = await repository.GetByUserIdAsync("user-1", 10, 0);

        // Assert
        result.Count().ShouldBe(2);
        result.ShouldContain(e => e.EventId == "test-event-1");
        result.ShouldContain(e => e.EventId == "test-event-3");
    }

    [Fact]
    public async Task GetByTypeAsync_ShouldReturnEventsOfType()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EventRepository(context, _logger);
        var events = new[]
        {
            new Event("test-event-1", "USER_COMMENTED", "user-1", DateTimeOffset.UtcNow),
            new Event("test-event-2", "USER_LIKED", "user-2", DateTimeOffset.UtcNow),
            new Event("test-event-3", "USER_COMMENTED", "user-3", DateTimeOffset.UtcNow)
        };

        foreach (var @event in events)
        {
            await repository.StoreAsync(@event);
        }

        // Act
        var result = await repository.GetByTypeAsync("USER_COMMENTED", 10, 0);

        // Assert
        result.Count().ShouldBe(2);
        result.ShouldContain(e => e.EventId == "test-event-1");
        result.ShouldContain(e => e.EventId == "test-event-3");
    }

    [Fact]
    public async Task GetByUserIdAndEventTypeAsync_ShouldReturnFilteredEvents()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EventRepository(context, _logger);
        var events = new[]
        {
            new Event("test-event-1", "USER_COMMENTED", "user-1", DateTimeOffset.UtcNow),
            new Event("test-event-2", "USER_LIKED", "user-1", DateTimeOffset.UtcNow),
            new Event("test-event-3", "USER_COMMENTED", "user-2", DateTimeOffset.UtcNow)
        };

        foreach (var @event in events)
        {
            await repository.StoreAsync(@event);
        }

        // Act
        var result = await repository.GetByUserIdAndEventTypeAsync("user-1", "USER_COMMENTED");

        // Assert
        result.Count().ShouldBe(1);
        result.ShouldContain(e => e.EventId == "test-event-1");
    }

    [Fact]
    public async Task GetEventCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EventRepository(context, _logger);
        var events = new[]
        {
            new Event("test-event-1", "USER_COMMENTED", "user-1", DateTimeOffset.UtcNow),
            new Event("test-event-2", "USER_LIKED", "user-2", DateTimeOffset.UtcNow)
        };

        foreach (var @event in events)
        {
            await repository.StoreAsync(@event);
        }

        // Act
        var result = await repository.GetEventCountAsync();

        // Assert
        result.ShouldBe(2);
    }

    [Fact]
    public async Task GetEventCountByUserIdAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EventRepository(context, _logger);
        var events = new[]
        {
            new Event("test-event-1", "USER_COMMENTED", "user-1", DateTimeOffset.UtcNow),
            new Event("test-event-2", "USER_LIKED", "user-1", DateTimeOffset.UtcNow),
            new Event("test-event-3", "USER_SHARED", "user-2", DateTimeOffset.UtcNow)
        };

        foreach (var @event in events)
        {
            await repository.StoreAsync(@event);
        }

        // Act
        var result = await repository.GetEventCountByUserIdAsync("user-1");

        // Assert
        result.ShouldBe(2);
    }

    [Fact]
    public async Task ApplyRetentionPolicyAsync_ShouldDeleteOldEvents()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EventRepository(context, _logger);
        var oldEvent = new Event("old-event", "USER_COMMENTED", "user-1", DateTimeOffset.UtcNow.AddDays(-100));
        var newEvent = new Event("new-event", "USER_LIKED", "user-1", DateTimeOffset.UtcNow);

        await repository.StoreAsync(oldEvent);
        await repository.StoreAsync(newEvent);

        // Act
        var result = await repository.ApplyRetentionPolicyAsync(TimeSpan.FromDays(50));

        // Assert
        result.ShouldBe(1); // One old event deleted

        var remainingEvents = await context.Events.ToListAsync();
        remainingEvents.Count.ShouldBe(1);
        remainingEvents.ShouldContain(e => e.EventId == "new-event");
    }
}