using Microsoft.AspNetCore.Mvc;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;

namespace GamificationEngine.Api.Controllers;

/// <summary>
/// API controller for user state management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserStateService _userStateService;

    public UsersController(IUserStateService userStateService)
    {
        _userStateService = userStateService ?? throw new ArgumentNullException(nameof(userStateService));
    }

    /// <summary>
    /// Gets the complete user state including points, badges, trophies, and current levels
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Complete user state</returns>
    [HttpGet("{userId}/state")]
    [ProducesResponseType(typeof(UserStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserState(string userId)
    {
        var result = await _userStateService.GetUserStateAsync(userId);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets user points for all categories
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>User points by category</returns>
    [HttpGet("{userId}/points")]
    [ProducesResponseType(typeof(Dictionary<string, long>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserPoints(string userId)
    {
        var result = await _userStateService.GetUserPointsAsync(userId);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets user points for a specific category
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="category">The point category</param>
    /// <returns>User points for the category</returns>
    [HttpGet("{userId}/points/{category}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserPointsForCategory(string userId, string category)
    {
        var result = await _userStateService.GetUserPointsForCategoryAsync(userId, category);

        if (result.IsSuccess)
            return Ok(new { userId, category, points = result.Value });

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets user badges
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Collection of user badges</returns>
    [HttpGet("{userId}/badges")]
    [ProducesResponseType(typeof(IEnumerable<BadgeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserBadges(string userId)
    {
        var result = await _userStateService.GetUserBadgesAsync(userId);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets user trophies
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Collection of user trophies</returns>
    [HttpGet("{userId}/trophies")]
    [ProducesResponseType(typeof(IEnumerable<TrophyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserTrophies(string userId)
    {
        var result = await _userStateService.GetUserTrophiesAsync(userId);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets user current level for a specific category
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="category">The point category</param>
    /// <returns>Current level for the category</returns>
    [HttpGet("{userId}/levels/{category}")]
    [ProducesResponseType(typeof(LevelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserCurrentLevel(string userId, string category)
    {
        var result = await _userStateService.GetUserCurrentLevelAsync(userId, category);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets user current levels for all categories
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Current levels by category</returns>
    [HttpGet("{userId}/levels")]
    [ProducesResponseType(typeof(Dictionary<string, LevelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserCurrentLevels(string userId)
    {
        var result = await _userStateService.GetUserCurrentLevelsAsync(userId);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets user reward history
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of entries per page (1-1000)</param>
    /// <returns>User reward history</returns>
    [HttpGet("{userId}/rewards/history")]
    [ProducesResponseType(typeof(UserRewardHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserRewardHistory(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _userStateService.GetUserRewardHistoryAsync(userId, page, pageSize);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }
}
