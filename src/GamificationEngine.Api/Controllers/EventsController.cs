using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using GamificationEngine.Domain.Events;
using GamificationEngine.Shared;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Application.Configuration;
using Microsoft.Extensions.Options;
namespace GamificationEngine.Api.Controllers;

/// <summary>
/// API controller for event ingestion and retrieval
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventIngestionService _eventIngestionService;
    private readonly IEventDefinitionRepository _eventDefinitionRepository;
    private readonly IDryRunEvaluationService _dryRunEvaluationService;
    private readonly SimulationSettings? _simulationSettings;

    public EventsController(
        IEventIngestionService eventIngestionService,
        IEventDefinitionRepository eventDefinitionRepository,
        IDryRunEvaluationService dryRunEvaluationService,
        IOptions<EngineConfiguration> configuration)
    {
        _eventIngestionService = eventIngestionService ?? throw new ArgumentNullException(nameof(eventIngestionService));
        _eventDefinitionRepository = eventDefinitionRepository ?? throw new ArgumentNullException(nameof(eventDefinitionRepository));
        _dryRunEvaluationService = dryRunEvaluationService ?? throw new ArgumentNullException(nameof(dryRunEvaluationService));
        _simulationSettings = configuration?.Value?.Simulation;

        // Debug logging
        Console.WriteLine($"Simulation settings loaded: {_simulationSettings?.Enabled}");
        Console.WriteLine($"Engine configuration: {configuration?.Value?.Engine?.Id}");
    }

    /// <summary>
    /// Ingests a new event into the system
    /// </summary>
    /// <param name="request">The event data</param>
    /// <returns>Result of the ingestion operation</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IngestEvent([FromBody] IngestEventRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var @event = new Event(
                request.EventId ?? Guid.NewGuid().ToString(),
                request.EventType,
                request.UserId,
                request.OccurredAt ?? DateTimeOffset.UtcNow,
                request.Attributes
            );

            var result = await _eventIngestionService.IngestEventAsync(@event);

            if (result.IsSuccess && result.Value != null)
                return CreatedAtAction(nameof(GetEvent), new { eventId = @event.EventId }, EventDto.FromDomain(result.Value));

            return BadRequest(new { error = result.Error?.Message ?? "Failed to ingest event" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves events for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="limit">Maximum number of events to return (default: 100, max: 1000)</param>
    /// <param name="offset">Number of events to skip (default: 0)</param>
    /// <returns>Collection of events for the user</returns>
    [HttpGet("user/{userId:minlength(1)}")]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserEvents(string userId, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        var result = await _eventIngestionService.GetUserEventsAsync(userId, limit, offset);

        if (result.IsSuccess && result.Value != null)
            return Ok(EventDto.FromDomain(result.Value));

        return BadRequest(new { error = result.Error?.Message ?? "Failed to retrieve user events" });
    }

    /// <summary>
    /// Retrieves events by type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="limit">Maximum number of events to return (default: 100, max: 1000)</param>
    /// <param name="offset">Number of events to skip (default: 0)</param>
    /// <returns>Collection of events of the specified type</returns>
    [HttpGet("type/{eventType:minlength(1)}")]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEventsByType(string eventType, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
    {
        var result = await _eventIngestionService.GetEventsByTypeAsync(eventType, limit, offset);

        if (result.IsSuccess && result.Value != null)
            return Ok(EventDto.FromDomain(result.Value));

        return BadRequest(new { error = result.Error?.Message ?? "Failed to retrieve events by type" });
    }

    /// <summary>
    /// Retrieves the event catalog with all available event definitions
    /// </summary>
    /// <returns>Collection of event definitions</returns>
    [HttpGet("catalog")]
    [ProducesResponseType(typeof(IEnumerable<EventDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEventCatalog()
    {
        try
        {
            var eventDefinitions = await _eventDefinitionRepository.GetAllAsync();
            var eventDefinitionDtos = eventDefinitions.Select(ed => new EventDefinitionDto
            {
                Id = ed.Id,
                Description = ed.Description,
                PayloadSchema = ed.PayloadSchema
            });

            return Ok(eventDefinitionDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Failed to retrieve event catalog: {ex.Message}" });
        }
    }

    /// <summary>
    /// Retrieves an event by its ID
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <returns>The event if found</returns>
    [HttpGet("{eventId}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEvent(string eventId)
    {
        var result = await _eventIngestionService.GetEventByIdAsync(eventId);

        if (result.IsSuccess)
        {
            if (result.Value == null)
                return NotFound(new { message = "Event not found" });

            return Ok(EventDto.FromDomain(result.Value));
        }

        return BadRequest(new { error = result.Error?.Message ?? "Failed to retrieve event" });
    }

    /// <summary>
    /// Performs a dry-run evaluation of rules for the given event without executing rewards
    /// </summary>
    /// <param name="request">The event data to evaluate</param>
    /// <returns>Detailed evaluation trace showing which rules would execute and what rewards would be awarded</returns>
    [HttpPost("sandbox/dry-run")]
    [ProducesResponseType(typeof(DryRunResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DryRunEvaluation([FromBody] DryRunRequestDto request)
    {
        // Check if simulation is enabled
        if (_simulationSettings?.Enabled != true)
            return NotFound(new { error = "Sandbox dry-run functionality is not enabled. Please enable simulation in configuration." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Create the event from the request
            var @event = new Event(
                request.EventId ?? Guid.NewGuid().ToString(),
                request.EventType,
                request.UserId,
                request.OccurredAt ?? DateTimeOffset.UtcNow,
                request.Attributes
            );

            // Perform dry-run evaluation
            var result = await _dryRunEvaluationService.DryRunRulesAsync(@event);

            if (result.IsSuccess && result.Value != null)
                return Ok(result.Value);

            return BadRequest(new { error = result.Error?.Message ?? "Failed to perform dry-run evaluation" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Internal server error during dry-run evaluation: {ex.Message}" });
        }
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
    [Required]
    [MinLength(1)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user who performed the action
    /// </summary>
    [Required]
    [MinLength(1)]
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
