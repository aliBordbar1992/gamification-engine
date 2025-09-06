namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Interface for caching services
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from the cache
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached value if found, null otherwise</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in the cache
    /// </summary>
    /// <typeparam name="T">The type of the value to cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expiration">Optional expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in the cache
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the key exists, false otherwise</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple values from the cache
    /// </summary>
    /// <typeparam name="T">The type of the cached values</typeparam>
    /// <param name="keys">The cache keys</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of keys and their cached values</returns>
    Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets multiple values in the cache
    /// </summary>
    /// <typeparam name="T">The type of the values to cache</typeparam>
    /// <param name="items">Dictionary of keys and values to cache</param>
    /// <param name="expiration">Optional expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetManyAsync<T>(IDictionary<string, T> items, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes multiple values from the cache
    /// </summary>
    /// <param name="keys">The cache keys to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all values from the cache
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets a value in the cache using a factory function
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value if not cached</param>
    /// <param name="expiration">Optional expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached or newly created value</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
}
