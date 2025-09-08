using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Users;

namespace GamificationEngine.Infrastructure.Caching;

/// <summary>
/// Cached implementation of the user state repository
/// </summary>
public class CachedUserStateRepository : IUserStateRepository
{
    private readonly IUserStateRepository _innerRepository;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _defaultCacheExpiration;

    private const string UserStateCachePrefix = "userstate:";

    public CachedUserStateRepository(IUserStateRepository innerRepository, ICacheService cacheService, TimeSpan? defaultCacheExpiration = null)
    {
        _innerRepository = innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _defaultCacheExpiration = defaultCacheExpiration ?? TimeSpan.FromMinutes(10);
    }

    public async Task<UserState?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var cacheKey = $"{UserStateCachePrefix}{userId}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _innerRepository.GetByUserIdAsync(userId, cancellationToken),
            _defaultCacheExpiration,
            cancellationToken);
    }

    public async Task SaveAsync(UserState userState, CancellationToken cancellationToken = default)
    {
        if (userState == null)
            throw new ArgumentNullException(nameof(userState));

        await _innerRepository.SaveAsync(userState, cancellationToken);

        // Update cache with the new value
        var cacheKey = $"{UserStateCachePrefix}{userState.UserId}";
        await _cacheService.SetAsync(cacheKey, userState, _defaultCacheExpiration, cancellationToken);
    }

    public async Task<IEnumerable<UserState>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // For GetAllAsync, we don't cache the entire list as it could be large and change frequently
        // We just delegate to the inner repository
        return await _innerRepository.GetAllAsync(cancellationToken);
    }

}
