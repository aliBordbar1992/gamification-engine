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

    public RulesController(IRuleManagementService ruleManagementService)
    {
        _ruleManagementService = ruleManagementService ?? throw new ArgumentNullException(nameof(ruleManagementService));
    }

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
}
