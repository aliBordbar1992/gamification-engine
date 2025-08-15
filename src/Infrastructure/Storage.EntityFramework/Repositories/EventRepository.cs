using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Infrastructure.Storage.EntityFramework.Repositories;

/// <summary>
/// EF Core implementation of Event repository with PostgreSQL support
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly GamificationEngineDbContext _context;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(GamificationEngineDbContext context, ILogger<EventRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Event?> GetByIdAsync(string eventId)
    {
        try
        {
            var @event = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            return @event;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event with ID {EventId}", eventId);
            throw;
        }
    }

    public async Task<IEnumerable<Event>> GetByUserIdAsync(string userId, int limit, int offset)
    {
        try
        {
            var events = await _context.Events
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.OccurredAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Event>> GetByTypeAsync(string eventType, int limit, int offset)
    {
        try
        {
            var events = await _context.Events
                .Where(e => e.EventType == eventType)
                .OrderByDescending(e => e.OccurredAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events of type {EventType}", eventType);
            throw;
        }
    }

    public async Task StoreAsync(Event @event)
    {
        try
        {
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Event {EventId} stored successfully for user {UserId}", @event.EventId, @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing event {EventId}", @event.EventId);
            throw;
        }
    }

    // Additional methods for EF Core specific functionality
    public async Task<IEnumerable<Event>> GetByUserIdAndEventTypeAsync(string userId, string eventType, CancellationToken cancellationToken = default)
    {
        try
        {
            var events = await _context.Events
                .Where(e => e.UserId == userId && e.EventType == eventType)
                .OrderByDescending(e => e.OccurredAt)
                .ToListAsync(cancellationToken);

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for user {UserId} and type {EventType}", userId, eventType);
            throw;
        }
    }

    public async Task<int> ApplyRetentionPolicyAsync(TimeSpan retentionPeriod, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTimeOffset.UtcNow.Subtract(retentionPeriod);
            var oldEvents = await _context.Events
                .Where(e => e.OccurredAt < cutoffDate)
                .ToListAsync(cancellationToken);

            var deletedCount = oldEvents.Count;
            _context.Events.RemoveRange(oldEvents);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Retention policy applied: {DeletedCount} events older than {CutoffDate} were deleted", deletedCount, cutoffDate);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying retention policy");
            throw;
        }
    }

    public async Task<int> GetEventCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _context.Events.CountAsync(cancellationToken);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event count");
            throw;
        }
    }

    public async Task<int> GetEventCountByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _context.Events
                .Where(e => e.UserId == userId)
                .CountAsync(cancellationToken);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event count for user {UserId}", userId);
            throw;
        }
    }
}