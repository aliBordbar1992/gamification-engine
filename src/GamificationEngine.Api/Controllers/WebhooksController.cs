using Microsoft.AspNetCore.Mvc;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;

namespace GamificationEngine.Api.Controllers;

/// <summary>
/// API controller for webhook management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IWebhookService _webhookService;

    public WebhooksController(IWebhookService webhookService)
    {
        _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
    }

    /// <summary>
    /// Gets all registered webhooks
    /// </summary>
    /// <returns>Collection of registered webhooks</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WebhookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllWebhooks()
    {
        var result = await _webhookService.GetAllWebhooksAsync();

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets webhooks for a specific event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <returns>Collection of webhooks for the event type</returns>
    [HttpGet("event-type/{eventType}")]
    [ProducesResponseType(typeof(IEnumerable<WebhookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWebhooksByEventType(string eventType)
    {
        var result = await _webhookService.GetWebhooksByEventTypeAsync(eventType);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a webhook by ID
    /// </summary>
    /// <param name="id">The webhook ID</param>
    /// <returns>The webhook if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWebhookById(string id)
    {
        var result = await _webhookService.GetWebhookByIdAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Registers a new webhook
    /// </summary>
    /// <param name="dto">The webhook registration data</param>
    /// <returns>The registered webhook</returns>
    [HttpPost]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterWebhook([FromBody] RegisterWebhookDto dto)
    {
        var result = await _webhookService.RegisterWebhookAsync(dto);

        if (result.IsSuccess && result.Value != null)
            return CreatedAtAction(nameof(GetWebhookById), new { id = result.Value.Id }, result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates an existing webhook
    /// </summary>
    /// <param name="id">The webhook ID</param>
    /// <param name="dto">The webhook update data</param>
    /// <returns>The updated webhook</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateWebhook(string id, [FromBody] UpdateWebhookDto dto)
    {
        var result = await _webhookService.UpdateWebhookAsync(id, dto);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Deletes a webhook
    /// </summary>
    /// <param name="id">The webhook ID</param>
    /// <returns>True if deleted successfully</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteWebhook(string id)
    {
        var result = await _webhookService.DeleteWebhookAsync(id);

        if (result.IsSuccess)
            return Ok(new { success = true });

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Tests a webhook endpoint
    /// </summary>
    /// <param name="id">The webhook ID</param>
    /// <returns>Test result</returns>
    [HttpPost("{id}/test")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestWebhook(string id)
    {
        var result = await _webhookService.TestWebhookAsync(id);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        if (result.Error?.Contains("not found") == true)
            return NotFound(new { error = result.Error });

        return BadRequest(new { error = result.Error });
    }
}
