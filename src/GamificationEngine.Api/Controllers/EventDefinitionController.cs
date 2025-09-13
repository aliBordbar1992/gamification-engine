using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;

namespace GamificationEngine.Api.Controllers;

/// <summary>
/// API controller for event definition management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventDefinitionController : ControllerBase
{
    private readonly IEventDefinitionManagementService _eventDefinitionManagementService;

    public EventDefinitionController(IEventDefinitionManagementService eventDefinitionManagementService)
    {
        _eventDefinitionManagementService = eventDefinitionManagementService ?? throw new ArgumentNullException(nameof(eventDefinitionManagementService));
    }

    /// <summary>
    /// Gets all event definitions
    /// </summary>
    /// <returns>Collection of event definitions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllEventDefinitions()
    {
        var result = await _eventDefinitionManagementService.GetAllEventDefinitionsAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets an event definition by ID
    /// </summary>
    /// <param name="id">The event definition ID</param>
    /// <returns>The event definition if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEventDefinitionById(string id)
    {
        var result = await _eventDefinitionManagementService.GetEventDefinitionByIdAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new event definition
    /// </summary>
    /// <param name="dto">The event definition creation data</param>
    /// <returns>The created event definition</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EventDefinitionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEventDefinition([FromBody] CreateEventDefinitionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _eventDefinitionManagementService.CreateEventDefinitionAsync(dto);

        if (result.IsSuccess && result.Value != null)
            return CreatedAtAction(nameof(GetEventDefinitionById), new { id = result.Value.Id }, result.Value);

        if (result.Error?.Contains("already exists") == true)
            return Conflict(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates an existing event definition
    /// </summary>
    /// <param name="id">The event definition ID</param>
    /// <param name="dto">The event definition update data</param>
    /// <returns>The updated event definition</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EventDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEventDefinition(string id, [FromBody] UpdateEventDefinitionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _eventDefinitionManagementService.UpdateEventDefinitionAsync(id, dto);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Deletes an event definition
    /// </summary>
    /// <param name="id">The event definition ID</param>
    /// <returns>No content on successful deletion</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteEventDefinition(string id)
    {
        var result = await _eventDefinitionManagementService.DeleteEventDefinitionAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }
}
