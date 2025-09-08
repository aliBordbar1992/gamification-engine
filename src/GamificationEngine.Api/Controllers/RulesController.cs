using Microsoft.AspNetCore.Mvc;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;

namespace GamificationEngine.Api.Controllers;

/// <summary>
/// API controller for rule management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RulesController : ControllerBase
{
    private readonly IRuleManagementService _ruleManagementService;
    private readonly IEntityManagementService _entityManagementService;

    public RulesController(
        IRuleManagementService ruleManagementService,
        IEntityManagementService entityManagementService)
    {
        _ruleManagementService = ruleManagementService ?? throw new ArgumentNullException(nameof(ruleManagementService));
        _entityManagementService = entityManagementService ?? throw new ArgumentNullException(nameof(entityManagementService));
    }

    #region Rules Management

    /// <summary>
    /// Gets all rules
    /// </summary>
    /// <returns>Collection of all rules</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllRules()
    {
        var result = await _ruleManagementService.GetAllRulesAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets all active rules
    /// </summary>
    /// <returns>Collection of active rules</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<RuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActiveRules()
    {
        var result = await _ruleManagementService.GetActiveRulesAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets rules that can be triggered by a specific event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <returns>Collection of rules for the event type</returns>
    [HttpGet("trigger/{eventType}")]
    [ProducesResponseType(typeof(IEnumerable<RuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRulesByTrigger(string eventType)
    {
        var result = await _ruleManagementService.GetRulesByTriggerAsync(eventType);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a rule by ID
    /// </summary>
    /// <param name="id">The rule ID</param>
    /// <returns>The rule if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRuleById(string id)
    {
        var result = await _ruleManagementService.GetRuleByIdAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new rule
    /// </summary>
    /// <param name="dto">The rule creation data</param>
    /// <returns>The created rule</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRule([FromBody] CreateRuleDto dto)
    {
        var result = await _ruleManagementService.CreateRuleAsync(dto);

        if (result.IsSuccess && result.Value != null)
            return CreatedAtAction(nameof(GetRuleById), new { id = result.Value.Id }, result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates an existing rule
    /// </summary>
    /// <param name="id">The rule ID</param>
    /// <param name="dto">The rule update data</param>
    /// <returns>The updated rule</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRule(string id, [FromBody] UpdateRuleDto dto)
    {
        var result = await _ruleManagementService.UpdateRuleAsync(id, dto);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Deletes a rule
    /// </summary>
    /// <param name="id">The rule ID</param>
    /// <returns>True if deleted successfully</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRule(string id)
    {
        var result = await _ruleManagementService.DeleteRuleAsync(id);

        if (result.IsSuccess)
            return Ok(new { success = true });

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Activates a rule
    /// </summary>
    /// <param name="id">The rule ID</param>
    /// <returns>True if activated successfully</returns>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivateRule(string id)
    {
        var result = await _ruleManagementService.ActivateRuleAsync(id);

        if (result.IsSuccess)
            return Ok(new { success = true });

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Deactivates a rule
    /// </summary>
    /// <param name="id">The rule ID</param>
    /// <returns>True if deactivated successfully</returns>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeactivateRule(string id)
    {
        var result = await _ruleManagementService.DeactivateRuleAsync(id);

        if (result.IsSuccess)
            return Ok(new { success = true });

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    #endregion

    #region Entity Management (CRUD operations for badges, trophies, levels, point categories)

    /// <summary>
    /// Gets all point categories
    /// </summary>
    /// <returns>Collection of point categories</returns>
    [HttpGet("entities/point-categories")]
    public async Task<IActionResult> GetAllPointCategories()
    {
        var result = await _entityManagementService.GetAllPointCategoriesAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new point category
    /// </summary>
    /// <param name="dto">The point category creation data</param>
    /// <returns>The created point category</returns>
    [HttpPost("entities/point-categories")]
    public async Task<IActionResult> CreatePointCategory([FromBody] CreatePointCategoryDto dto)
    {
        var result = await _entityManagementService.CreatePointCategoryAsync(dto);

        if (result.IsSuccess && result.Value != null)
            return CreatedAtAction(nameof(GetPointCategoryById), new { id = result.Value.Id }, result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a point category by ID
    /// </summary>
    /// <param name="id">The point category ID</param>
    /// <returns>The point category if found</returns>
    [HttpGet("entities/point-categories/{id}")]
    public async Task<IActionResult> GetPointCategoryById(string id)
    {
        var result = await _entityManagementService.GetPointCategoryByIdAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates a point category
    /// </summary>
    /// <param name="id">The point category ID</param>
    /// <param name="dto">The point category update data</param>
    /// <returns>The updated point category</returns>
    [HttpPut("entities/point-categories/{id}")]
    public async Task<IActionResult> UpdatePointCategory(string id, [FromBody] UpdatePointCategoryDto dto)
    {
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
    /// <returns>True if deleted successfully</returns>
    [HttpDelete("entities/point-categories/{id}")]
    public async Task<IActionResult> DeletePointCategory(string id)
    {
        var result = await _entityManagementService.DeletePointCategoryAsync(id);

        if (result.IsSuccess)
            return Ok(new { success = true });

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets all badges
    /// </summary>
    /// <returns>Collection of badges</returns>
    [HttpGet("entities/badges")]
    public async Task<IActionResult> GetAllBadges()
    {
        var result = await _entityManagementService.GetAllBadgesAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new badge
    /// </summary>
    /// <param name="dto">The badge creation data</param>
    /// <returns>The created badge</returns>
    [HttpPost("entities/badges")]
    public async Task<IActionResult> CreateBadge([FromBody] CreateBadgeDto dto)
    {
        var result = await _entityManagementService.CreateBadgeAsync(dto);

        if (result.IsSuccess && result.Value != null)
            return CreatedAtAction(nameof(GetBadgeById), new { id = result.Value.Id }, result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a badge by ID
    /// </summary>
    /// <param name="id">The badge ID</param>
    /// <returns>The badge if found</returns>
    [HttpGet("entities/badges/{id}")]
    public async Task<IActionResult> GetBadgeById(string id)
    {
        var result = await _entityManagementService.GetBadgeByIdAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates a badge
    /// </summary>
    /// <param name="id">The badge ID</param>
    /// <param name="dto">The badge update data</param>
    /// <returns>The updated badge</returns>
    [HttpPut("entities/badges/{id}")]
    public async Task<IActionResult> UpdateBadge(string id, [FromBody] UpdateBadgeDto dto)
    {
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
    /// <returns>True if deleted successfully</returns>
    [HttpDelete("entities/badges/{id}")]
    public async Task<IActionResult> DeleteBadge(string id)
    {
        var result = await _entityManagementService.DeleteBadgeAsync(id);

        if (result.IsSuccess)
            return Ok(new { success = true });

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets all trophies
    /// </summary>
    /// <returns>Collection of trophies</returns>
    [HttpGet("entities/trophies")]
    public async Task<IActionResult> GetAllTrophies()
    {
        var result = await _entityManagementService.GetAllTrophiesAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new trophy
    /// </summary>
    /// <param name="dto">The trophy creation data</param>
    /// <returns>The created trophy</returns>
    [HttpPost("entities/trophies")]
    public async Task<IActionResult> CreateTrophy([FromBody] CreateTrophyDto dto)
    {
        var result = await _entityManagementService.CreateTrophyAsync(dto);

        if (result.IsSuccess && result.Value != null)
            return CreatedAtAction(nameof(GetTrophyById), new { id = result.Value.Id }, result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a trophy by ID
    /// </summary>
    /// <param name="id">The trophy ID</param>
    /// <returns>The trophy if found</returns>
    [HttpGet("entities/trophies/{id}")]
    public async Task<IActionResult> GetTrophyById(string id)
    {
        var result = await _entityManagementService.GetTrophyByIdAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates a trophy
    /// </summary>
    /// <param name="id">The trophy ID</param>
    /// <param name="dto">The trophy update data</param>
    /// <returns>The updated trophy</returns>
    [HttpPut("entities/trophies/{id}")]
    public async Task<IActionResult> UpdateTrophy(string id, [FromBody] UpdateTrophyDto dto)
    {
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
    /// <returns>True if deleted successfully</returns>
    [HttpDelete("entities/trophies/{id}")]
    public async Task<IActionResult> DeleteTrophy(string id)
    {
        var result = await _entityManagementService.DeleteTrophyAsync(id);

        if (result.IsSuccess)
            return Ok(new { success = true });

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets all levels
    /// </summary>
    /// <returns>Collection of levels</returns>
    [HttpGet("entities/levels")]
    public async Task<IActionResult> GetAllLevels()
    {
        var result = await _entityManagementService.GetAllLevelsAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new level
    /// </summary>
    /// <param name="dto">The level creation data</param>
    /// <returns>The created level</returns>
    [HttpPost("entities/levels")]
    public async Task<IActionResult> CreateLevel([FromBody] CreateLevelDto dto)
    {
        var result = await _entityManagementService.CreateLevelAsync(dto);

        if (result.IsSuccess && result.Value != null)
            return CreatedAtAction(nameof(GetLevelById), new { id = result.Value.Id }, result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a level by ID
    /// </summary>
    /// <param name="id">The level ID</param>
    /// <returns>The level if found</returns>
    [HttpGet("entities/levels/{id}")]
    public async Task<IActionResult> GetLevelById(string id)
    {
        var result = await _entityManagementService.GetLevelByIdAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates a level
    /// </summary>
    /// <param name="id">The level ID</param>
    /// <param name="dto">The level update data</param>
    /// <returns>The updated level</returns>
    [HttpPut("entities/levels/{id}")]
    public async Task<IActionResult> UpdateLevel(string id, [FromBody] UpdateLevelDto dto)
    {
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
    /// <returns>True if deleted successfully</returns>
    [HttpDelete("entities/levels/{id}")]
    public async Task<IActionResult> DeleteLevel(string id)
    {
        var result = await _entityManagementService.DeleteLevelAsync(id);

        if (result.IsSuccess)
            return Ok(new { success = true });

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    #endregion
}
