using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GamificationEngine.Api.Controllers;

/// <summary>
/// Controller for managing user wallets and transactions
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly ILogger<WalletController> _logger;

    public WalletController(IWalletService walletService, ILogger<WalletController> logger)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a wallet for a specific user and point category
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet information</returns>
    [HttpGet("users/{userId}/categories/{pointCategoryId}")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWallet(string userId, string pointCategoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _walletService.GetWalletAsync(userId, pointCategoryId, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet for user {UserId} and category {PointCategoryId}", userId, pointCategoryId);
            return StatusCode(500, "An error occurred while retrieving the wallet");
        }
    }

    /// <summary>
    /// Gets all wallets for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of user wallets</returns>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<WalletDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserWallets(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _walletService.GetUserWalletsAsync(userId, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallets for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user wallets");
        }
    }

    /// <summary>
    /// Gets the balance for a specific user and point category
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current balance</returns>
    [HttpGet("users/{userId}/categories/{pointCategoryId}/balance")]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBalance(string userId, string pointCategoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _walletService.GetBalanceAsync(userId, pointCategoryId, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance for user {UserId} and category {PointCategoryId}", userId, pointCategoryId);
            return StatusCode(500, "An error occurred while retrieving the balance");
        }
    }

    /// <summary>
    /// Spends points from a user's wallet
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The spending request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created transaction</returns>
    [HttpPost("users/{userId}/spend")]
    [ProducesResponseType(typeof(WalletTransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SpendPoints(string userId, [FromBody] SpendPointsRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _walletService.SpendPointsAsync(
                userId,
                request.PointCategoryId,
                request.Amount,
                request.Description,
                request.ReferenceId,
                request.Metadata,
                cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error spending points for user {UserId}", userId);
            return StatusCode(500, "An error occurred while spending points");
        }
    }

    /// <summary>
    /// Transfers points from one user to another
    /// </summary>
    /// <param name="fromUserId">The sender user ID</param>
    /// <param name="request">The transfer request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created transfer</returns>
    [HttpPost("users/{fromUserId}/transfer")]
    [ProducesResponseType(typeof(WalletTransferDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TransferPoints(string fromUserId, [FromBody] TransferPointsRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _walletService.TransferPointsAsync(
                fromUserId,
                request.ToUserId,
                request.PointCategoryId,
                request.Amount,
                request.Description,
                request.ReferenceId,
                request.Metadata,
                cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring points from user {FromUserId}", fromUserId);
            return StatusCode(500, "An error occurred while transferring points");
        }
    }

    /// <summary>
    /// Gets transaction history for a user's wallet
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pointCategoryId">The point category ID</param>
    /// <param name="from">Optional start date filter (ISO 8601 format)</param>
    /// <param name="to">Optional end date filter (ISO 8601 format)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions</returns>
    [HttpGet("users/{userId}/categories/{pointCategoryId}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<WalletTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTransactionHistory(
        string userId,
        string pointCategoryId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _walletService.GetTransactionHistoryAsync(userId, pointCategoryId, from, to, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction history for user {UserId} and category {PointCategoryId}", userId, pointCategoryId);
            return StatusCode(500, "An error occurred while retrieving transaction history");
        }
    }
}
