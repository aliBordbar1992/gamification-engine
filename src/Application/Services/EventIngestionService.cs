using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Shared;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Application.Abstractions;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Implementation of the event ingestion service
/// </summary>
public class EventIngestionService : IEventIngestionService
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventQueue _eventQueue;
    private readonly IEventValidationService _eventValidationService;

    public EventIngestionService(
        IEventRepository eventRepository,
        IEventQueue eventQueue,
        IEventValidationService eventValidationService)
    {
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _eventQueue = eventQueue ?? throw new ArgumentNullException(nameof(eventQueue));
        _eventValidationService = eventValidationService ?? throw new ArgumentNullException(nameof(eventValidationService));
    }

    public async Task<Result<Event, DomainError>> IngestEventAsync(Event @event)
    {
        try
        {
            if (@event == null)
                return Result<Event, DomainError>.Failure(new InvalidEventError("Event cannot be null"));

            // Validate event against catalog
            var isValid = await _eventValidationService.ValidateEventAsync(@event);
            if (!isValid)
                return Result<Event, DomainError>.Failure(new InvalidEventError($"Event validation failed for event type: {@event.EventType}"));

            // Enqueue the event for asynchronous processing
            var enqueueResult = await _eventQueue.EnqueueAsync(@event);
            if (!enqueueResult.IsSuccess)
            {
                return Result<Event, DomainError>.Failure(enqueueResult.Error!);
            }

            return Result<Event, DomainError>.Success(@event);
        }
        catch (Exception ex)
        {
            return Result<Event, DomainError>.Failure(new EventStorageError($"Failed to enqueue event: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<Event>, DomainError>> GetUserEventsAsync(string userId, int limit = 100, int offset = 0)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<IEnumerable<Event>, DomainError>.Failure(new InvalidUserIdError("User ID cannot be empty"));

            if (limit <= 0 || limit > 1000)
                return Result<IEnumerable<Event>, DomainError>.Failure(new InvalidParameterError("Limit must be between 1 and 1000"));

            if (offset < 0)
                return Result<IEnumerable<Event>, DomainError>.Failure(new InvalidParameterError("Offset cannot be negative"));

            var events = await _eventRepository.GetByUserIdAsync(userId, limit, offset);
            return Result<IEnumerable<Event>, DomainError>.Success(events);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Event>, DomainError>.Failure(new EventRetrievalError($"Failed to retrieve user events: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<Event>, DomainError>> GetEventsByTypeAsync(string eventType, int limit = 100, int offset = 0)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(eventType))
                return Result<IEnumerable<Event>, DomainError>.Failure(new InvalidEventTypeError("Event type cannot be empty"));

            if (limit <= 0 || limit > 1000)
                return Result<IEnumerable<Event>, DomainError>.Failure(new InvalidParameterError("Limit must be between 1 and 1000"));

            if (offset < 0)
                return Result<IEnumerable<Event>, DomainError>.Failure(new InvalidParameterError("Offset cannot be negative"));

            var events = await _eventRepository.GetByTypeAsync(eventType, limit, offset);
            return Result<IEnumerable<Event>, DomainError>.Success(events);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Event>, DomainError>.Failure(new EventRetrievalError($"Failed to retrieve events by type: {ex.Message}"));
        }
    }
}