using Microsoft.AspNetCore.Mvc;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;

namespace GamificationEngine.Api.Controllers;

/// <summary>
/// API controller for managing badges
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BadgesController : ControllerBase
{
    private readonly IEntityManagementService _entityManagementService;

    public BadgesController(IEntityManagementService entityManagementService)
    {
        _entityManagementService = entityManagementService ?? throw new ArgumentNullException(nameof(entityManagementService));
    }

    /// <summary>
    /// Gets all badges
    /// </summary>
    /// <returns>Collection of badges</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _entityManagementService.GetAllBadgesAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets all visible badges
    /// </summary>
    /// <returns>Collection of visible badges</returns>
    [HttpGet("visible")]
    public async Task<IActionResult> GetVisible()
    {
        var result = await _entityManagementService.GetVisibleBadgesAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a badge by ID
    /// </summary>
    /// <param name="id">The badge ID</param>
    /// <returns>The badge if found</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _entityManagementService.GetBadgeByIdAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new badge
    /// </summary>
    /// <param name="request">The badge data</param>
    /// <returns>The created badge</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBadgeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dto = new CreateBadgeDto
        {
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            Image = request.Image,
            Visible = request.Visible
        };

        var result = await _entityManagementService.CreateBadgeAsync(dto);

        if (result.IsSuccess && result.Value != null)
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates an existing badge
    /// </summary>
    /// <param name="id">The badge ID</param>
    /// <param name="request">The updated badge data</param>
    /// <returns>The updated badge</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateBadgeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dto = new UpdateBadgeDto
        {
            Name = request.Name,
            Description = request.Description,
            Image = request.Image,
            Visible = request.Visible
        };

        var result = await _entityManagementService.UpdateBadgeAsync(id, dto);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Deletes a badge
    /// </summary>
    /// <param name="id">The badge ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _entityManagementService.DeleteBadgeAsync(id);

        if (result.IsSuccess)
            return NoContent();

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }
}

/// <summary>
/// Request model for creating a badge
/// </summary>
public class CreateBadgeRequest
{
    /// <summary>
    /// Unique identifier for the badge
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the badge
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the badge
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Image URL or path for the badge
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Whether the badge is visible to users
    /// </summary>
    public bool Visible { get; set; } = true;
}

/// <summary>
/// Request model for updating a badge
/// </summary>
public class UpdateBadgeRequest
{
    /// <summary>
    /// Display name for the badge
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the badge
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Image URL or path for the badge
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Whether the badge is visible to users
    /// </summary>
    public bool Visible { get; set; }
}
