using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of the event definition repository
/// </summary>
public class InMemoryEventDefinitionRepository : IEventDefinitionRepository
{
    private readonly Dictionary<string, EventDefinition> _eventDefinitions = new();

    public Task<IEnumerable<EventDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_eventDefinitions.Values.AsEnumerable());
    }

    public Task<EventDefinition?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Task.FromResult<EventDefinition?>(null);

        _eventDefinitions.TryGetValue(id, out var eventDefinition);
        return Task.FromResult(eventDefinition);
    }

    public Task AddAsync(EventDefinition eventDefinition, CancellationToken cancellationToken = default)
    {
        if (eventDefinition == null)
            throw new ArgumentNullException(nameof(eventDefinition));

        _eventDefinitions[eventDefinition.Id] = eventDefinition;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(EventDefinition eventDefinition, CancellationToken cancellationToken = default)
    {
        if (eventDefinition == null)
            throw new ArgumentNullException(nameof(eventDefinition));

        _eventDefinitions[eventDefinition.Id] = eventDefinition;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Task.CompletedTask;

        _eventDefinitions.Remove(id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Task.FromResult(false);

        return Task.FromResult(_eventDefinitions.ContainsKey(id));
    }
}
