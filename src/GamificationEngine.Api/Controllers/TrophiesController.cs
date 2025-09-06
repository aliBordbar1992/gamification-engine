using Microsoft.AspNetCore.Mvc;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;

namespace GamificationEngine.Api.Controllers;

/// <summary>
/// API controller for managing trophies
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TrophiesController : ControllerBase
{
    private readonly IEntityManagementService _entityManagementService;

    public TrophiesController(IEntityManagementService entityManagementService)
    {
        _entityManagementService = entityManagementService ?? throw new ArgumentNullException(nameof(entityManagementService));
    }

    /// <summary>
    /// Gets all trophies
    /// </summary>
    /// <returns>Collection of trophies</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _entityManagementService.GetAllTrophiesAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets all visible trophies
    /// </summary>
    /// <returns>Collection of visible trophies</returns>
    [HttpGet("visible")]
    public async Task<IActionResult> GetVisible()
    {
        var result = await _entityManagementService.GetVisibleTrophiesAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a trophy by ID
    /// </summary>
    /// <param name="id">The trophy ID</param>
    /// <returns>The trophy if found</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _entityManagementService.GetTrophyByIdAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new trophy
    /// </summary>
    /// <param name="request">The trophy data</param>
    /// <returns>The created trophy</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTrophyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dto = new CreateTrophyDto
        {
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            Image = request.Image,
            Visible = request.Visible
        };

        var result = await _entityManagementService.CreateTrophyAsync(dto);

        if (result.IsSuccess && result.Value != null)
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates an existing trophy
    /// </summary>
    /// <param name="id">The trophy ID</param>
    /// <param name="request">The updated trophy data</param>
    /// <returns>The updated trophy</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTrophyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dto = new UpdateTrophyDto
        {
            Name = request.Name,
            Description = request.Description,
            Image = request.Image,
            Visible = request.Visible
        };

        var result = await _entityManagementService.UpdateTrophyAsync(id, dto);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Deletes a trophy
    /// </summary>
    /// <param name="id">The trophy ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _entityManagementService.DeleteTrophyAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }
}

/// <summary>
/// Request model for creating a trophy
/// </summary>
public class CreateTrophyRequest
{
    /// <summary>
    /// Unique identifier for the trophy
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the trophy
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the trophy
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Image URL or path for the trophy
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Whether the trophy is visible to users
    /// </summary>
    public bool Visible { get; set; } = true;
}

/// <summary>
/// Request model for updating a trophy
/// </summary>
public class UpdateTrophyRequest
{
    /// <summary>
    /// Display name for the trophy
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the trophy
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Image URL or path for the trophy
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Whether the trophy is visible to users
    /// </summary>
    public bool Visible { get; set; }
}
