using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GamificationEngine.Domain.Users;
using GamificationEngine.Domain.Repositories;

namespace GamificationEngine.Infrastructure.Storage.EntityFramework.Repositories;

/// <summary>
/// EF Core implementation of UserState repository with PostgreSQL support
/// </summary>
public class UserStateRepository : IUserStateRepository
{
    private readonly GamificationEngineDbContext _context;
    private readonly ILogger<UserStateRepository> _logger;

    public UserStateRepository(GamificationEngineDbContext context, ILogger<UserStateRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserState?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userState = await _context.UserStates
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            return userState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user state for user {UserId}", userId);
            throw;
        }
    }

    public async Task SaveAsync(UserState userState, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUserState = await _context.UserStates
                .FirstOrDefaultAsync(u => u.UserId == userState.UserId, cancellationToken);

            if (existingUserState != null)
            {
                _context.UserStates.Update(userState);
            }
            else
            {
                _context.UserStates.Add(userState);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User state for user {UserId} saved successfully", userState.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user state for user {UserId}", userState.UserId);
            throw;
        }
    }

    // Additional methods for EF Core specific functionality
    public async Task<IEnumerable<UserState>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userStates = await _context.UserStates.ToListAsync(cancellationToken);
            return userStates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all user states");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _context.UserStates
                .AnyAsync(u => u.UserId == userId, cancellationToken);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user state exists for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetUserCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _context.UserStates.CountAsync(cancellationToken);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user count");
            throw;
        }
    }
}