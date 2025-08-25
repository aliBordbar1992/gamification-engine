using GamificationEngine.Infrastructure.Storage.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using GamificationEngine.Integration.Tests.Infrastructure.Testing;
using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;

namespace GamificationEngine.Integration.Tests.Tests;

/// <summary>
/// Tests for test infrastructure abstractions
/// </summary>
public class TestInfrastructureAbstractionsTests : IAsyncDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ITestLifecycleManager _lifecycleManager;
    private readonly ITestDataManager _dataManager;
    private readonly ITestTimingUtilities _timingUtilities;
    private readonly ITestDatabase _testDatabase;

    public TestInfrastructureAbstractionsTests()
    {
        var services = new ServiceCollection();

        // Add test infrastructure
        services.AddTestInfrastructure();

        // Add test database (in-memory for unit testing)
        services.AddTestDatabase("InMemory");

        // Add logging
        services.AddTestLogging(LogLevel.Debug);

        // Add mock test database
        services.AddSingleton<ITestDatabase, MockTestDatabase>();

        _serviceProvider = services.BuildServiceProvider();

        _lifecycleManager = _serviceProvider.GetRequiredService<ITestLifecycleManager>();
        _dataManager = _serviceProvider.GetRequiredService<ITestDataManager>();
        _timingUtilities = _serviceProvider.GetRequiredService<ITestTimingUtilities>();
        _testDatabase = _serviceProvider.GetRequiredService<ITestDatabase>();
    }

    [Fact]
    public void TestInfrastructureServices_ShouldBeRegistered()
    {
        // Assert
        Assert.NotNull(_serviceProvider.GetService<ITestLifecycleManager>());
        Assert.NotNull(_serviceProvider.GetService<ITestDataManager>());
        Assert.NotNull(_serviceProvider.GetService<ITestTimingUtilities>());
    }

    [Fact]
    public async Task TestLifecycleManager_ShouldManageTestState()
    {
        // Act
        await _lifecycleManager.SetUpAsync();
        var state = _lifecycleManager.GetCurrentState();

        // Assert
        Assert.NotNull(state);
        Assert.NotEmpty(state.TestId);
        Assert.Equal(TestExecutionPhase.Running, state.CurrentPhase);
        Assert.True(state.TestStartTime > DateTime.MinValue);

        // Act
        await _lifecycleManager.TearDownAsync();
        state = _lifecycleManager.GetCurrentState();

        // Assert
        Assert.Equal(TestExecutionPhase.Completed, state.CurrentPhase);
    }

    [Fact]
    public async Task TestLifecycleManager_ShouldHandleIsolation()
    {
        // Arrange
        var testId = "test-isolation-1";

        // Act
        await _lifecycleManager.SetUpIsolationAsync(testId);
        var state = _lifecycleManager.GetCurrentState();

        // Assert
        Assert.Equal(testId, state.TestId);
        Assert.True(state.IsIsolated);
    }

    [Fact]
    public void TestDataManager_ShouldCreateTestEvents()
    {
        // Act
        var @event = _dataManager.CreateEvent("user-1", "TEST_EVENT");
        var events = _dataManager.CreateEvents("user-2", 3, "BATCH_EVENT");
        var mixedEvents = _dataManager.CreateMixedEventTypes("user-3",
            new Dictionary<string, int> { { "TYPE_A", 2 }, { "TYPE_B", 3 } });

        // Assert
        Assert.NotNull(@event);
        Assert.Equal("user-1", @event.UserId);
        Assert.Equal("TEST_EVENT", @event.EventType);

        Assert.Equal(3, events.Count);
        Assert.All(events, e => Assert.Equal("user-2", e.UserId));
        Assert.All(events, e => Assert.Equal("BATCH_EVENT", e.EventType));

        Assert.Equal(5, mixedEvents.Count);
        Assert.Equal(2, mixedEvents.Count(e => e.EventType == "TYPE_A"));
        Assert.Equal(3, mixedEvents.Count(e => e.EventType == "TYPE_B"));
    }

    [Fact]
    public void TestDataManager_ShouldCreateTestUserStates()
    {
        // Act
        var userState = _dataManager.CreateUserState("user-1",
            new Dictionary<string, long> { { "XP", 100 }, { "Coins", 50 } },
            new List<string> { "Badge1", "Badge2" });

        // Assert
        Assert.NotNull(userState);
        Assert.Equal("user-1", userState.UserId);
        Assert.Equal(100, userState.PointsByCategory["XP"]);
        Assert.Equal(50, userState.PointsByCategory["Coins"]);
        Assert.Contains("Badge1", userState.Badges);
        Assert.Contains("Badge2", userState.Badges);
    }

    [Fact]
    public void TestDataManager_ShouldCreateFixtures()
    {
        // Act
        var basicFixture = _dataManager.CreateFixture("basic_user");
        var powerFixture = _dataManager.CreateFixture("power_user");
        var customFixture = _dataManager.CreateFixture("custom_fixture");

        // Assert
        Assert.NotNull(basicFixture);
        Assert.Equal("Basic User", basicFixture.Name);
        Assert.NotEmpty(basicFixture.Events);
        Assert.NotEmpty(basicFixture.UserStates);

        Assert.NotNull(powerFixture);
        Assert.Equal("Power User", powerFixture.Name);
        Assert.True(powerFixture.Events.Count > basicFixture.Events.Count);

        Assert.NotNull(customFixture);
        Assert.Equal("custom_fixture", customFixture.Name);
    }

    [Fact]
    public void TestDataManager_ShouldValidateTestData()
    {
        // Arrange
        var userState = _dataManager.CreateUserState("user-1");

        // Act & Assert
        Assert.True(_dataManager.ValidateTestData(userState, u => u.UserId == "user-1"));
        Assert.False(_dataManager.ValidateTestData(userState, u => u.UserId == "user-2"));
    }

    [Fact]
    public async Task TestTimingUtilities_ShouldMeasureExecutionTime()
    {
        // Act
        var syncTime = _timingUtilities.MeasureExecutionTime(() =>
        {
            Task.Delay(100).Wait();
        });

        var asyncTime = await _timingUtilities.MeasureExecutionTimeAsync(async () =>
        {
            await Task.Delay(100);
        });

        // Assert
        Assert.True(syncTime.TotalMilliseconds >= 100);
        Assert.True(asyncTime.TotalMilliseconds >= 100);
    }

    [Fact]
    public async Task TestTimingUtilities_ShouldWaitForCondition()
    {
        // Arrange
        var conditionMet = false;
        var task = Task.Run(async () =>
        {
            await Task.Delay(100);
            conditionMet = true;
        });

        // Act
        var result = await _timingUtilities.WaitForConditionAsync(
            () => conditionMet,
            TimeSpan.FromSeconds(1));

        // Assert
        Assert.True(result);
        Assert.True(conditionMet);
    }

    [Fact]
    public async Task TestTimingUtilities_ShouldCreateTimer()
    {
        // Arrange
        var timer = _timingUtilities.CreateTimer(TimeSpan.FromMilliseconds(100));

        // Assert
        Assert.False(timer.HasExpired);
        Assert.True(timer.RemainingTime > TimeSpan.Zero);

        // Act
        await Task.Delay(150);

        // Assert
        Assert.True(timer.HasExpired);
        Assert.Equal(TimeSpan.Zero, timer.RemainingTime);
    }

    [Fact]
    public async Task TestTimingUtilities_ShouldHandleTaskWaiting()
    {
        // Arrange
        var tasks = new List<Task>
        {
            Task.Delay(100),
            Task.Delay(200),
            Task.Delay(300)
        };

        // Act
        var allCompleted = await _timingUtilities.WaitForAllTasksAsync(tasks, TimeSpan.FromSeconds(1));
        var anyCompleted = await _timingUtilities.WaitForAnyTaskAsync(tasks, TimeSpan.FromSeconds(1));

        // Assert
        Assert.True(allCompleted);
        Assert.NotNull(anyCompleted);
    }

    [Fact]
    public async Task TestInfrastructure_ShouldWorkTogether()
    {
        // Arrange
        var testId = "integration-test-1";

        // Act - Set up test lifecycle
        await _lifecycleManager.SetUpAsync();
        _lifecycleManager.SetTestInfo("IntegrationTest", "TestInfrastructureAbstractionsTests");

        // Create test data
        var events = _dataManager.CreateEvents("test-user", 5, "INTEGRATION_EVENT");
        var userState = _dataManager.CreateUserState("test-user", new Dictionary<string, long> { { "XP", 500 } });

        // Add test data to lifecycle manager
        _lifecycleManager.AddTestData("events", events);
        _lifecycleManager.AddTestData("userState", userState);
        _lifecycleManager.AddMetadata("testType", "integration");

        // Measure execution time
        var executionTime = await _timingUtilities.MeasureExecutionTimeAsync(async () =>
        {
            await Task.Delay(50);
        });

        // Assert
        var state = _lifecycleManager.GetCurrentState();
        Assert.Equal("IntegrationTest", state.TestName);
        Assert.Equal("TestInfrastructureAbstractionsTests", state.TestClassName);
        Assert.Equal("integration", state.Metadata["testType"]);
        Assert.NotNull(state.TestData["events"]);
        Assert.NotNull(state.TestData["userState"]);
        Assert.True(executionTime.TotalMilliseconds >= 50);

        // Cleanup
        await _lifecycleManager.TearDownAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_serviceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            _serviceProvider?.Dispose();
        }
    }
}

/// <summary>
/// Mock test database for unit testing
/// </summary>
public class MockTestDatabase : ITestDatabase
{
    public GamificationEngineDbContext Context => throw new NotImplementedException("Mock database context not implemented for unit tests");

    public Task InitializeAsync() => Task.CompletedTask;
    public Task EnsureCreatedAsync() => Task.CompletedTask;
    public Task SeedAsync() => Task.CompletedTask;
    public Task CleanupAsync() => Task.CompletedTask;
    public IServiceProvider GetServiceProvider() => throw new NotImplementedException();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}