using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of the event repository
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly List<Event> _events = new();
    private readonly object _lock = new();

    public Task StoreAsync(Event @event)
    {
        lock (_lock)
        {
            _events.Add(@event);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Event>> GetByUserIdAsync(string userId, int limit, int offset)
    {
        lock (_lock)
        {
            var userEvents = _events
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.OccurredAt)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return Task.FromResult<IEnumerable<Event>>(userEvents);
        }
    }

    public Task<IEnumerable<Event>> GetByTypeAsync(string eventType, int limit, int offset)
    {
        lock (_lock)
        {
            var typeEvents = _events
                .Where(e => e.EventType == eventType)
                .OrderByDescending(e => e.OccurredAt)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return Task.FromResult<IEnumerable<Event>>(typeEvents);
        }
    }

    public Task<Event?> GetByIdAsync(string eventId)
    {
        lock (_lock)
        {
            var @event = _events.FirstOrDefault(e => e.EventId == eventId);
            return Task.FromResult(@event);
        }
    }
}