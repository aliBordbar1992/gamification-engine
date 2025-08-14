using System.Collections.Concurrent;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Shared;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of the event queue using ConcurrentQueue
/// </summary>
public class InMemoryEventQueue : IEventQueue
{
    private readonly ConcurrentQueue<Event> _queue = new();
    private readonly SemaphoreSlim _semaphore = new(0);

    public Task<Result<bool, DomainError>> EnqueueAsync(Event @event)
    {
        try
        {
            if (@event == null)
                return Task.FromResult(Result<bool, DomainError>.Failure(new InvalidEventError("Event cannot be null")));

            _queue.Enqueue(@event);
            _semaphore.Release();

            return Task.FromResult(Result<bool, DomainError>.Success(true));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<bool, DomainError>.Failure(new EventStorageError($"Failed to enqueue event: {ex.Message}")));
        }
    }

    public async Task<Event?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Wait for an event to be available
            await _semaphore.WaitAsync(cancellationToken);

            if (_queue.TryDequeue(out var @event))
            {
                return @event;
            }

            return null;
        }
        catch (OperationCanceledException)
        {
            // Semaphore was released due to cancellation
            return null;
        }
    }

    public int GetQueueSize()
    {
        return _queue.Count;
    }

    public bool IsEmpty()
    {
        return _queue.IsEmpty;
    }
}