using GamificationEngine.Application.Abstractions;
using GamificationEngine.Infrastructure.Caching;

namespace GamificationEngine.Infrastructure.Tests.Caching;

/// <summary>
/// Tests for the in-memory cache service
/// </summary>
public class InMemoryCacheServiceTests : IDisposable
{
    private readonly ICacheService _cacheService;
    private bool _disposed = false;

    public InMemoryCacheServiceTests()
    {
        _cacheService = new InMemoryCacheService();
    }

    [Fact]
    public async Task SetAsync_WithValidData_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };

        // Act
        await _cacheService.SetAsync(key, value);

        // Assert
        var exists = await _cacheService.ExistsAsync(key);
        Assert.True(exists);
    }

    [Fact]
    public async Task GetAsync_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = new TestData { Id = 1, Name = "Test" };
        await _cacheService.SetAsync(key, expectedValue);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedValue.Id, result.Id);
        Assert.Equal(expectedValue.Name, result.Name);
    }

    [Fact]
    public async Task GetAsync_WithNonExistingKey_ShouldReturnNull()
    {
        // Act
        var result = await _cacheService.GetAsync<TestData>("non-existing-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_WithExistingKey_ShouldRemoveValue()
    {
        // Arrange
        var key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };
        await _cacheService.SetAsync(key, value);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        var exists = await _cacheService.ExistsAsync(key);
        Assert.False(exists);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ShouldExpireAfterTime()
    {
        // Arrange
        var key = "expiring-key";
        var value = new TestData { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act
        await _cacheService.SetAsync(key, value, expiration);

        // Assert - Should exist immediately
        var existsImmediately = await _cacheService.ExistsAsync(key);
        Assert.True(existsImmediately);

        // Wait for expiration
        await Task.Delay(150);

        // Assert - Should be expired
        var existsAfterExpiration = await _cacheService.ExistsAsync(key);
        Assert.False(existsAfterExpiration);
    }

    [Fact]
    public async Task SetManyAsync_WithMultipleItems_ShouldStoreAllValues()
    {
        // Arrange
        var items = new Dictionary<string, TestData>
        {
            { "key1", new TestData { Id = 1, Name = "Test1" } },
            { "key2", new TestData { Id = 2, Name = "Test2" } },
            { "key3", new TestData { Id = 3, Name = "Test3" } }
        };

        // Act
        await _cacheService.SetManyAsync(items);

        // Assert
        foreach (var item in items)
        {
            var exists = await _cacheService.ExistsAsync(item.Key);
            Assert.True(exists);
        }
    }

    [Fact]
    public async Task GetManyAsync_WithMultipleKeys_ShouldReturnAllValues()
    {
        // Arrange
        var items = new Dictionary<string, TestData>
        {
            { "key1", new TestData { Id = 1, Name = "Test1" } },
            { "key2", new TestData { Id = 2, Name = "Test2" } },
            { "key3", new TestData { Id = 3, Name = "Test3" } }
        };
        await _cacheService.SetManyAsync(items);

        // Act
        var result = await _cacheService.GetManyAsync<TestData>(items.Keys);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(items["key1"].Id, result["key1"]?.Id);
        Assert.Equal(items["key2"].Id, result["key2"]?.Id);
        Assert.Equal(items["key3"].Id, result["key3"]?.Id);
    }

    [Fact]
    public async Task GetOrSetAsync_WithNonExistingKey_ShouldCallFactoryAndStore()
    {
        // Arrange
        var key = "factory-key";
        var expectedValue = new TestData { Id = 42, Name = "Factory Test" };
        var factoryCallCount = 0;

        // Act
        var result = await _cacheService.GetOrSetAsync(key, () =>
        {
            factoryCallCount++;
            return Task.FromResult(expectedValue);
        });

        // Assert
        Assert.Equal(expectedValue.Id, result.Id);
        Assert.Equal(expectedValue.Name, result.Name);
        Assert.Equal(1, factoryCallCount);

        // Verify it's cached
        var cachedResult = await _cacheService.GetAsync<TestData>(key);
        Assert.NotNull(cachedResult);
        Assert.Equal(expectedValue.Id, cachedResult.Id);
    }

    [Fact]
    public async Task GetOrSetAsync_WithExistingKey_ShouldNotCallFactory()
    {
        // Arrange
        var key = "cached-key";
        var cachedValue = new TestData { Id = 1, Name = "Cached" };
        await _cacheService.SetAsync(key, cachedValue);
        var factoryCallCount = 0;

        // Act
        var result = await _cacheService.GetOrSetAsync(key, () =>
        {
            factoryCallCount++;
            return Task.FromResult(new TestData { Id = 2, Name = "Factory" });
        });

        // Assert
        Assert.Equal(cachedValue.Id, result.Id);
        Assert.Equal(0, factoryCallCount);
    }

    [Fact]
    public async Task ClearAsync_ShouldRemoveAllValues()
    {
        // Arrange
        var items = new Dictionary<string, TestData>
        {
            { "key1", new TestData { Id = 1, Name = "Test1" } },
            { "key2", new TestData { Id = 2, Name = "Test2" } }
        };
        await _cacheService.SetManyAsync(items);

        // Act
        await _cacheService.ClearAsync();

        // Assert
        foreach (var key in items.Keys)
        {
            var exists = await _cacheService.ExistsAsync(key);
            Assert.False(exists);
        }
    }

    [Fact]
    public async Task SetAsync_WithNullValue_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _cacheService.SetAsync<TestData>("key", null!));
    }

    [Fact]
    public async Task SetAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var value = new TestData { Id = 1, Name = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheService.SetAsync("", value));
    }

    [Fact]
    public async Task GetAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheService.GetAsync<TestData>(""));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_cacheService is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _disposed = true;
        }
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
