using Microsoft.AspNetCore.Mvc;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;

namespace GamificationEngine.Api.Controllers;

/// <summary>
/// API controller for leaderboard operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LeaderboardsController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardsController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService ?? throw new ArgumentNullException(nameof(leaderboardService));
    }

    /// <summary>
    /// Gets leaderboard data based on query parameters
    /// </summary>
    /// <param name="type">Leaderboard type (points, badges, trophies, level)</param>
    /// <param name="category">Category for points/level leaderboards</param>
    /// <param name="timeRange">Time range (daily, weekly, monthly, alltime)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of entries per page (1-1000)</param>
    /// <returns>Leaderboard data</returns>
    [HttpGet]
    public async Task<IActionResult> GetLeaderboard(
        [FromQuery] string type,
        [FromQuery] string? category = null,
        [FromQuery] string timeRange = "alltime",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new LeaderboardQueryDto
        {
            Type = type,
            Category = category,
            TimeRange = timeRange,
            Page = page,
            PageSize = pageSize
        };

        var result = await _leaderboardService.GetLeaderboardAsync(query);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets points leaderboard for a specific category
    /// </summary>
    /// <param name="category">Point category</param>
    /// <param name="timeRange">Time range (daily, weekly, monthly, alltime)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of entries per page (1-1000)</param>
    /// <returns>Points leaderboard data</returns>
    [HttpGet("points/{category}")]
    public async Task<IActionResult> GetPointsLeaderboard(
        string category,
        [FromQuery] string timeRange = "alltime",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _leaderboardService.GetPointsLeaderboardAsync(category, timeRange, page, pageSize);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets badges leaderboard
    /// </summary>
    /// <param name="timeRange">Time range (daily, weekly, monthly, alltime)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of entries per page (1-1000)</param>
    /// <returns>Badges leaderboard data</returns>
    [HttpGet("badges")]
    public async Task<IActionResult> GetBadgesLeaderboard(
        [FromQuery] string timeRange = "alltime",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _leaderboardService.GetBadgesLeaderboardAsync(timeRange, page, pageSize);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets trophies leaderboard
    /// </summary>
    /// <param name="timeRange">Time range (daily, weekly, monthly, alltime)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of entries per page (1-1000)</param>
    /// <returns>Trophies leaderboard data</returns>
    [HttpGet("trophies")]
    public async Task<IActionResult> GetTrophiesLeaderboard(
        [FromQuery] string timeRange = "alltime",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _leaderboardService.GetTrophiesLeaderboardAsync(timeRange, page, pageSize);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets levels leaderboard for a specific category
    /// </summary>
    /// <param name="category">Level category</param>
    /// <param name="timeRange">Time range (daily, weekly, monthly, alltime)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of entries per page (1-1000)</param>
    /// <returns>Levels leaderboard data</returns>
    [HttpGet("levels/{category}")]
    public async Task<IActionResult> GetLevelsLeaderboard(
        string category,
        [FromQuery] string timeRange = "alltime",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _leaderboardService.GetLevelsLeaderboardAsync(category, timeRange, page, pageSize);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a user's rank in a specific leaderboard
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="type">Leaderboard type (points, badges, trophies, level)</param>
    /// <param name="category">Category for points/level leaderboards</param>
    /// <param name="timeRange">Time range (daily, weekly, monthly, alltime)</param>
    /// <returns>User rank information</returns>
    [HttpGet("user/{userId}/rank")]
    public async Task<IActionResult> GetUserRank(
        string userId,
        [FromQuery] string type,
        [FromQuery] string? category = null,
        [FromQuery] string timeRange = "alltime")
    {
        var query = new LeaderboardQueryDto
        {
            Type = type,
            Category = category,
            TimeRange = timeRange,
            Page = 1,
            PageSize = 1 // Not used for rank queries
        };

        var result = await _leaderboardService.GetUserRankAsync(userId, query);

        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Refreshes leaderboard cache for better performance
    /// </summary>
    /// <param name="type">Leaderboard type (points, badges, trophies, level)</param>
    /// <param name="category">Category for points/level leaderboards</param>
    /// <param name="timeRange">Time range (daily, weekly, monthly, alltime)</param>
    /// <returns>Success status</returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshCache(
        [FromQuery] string type,
        [FromQuery] string? category = null,
        [FromQuery] string timeRange = "alltime")
    {
        var query = new LeaderboardQueryDto
        {
            Type = type,
            Category = category,
            TimeRange = timeRange,
            Page = 1,
            PageSize = 1 // Not used for cache refresh
        };

        var result = await _leaderboardService.RefreshLeaderboardCacheAsync(query);

        if (result.IsSuccess && result.Value)
            return Ok(new { success = true, message = "Cache refreshed successfully" });

        return BadRequest(new { error = result.Error });
    }
}
