using Microsoft.AspNetCore.Mvc;
using GamificationEngine.Application.Services;
using GamificationEngine.Domain.Events;
using GamificationEngine.Shared;

namespace GamificationEngine.Api.Controllers;

/// <summary>
/// API controller for event ingestion and retrieval
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventIngestionService _eventIngestionService;

    public EventsController(IEventIngestionService eventIngestionService)
    {
        _eventIngestionService = eventIngestionService ?? throw new ArgumentNullException(nameof(eventIngestionService));
    }

    /// <summary>
    /// Ingests a new event into the system
    /// </summary>
    /// <param name="request">The event data</param>
    /// <returns>Result of the ingestion operation</returns>
    [HttpPost]
    public async Task<IActionResult> IngestEvent([FromBody] IngestEventRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var @event = new Event(
            request.EventId ?? Guid.NewGuid().ToString(),
            request.EventType,
            request.UserId,
            request.OccurredAt ?? DateTimeOffset.UtcNow,
            request.Attributes
        );

        var result = await _eventIngestionService.IngestEventAsync(@event);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetEvent), new { eventId = @event.EventId }, @event);

        return BadRequest(new { error = result.Error?.Message });
    }

    /// <summary>
    /// Retrieves an event by its ID
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <returns>The event if found</returns>
    [HttpGet("{eventId}")]
    public IActionResult GetEvent(string eventId)
    {
        // This would require adding a method to the service, but for now we'll return not implemented
        return StatusCode(501, new { message = "Get event by ID not yet implemented" });
    }

    /// <summary>
    /// Retrieves events for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="limit">Maximum number of events to return (default: 100, max: 1000)</param>
    /// <param name="offset">Number of events to skip (default: 0)</param>
    /// <returns>Collection of events for the user</returns>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserEvents(string userId, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        var result = await _eventIngestionService.GetUserEventsAsync(userId, limit, offset);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error?.Message });
    }

    /// <summary>
    /// Retrieves events by type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="limit">Maximum number of events to return (default: 100, max: 1000)</param>
    /// <param name="offset">Number of events to skip (default: 0)</param>
    /// <returns>Collection of events of the specified type</returns>
    [HttpGet("type/{eventType}")]
    public async Task<IActionResult> GetEventsByType(string eventType, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        var result = await _eventIngestionService.GetEventsByTypeAsync(eventType, limit, offset);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error?.Message });
    }
}

/// <summary>
/// Request model for ingesting an event
/// </summary>
public class IngestEventRequest
{
    /// <summary>
    /// Optional event ID. If not provided, a new GUID will be generated
    /// </summary>
    public string? EventId { get; set; }

    /// <summary>
    /// The type of event (e.g., "USER_COMMENTED", "PRODUCT_PURCHASED")
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user who performed the action
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// When the event occurred. If not provided, current UTC time will be used
    /// </summary>
    public DateTimeOffset? OccurredAt { get; set; }

    /// <summary>
    /// Additional attributes for the event
    /// </summary>
    public Dictionary<string, object>? Attributes { get; set; }
}