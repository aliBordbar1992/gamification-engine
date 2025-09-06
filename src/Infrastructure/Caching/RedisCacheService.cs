using System.Text.Json;
using GamificationEngine.Application.Abstractions;
using StackExchange.Redis;

namespace GamificationEngine.Infrastructure.Caching;

/// <summary>
/// Redis implementation of the cache service
/// </summary>
public class RedisCacheService : ICacheService, IDisposable
{
    private readonly IDatabase _database;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        _database = _connectionMultiplexer.GetDatabase();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (JsonException)
        {
            // Remove corrupted item
            await _database.KeyDeleteAsync(key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            await _database.StringSetAsync(key, serializedValue, expiration);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Failed to serialize value for key '{key}': {ex.Message}", ex);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        return await _database.KeyExistsAsync(key);
    }

    public async Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) where T : class
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        var keyArray = keys.ToArray();
        var values = await _database.StringGetAsync(keyArray.Select(k => (RedisKey)k).ToArray());

        var result = new Dictionary<string, T?>();
        for (int i = 0; i < keyArray.Length; i++)
        {
            var key = keyArray[i];
            var value = values[i];

            if (value.HasValue)
            {
                try
                {
                    var deserializedValue = JsonSerializer.Deserialize<T>(value!, _jsonOptions);
                    result[key] = deserializedValue;
                }
                catch (JsonException)
                {
                    // Remove corrupted item
                    await _database.KeyDeleteAsync(key);
                    result[key] = null;
                }
            }
            else
            {
                result[key] = null;
            }
        }

        return result;
    }

    public async Task SetManyAsync<T>(IDictionary<string, T> items, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var keyValuePairs = new KeyValuePair<RedisKey, RedisValue>[items.Count];
        var index = 0;

        foreach (var item in items)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(item.Value, _jsonOptions);
                keyValuePairs[index] = new KeyValuePair<RedisKey, RedisValue>(item.Key, serializedValue);
                index++;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Failed to serialize value for key '{item.Key}': {ex.Message}", ex);
            }
        }

        await _database.StringSetAsync(keyValuePairs);

        if (expiration.HasValue)
        {
            var tasks = items.Keys.Select(key => _database.KeyExpireAsync(key, expiration.Value));
            await Task.WhenAll(tasks);
        }
    }

    public async Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        await _database.KeyDeleteAsync(redisKeys);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
        await server.FlushDatabaseAsync();
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

    public void Dispose()
    {
        _connectionMultiplexer?.Dispose();
    }
}
