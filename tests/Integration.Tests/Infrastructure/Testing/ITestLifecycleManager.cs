namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Interface for managing test lifecycle operations including setup, teardown, and state management
/// </summary>
public interface ITestLifecycleManager
{
    /// <summary>
    /// Performs test setup operations before each test method
    /// </summary>
    Task SetUpAsync();

    /// <summary>
    /// Performs test cleanup operations after each test method
    /// </summary>
    Task TearDownAsync();

    /// <summary>
    /// Resets the test environment to a clean state
    /// </summary>
    Task ResetAsync();

    /// <summary>
    /// Performs global test suite setup (called once before all tests)
    /// </summary>
    Task SetUpSuiteAsync();

    /// <summary>
    /// Performs global test suite cleanup (called once after all tests)
    /// </summary>
    Task TearDownSuiteAsync();

    /// <summary>
    /// Gets the current test state information
    /// </summary>
    TestStateInfo GetCurrentState();

    /// <summary>
    /// Sets up test data isolation for parallel test execution
    /// </summary>
    Task SetUpIsolationAsync(string testId);

    /// <summary>
    /// Sets the current test name and class name
    /// </summary>
    void SetTestInfo(string testName, string testClassName);

    /// <summary>
    /// Adds test data to the current test state
    /// </summary>
    void AddTestData(string key, object value);

    /// <summary>
    /// Adds metadata to the current test state
    /// </summary>
    void AddMetadata(string key, string value);
}