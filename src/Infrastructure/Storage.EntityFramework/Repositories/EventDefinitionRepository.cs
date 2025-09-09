using Microsoft.EntityFrameworkCore;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Infrastructure.Storage.EntityFramework.Repositories;

/// <summary>
/// Entity Framework implementation of the event definition repository
/// </summary>
public class EventDefinitionRepository : IEventDefinitionRepository
{
    private readonly GamificationEngineDbContext _context;

    public EventDefinitionRepository(GamificationEngineDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<EventDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EventDefinitions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<EventDefinition?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        return await _context.EventDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task AddAsync(EventDefinition eventDefinition, CancellationToken cancellationToken = default)
    {
        if (eventDefinition == null)
            throw new ArgumentNullException(nameof(eventDefinition));

        _context.EventDefinitions.Add(eventDefinition);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EventDefinition eventDefinition, CancellationToken cancellationToken = default)
    {
        if (eventDefinition == null)
            throw new ArgumentNullException(nameof(eventDefinition));

        _context.EventDefinitions.Update(eventDefinition);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        var eventDefinition = await _context.EventDefinitions
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (eventDefinition != null)
        {
            _context.EventDefinitions.Remove(eventDefinition);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        return await _context.EventDefinitions
            .AnyAsync(e => e.Id == id, cancellationToken);
    }
}
