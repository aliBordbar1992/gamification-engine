using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using GamificationEngine.Integration.Tests.Infrastructure.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

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
        _serviceProvider.GetService<ITestLifecycleManager>().ShouldNotBeNull();
        _serviceProvider.GetService<ITestDataManager>().ShouldNotBeNull();
        _serviceProvider.GetService<ITestTimingUtilities>().ShouldNotBeNull();
    }

    [Fact]
    public async Task TestLifecycleManager_ShouldManageTestState()
    {
        // Act
        await _lifecycleManager.SetUpAsync();
        var state = _lifecycleManager.GetCurrentState();

        // Assert
        state.ShouldNotBeNull();
        state.TestId.ShouldNotBeEmpty();
        state.CurrentPhase.ShouldBe(TestExecutionPhase.Running);
        state.TestStartTime.ShouldBeGreaterThan(DateTime.MinValue);

        // Act
        await _lifecycleManager.TearDownAsync();
        state = _lifecycleManager.GetCurrentState();

        // Assert
        state.CurrentPhase.ShouldBe(TestExecutionPhase.Completed);
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
        state.TestId.ShouldBe(testId);
        state.IsIsolated.ShouldBeTrue();
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
        @event.ShouldNotBeNull();
        @event.UserId.ShouldBe("user-1");
        @event.EventType.ShouldBe("TEST_EVENT");

        events.Count.ShouldBe(3);
        events.ShouldAllBe(e => e.UserId == "user-2");
        events.ShouldAllBe(e => e.EventType == "BATCH_EVENT");

        mixedEvents.Count.ShouldBe(5);
        mixedEvents.Count(e => e.EventType == "TYPE_A").ShouldBe(2);
        mixedEvents.Count(e => e.EventType == "TYPE_B").ShouldBe(3);
    }

    [Fact]
    public void TestDataManager_ShouldCreateTestUserStates()
    {
        // Act
        var userState = _dataManager.CreateUserState("user-1",
            new Dictionary<string, long> { { "XP", 100 }, { "Coins", 50 } },
            new List<string> { "Badge1", "Badge2" });

        // Assert
        userState.ShouldNotBeNull();
        userState.UserId.ShouldBe("user-1");
        userState.PointsByCategory["XP"].ShouldBe(100);
        userState.PointsByCategory["Coins"].ShouldBe(50);
        userState.Badges.ShouldContain("Badge1");
        userState.Badges.ShouldContain("Badge2");
    }

    [Fact]
    public void TestDataManager_ShouldCreateFixtures()
    {
        // Act
        var basicFixture = _dataManager.CreateFixture("basic_user");
        var powerFixture = _dataManager.CreateFixture("power_user");
        var customFixture = _dataManager.CreateFixture("custom_fixture");

        // Assert
        basicFixture.ShouldNotBeNull();
        basicFixture.Name.ShouldBe("Basic User");
        basicFixture.Events.ShouldNotBeEmpty();
        basicFixture.UserStates.ShouldNotBeEmpty();

        powerFixture.ShouldNotBeNull();
        powerFixture.Name.ShouldBe("Power User");
        powerFixture.Events.Count.ShouldBeGreaterThan(basicFixture.Events.Count);

        customFixture.ShouldNotBeNull();
        customFixture.Name.ShouldBe("custom_fixture");
    }

    [Fact]
    public void TestDataManager_ShouldValidateTestData()
    {
        // Arrange
        var userState = _dataManager.CreateUserState("user-1");

        // Act & Assert
        _dataManager.ValidateTestData(userState, u => u.UserId == "user-1").ShouldBeTrue();
        _dataManager.ValidateTestData(userState, u => u.UserId == "user-2").ShouldBeFalse();
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
        syncTime.TotalMilliseconds.ShouldBeGreaterThanOrEqualTo(100);
        asyncTime.TotalMilliseconds.ShouldBeGreaterThanOrEqualTo(100);
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
        result.ShouldBeTrue();
        conditionMet.ShouldBeTrue();
    }

    [Fact]
    public async Task TestTimingUtilities_ShouldCreateTimer()
    {
        // Arrange
        var timer = _timingUtilities.CreateTimer(TimeSpan.FromMilliseconds(100));

        // Assert
        timer.HasExpired.ShouldBeFalse();
        timer.RemainingTime.ShouldBeGreaterThan(TimeSpan.Zero);

        // Act
        await Task.Delay(150);

        // Assert
        timer.HasExpired.ShouldBeTrue();
        timer.RemainingTime.ShouldBe(TimeSpan.Zero);
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
        allCompleted.ShouldBeTrue();
        anyCompleted.ShouldNotBeNull();
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
        state.TestName.ShouldBe("IntegrationTest");
        state.TestClassName.ShouldBe("TestInfrastructureAbstractionsTests");
        state.Metadata["testType"].ShouldBe("integration");
        state.TestData["events"].ShouldNotBeNull();
        state.TestData["userState"].ShouldNotBeNull();
        executionTime.TotalMilliseconds.ShouldBeGreaterThanOrEqualTo(50);

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