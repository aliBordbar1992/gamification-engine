using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Manages test lifecycle operations including setup, teardown, and state management
/// </summary>
public class TestLifecycleManager : ITestLifecycleManager
{
    private readonly ITestDatabase _testDatabase;
    private readonly ILogger<TestLifecycleManager> _logger;
    private readonly TestStateInfo _currentState;
    private readonly Dictionary<string, object> _suiteData;

    public TestLifecycleManager(ITestDatabase testDatabase, ILogger<TestLifecycleManager> logger)
    {
        _testDatabase = testDatabase ?? throw new ArgumentNullException(nameof(testDatabase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentState = new TestStateInfo();
        _suiteData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Performs test setup operations before each test method
    /// </summary>
    public async Task SetUpAsync()
    {
        try
        {
            _currentState.CurrentPhase = TestExecutionPhase.SettingUp;
            _currentState.TestStartTime = DateTime.UtcNow;
            _currentState.TestId = Guid.NewGuid().ToString();

            _logger.LogInformation("Setting up test {TestId} at {StartTime}",
                _currentState.TestId, _currentState.TestStartTime);

            // Initialize the test database
            await _testDatabase.InitializeAsync();
            await _testDatabase.EnsureCreatedAsync();

            // Clear any existing data
            await _testDatabase.CleanupAsync();

            // Seed with fresh test data
            await _testDatabase.SeedAsync();

            _currentState.CurrentPhase = TestExecutionPhase.Running;
            _logger.LogInformation("Test {TestId} setup completed successfully", _currentState.TestId);
        }
        catch (Exception ex)
        {
            _currentState.CurrentPhase = TestExecutionPhase.Failed;
            _logger.LogError(ex, "Failed to set up test {TestId}", _currentState.TestId);
            throw;
        }
    }

    /// <summary>
    /// Performs test cleanup operations after each test method
    /// </summary>
    public async Task TearDownAsync()
    {
        try
        {
            _currentState.CurrentPhase = TestExecutionPhase.TearingDown;
            _logger.LogInformation("Tearing down test {TestId}", _currentState.TestId);

            // Clean up test data
            await _testDatabase.CleanupAsync();

            // Clear test-specific data
            _currentState.TestData.Clear();

            _currentState.CurrentPhase = TestExecutionPhase.Completed;
            _logger.LogInformation("Test {TestId} teardown completed successfully", _currentState.TestId);
        }
        catch (Exception ex)
        {
            _currentState.CurrentPhase = TestExecutionPhase.Failed;
            _logger.LogError(ex, "Failed to tear down test {TestId}", _currentState.TestId);
            throw;
        }
    }

    /// <summary>
    /// Resets the test environment to a clean state
    /// </summary>
    public async Task ResetAsync()
    {
        try
        {
            _logger.LogInformation("Resetting test environment for test {TestId}", _currentState.TestId);

            // Clean up all data
            await _testDatabase.CleanupAsync();

            // Reset state
            _currentState.TestData.Clear();
            _currentState.Metadata.Clear();

            // Re-seed with fresh data
            await _testDatabase.SeedAsync();

            _logger.LogInformation("Test environment reset completed for test {TestId}", _currentState.TestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset test environment for test {TestId}", _currentState.TestId);
            throw;
        }
    }

    /// <summary>
    /// Performs global test suite setup (called once before all tests)
    /// </summary>
    public async Task SetUpSuiteAsync()
    {
        try
        {
            _logger.LogInformation("Setting up test suite");

            // Initialize suite-level resources
            _suiteData.Clear();

            // Set up database infrastructure
            await _testDatabase.InitializeAsync();

            _logger.LogInformation("Test suite setup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set up test suite");
            throw;
        }
    }

    /// <summary>
    /// Performs global test suite cleanup (called once after all tests)
    /// </summary>
    public async Task TearDownSuiteAsync()
    {
        try
        {
            _logger.LogInformation("Tearing down test suite");

            // Clean up suite-level resources
            _suiteData.Clear();

            // Clean up database
            await _testDatabase.CleanupAsync();

            _logger.LogInformation("Test suite teardown completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to tear down test suite");
            throw;
        }
    }

    /// <summary>
    /// Gets the current test state information
    /// </summary>
    public TestStateInfo GetCurrentState()
    {
        return new TestStateInfo
        {
            TestId = _currentState.TestId,
            TestName = _currentState.TestName,
            TestClassName = _currentState.TestClassName,
            TestStartTime = _currentState.TestStartTime,
            CurrentPhase = _currentState.CurrentPhase,
            DatabaseConnection = _currentState.DatabaseConnection,
            TestData = new Dictionary<string, object>(_currentState.TestData),
            IsIsolated = _currentState.IsIsolated,
            Metadata = new Dictionary<string, string>(_currentState.Metadata)
        };
    }

    /// <summary>
    /// Sets up test data isolation for parallel test execution
    /// </summary>
    public async Task SetUpIsolationAsync(string testId)
    {
        try
        {
            _logger.LogInformation("Setting up isolation for test {TestId}", testId);

            _currentState.TestId = testId;
            _currentState.IsIsolated = true;

            // Create isolated database context
            await _testDatabase.InitializeAsync();
            await _testDatabase.EnsureCreatedAsync();

            // Clear any existing data
            await _testDatabase.CleanupAsync();

            // Seed with isolated test data
            await _testDatabase.SeedAsync();

            _logger.LogInformation("Isolation setup completed for test {TestId}", testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set up isolation for test {TestId}", testId);
            throw;
        }
    }

    /// <summary>
    /// Sets the current test name and class name
    /// </summary>
    public void SetTestInfo(string testName, string testClassName)
    {
        _currentState.TestName = testName;
        _currentState.TestClassName = testClassName;
    }

    /// <summary>
    /// Adds test data to the current test state
    /// </summary>
    public void AddTestData(string key, object value)
    {
        _currentState.TestData[key] = value;
    }

    /// <summary>
    /// Adds metadata to the current test state
    /// </summary>
    public void AddMetadata(string key, string value)
    {
        _currentState.Metadata[key] = value;
    }

    /// <summary>
    /// Gets test data from the current test state
    /// </summary>
    public T? GetTestData<T>(string key)
    {
        if (_currentState.TestData.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }
}