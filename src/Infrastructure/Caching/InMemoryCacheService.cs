using System.Collections.Concurrent;
using System.Text.Json;
using GamificationEngine.Application.Abstractions;

namespace GamificationEngine.Infrastructure.Caching;

/// <summary>
/// In-memory implementation of the cache service
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
    private readonly Timer _cleanupTimer;
    private readonly object _lock = new();

    public InMemoryCacheService()
    {
        // Cleanup expired items every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredItems, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        if (_cache.TryGetValue(key, out var item) && !item.IsExpired())
        {
            try
            {
                var value = JsonSerializer.Deserialize<T>(item.Value);
                return Task.FromResult(value);
            }
            catch (JsonException)
            {
                // Remove corrupted item
                _cache.TryRemove(key, out _);
                return Task.FromResult<T?>(null);
            }
        }

        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var expiresAt = expiration.HasValue ? DateTimeOffset.UtcNow.Add(expiration.Value) : (DateTimeOffset?)null;

            var cacheItem = new CacheItem(serializedValue, expiresAt);
            _cache.AddOrUpdate(key, cacheItem, (_, _) => cacheItem);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Failed to serialize value for key '{key}': {ex.Message}", ex);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        var exists = _cache.TryGetValue(key, out var item) && !item.IsExpired();
        return Task.FromResult(exists);
    }

    public async Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) where T : class
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        var result = new Dictionary<string, T?>();
        var tasks = keys.Select(async key =>
        {
            var value = await GetAsync<T>(key, cancellationToken);
            return new KeyValuePair<string, T?>(key, value);
        });

        var results = await Task.WhenAll(tasks);
        foreach (var kvp in results)
        {
            result[kvp.Key] = kvp.Value;
        }

        return result;
    }

    public async Task SetManyAsync<T>(IDictionary<string, T> items, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var tasks = items.Select(kvp => SetAsync(kvp.Key, kvp.Value, expiration, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        var tasks = keys.Select(key => RemoveAsync(key, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _cache.Clear();
        return Task.CompletedTask;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var newValue = await factory();
        await SetAsync(key, newValue, expiration, cancellationToken);
        return newValue;
    }

    private void CleanupExpiredItems(object? state)
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.IsExpired())
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }

    private class CacheItem
    {
        public string Value { get; }
        public DateTimeOffset? ExpiresAt { get; }

        public CacheItem(string value, DateTimeOffset? expiresAt)
        {
            Value = value;
            ExpiresAt = expiresAt;
        }

        public bool IsExpired()
        {
            return ExpiresAt.HasValue && DateTimeOffset.UtcNow > ExpiresAt.Value;
        }
    }
}
