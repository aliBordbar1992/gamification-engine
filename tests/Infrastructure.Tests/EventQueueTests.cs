using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Infrastructure.Storage.InMemory;
using Shouldly;
using Xunit;

namespace GamificationEngine.Infrastructure.Tests;

public class EventQueueTests
{
    [Fact]
    public async Task EnqueueAsync_WithValidEvent_ShouldReturnSuccess()
    {
        // Arrange
        IEventQueue queue = new InMemoryEventQueue();
        var @event = new Event("test-1", "TEST_EVENT", "user123", DateTimeOffset.UtcNow);

        // Act
        var result = await queue.EnqueueAsync(@event);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
        queue.GetQueueSize().ShouldBe(1);
        queue.IsEmpty().ShouldBeFalse();
    }

    [Fact]
    public async Task EnqueueAsync_WithNullEvent_ShouldReturnFailure()
    {
        // Arrange
        IEventQueue queue = new InMemoryEventQueue();

        // Act
        var result = await queue.EnqueueAsync(null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error!.Code.ShouldBe("INVALID_EVENT");
        queue.GetQueueSize().ShouldBe(0);
        queue.IsEmpty().ShouldBeTrue();
    }

    [Fact]
    public async Task DequeueAsync_WithEmptyQueue_ShouldReturnNull()
    {
        // Arrange
        IEventQueue queue = new InMemoryEventQueue();

        // Act
        var @event = await queue.DequeueAsync();

        // Assert
        @event.ShouldBeNull();
        queue.GetQueueSize().ShouldBe(0);
        queue.IsEmpty().ShouldBeTrue();
    }

    [Fact]
    public async Task DequeueAsync_WithEventsInQueue_ShouldReturnEventsInOrder()
    {
        // Arrange
        IEventQueue queue = new InMemoryEventQueue();
        var event1 = new Event("test-1", "TEST_EVENT_1", "user123", DateTimeOffset.UtcNow);
        var event2 = new Event("test-2", "TEST_EVENT_2", "user123", DateTimeOffset.UtcNow);

        await queue.EnqueueAsync(event1);
        await queue.EnqueueAsync(event2);

        // Act & Assert
        queue.GetQueueSize().ShouldBe(2);

        var dequeuedEvent1 = await queue.DequeueAsync();
        dequeuedEvent1.ShouldNotBeNull();
        dequeuedEvent1!.EventId.ShouldBe("test-1");
        queue.GetQueueSize().ShouldBe(1);

        var dequeuedEvent2 = await queue.DequeueAsync();
        dequeuedEvent2.ShouldNotBeNull();
        dequeuedEvent2!.EventId.ShouldBe("test-2");
        queue.GetQueueSize().ShouldBe(0);
        queue.IsEmpty().ShouldBeTrue();
    }

    [Fact]
    public async Task MultipleEnqueueDequeue_ShouldMaintainCorrectQueueSize()
    {
        // Arrange
        IEventQueue queue = new InMemoryEventQueue();
        var events = Enumerable.Range(1, 5)
            .Select(i => new Event($"test-{i}", $"TEST_EVENT_{i}", "user123", DateTimeOffset.UtcNow))
            .ToList();

        // Act - Enqueue all events
        foreach (var @event in events)
        {
            await queue.EnqueueAsync(@event);
        }

        // Assert
        queue.GetQueueSize().ShouldBe(5);
        queue.IsEmpty().ShouldBeFalse();

        // Act - Dequeue all events
        for (int i = 0; i < 5; i++)
        {
            var dequeuedEvent = await queue.DequeueAsync();
            dequeuedEvent.ShouldNotBeNull();
            dequeuedEvent!.EventId.ShouldBe($"test-{i + 1}");
        }

        // Assert
        queue.GetQueueSize().ShouldBe(0);
        queue.IsEmpty().ShouldBeTrue();
    }
}