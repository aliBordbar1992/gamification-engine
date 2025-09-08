using Microsoft.AspNetCore.Mvc;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;

namespace GamificationEngine.Api.Controllers;

/// <summary>
/// API controller for managing point categories
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PointCategoriesController : ControllerBase
{
    private readonly IEntityManagementService _entityManagementService;

    public PointCategoriesController(IEntityManagementService entityManagementService)
    {
        _entityManagementService = entityManagementService ?? throw new ArgumentNullException(nameof(entityManagementService));
    }

    /// <summary>
    /// Gets all point categories
    /// </summary>
    /// <returns>Collection of point categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PointCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _entityManagementService.GetAllPointCategoriesAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a point category by ID
    /// </summary>
    /// <param name="id">The point category ID</param>
    /// <returns>The point category if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PointCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _entityManagementService.GetPointCategoryByIdAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new point category
    /// </summary>
    /// <param name="request">The point category data</param>
    /// <returns>The created point category</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PointCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePointCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dto = new CreatePointCategoryDto
        {
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            Aggregation = request.Aggregation
        };

        var result = await _entityManagementService.CreatePointCategoryAsync(dto);

        if (result.IsSuccess && result.Value != null)
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates an existing point category
    /// </summary>
    /// <param name="id">The point category ID</param>
    /// <param name="request">The updated point category data</param>
    /// <returns>The updated point category</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PointCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePointCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dto = new UpdatePointCategoryDto
        {
            Name = request.Name,
            Description = request.Description,
            Aggregation = request.Aggregation
        };

        var result = await _entityManagementService.UpdatePointCategoryAsync(id, dto);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Deletes a point category
    /// </summary>
    /// <param name="id">The point category ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _entityManagementService.DeletePointCategoryAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }
}

/// <summary>
/// Request model for creating a point category
/// </summary>
public class CreatePointCategoryRequest
{
    /// <summary>
    /// Unique identifier for the point category
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the point category
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the point category
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// How points in this category should be aggregated (sum, max, min, avg, count)
    /// </summary>
    public string Aggregation { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating a point category
/// </summary>
public class UpdatePointCategoryRequest
{
    /// <summary>
    /// Display name for the point category
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the point category
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// How points in this category should be aggregated (sum, max, min, avg, count)
    /// </summary>
    public string Aggregation { get; set; } = string.Empty;
}
