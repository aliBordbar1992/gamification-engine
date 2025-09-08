using Microsoft.AspNetCore.Mvc;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;

namespace GamificationEngine.Api.Controllers;

/// <summary>
/// API controller for managing levels
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LevelsController : ControllerBase
{
    private readonly IEntityManagementService _entityManagementService;

    public LevelsController(IEntityManagementService entityManagementService)
    {
        _entityManagementService = entityManagementService ?? throw new ArgumentNullException(nameof(entityManagementService));
    }

    /// <summary>
    /// Gets all levels
    /// </summary>
    /// <returns>Collection of levels</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LevelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _entityManagementService.GetAllLevelsAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets all levels for a specific category
    /// </summary>
    /// <param name="category">The point category</param>
    /// <returns>Collection of levels for the category</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<LevelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByCategory(string category)
    {
        var result = await _entityManagementService.GetLevelsByCategoryAsync(category);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a level by ID
    /// </summary>
    /// <param name="id">The level ID</param>
    /// <returns>The level if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LevelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _entityManagementService.GetLevelByIdAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new level
    /// </summary>
    /// <param name="request">The level data</param>
    /// <returns>The created level</returns>
    [HttpPost]
    [ProducesResponseType(typeof(LevelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLevelRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dto = new CreateLevelDto
        {
            Id = request.Id,
            Name = request.Name,
            Category = request.Category,
            MinPoints = request.MinPoints
        };

        var result = await _entityManagementService.CreateLevelAsync(dto);

        if (result.IsSuccess && result.Value != null)
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates an existing level
    /// </summary>
    /// <param name="id">The level ID</param>
    /// <param name="request">The updated level data</param>
    /// <returns>The updated level</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(LevelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateLevelRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dto = new UpdateLevelDto
        {
            Name = request.Name,
            Category = request.Category,
            MinPoints = request.MinPoints
        };

        var result = await _entityManagementService.UpdateLevelAsync(id, dto);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Deletes a level
    /// </summary>
    /// <param name="id">The level ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _entityManagementService.DeleteLevelAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }
}

/// <summary>
/// Request model for creating a level
/// </summary>
public class CreateLevelRequest
{
    /// <summary>
    /// Unique identifier for the level
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the level
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Point category this level applies to
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Minimum points required to achieve this level
    /// </summary>
    public long MinPoints { get; set; }
}

/// <summary>
/// Request model for updating a level
/// </summary>
public class UpdateLevelRequest
{
    /// <summary>
    /// Display name for the level
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Point category this level applies to
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Minimum points required to achieve this level
    /// </summary>
    public long MinPoints { get; set; }
}
