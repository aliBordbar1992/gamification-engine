using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using System.Diagnostics;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Implementation of test execution tracker
/// </summary>
public class TestExecutionTracker : ITestExecutionTracker
{
    private readonly TestExecutionMonitor _monitor;
    private readonly string _testName;
    private readonly string? _testCategory;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    public TestExecutionTracker(TestExecutionMonitor monitor, string testName, string? testCategory)
    {
        _monitor = monitor;
        _testName = testName;
        _testCategory = testCategory;
        _stopwatch = Stopwatch.StartNew();
    }

    public void MarkSuccess()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordTestExecution(_testName, _testCategory, _stopwatch.Elapsed, true);
        }
    }

    public void MarkFailure(Exception? exception = null)
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordTestExecution(_testName, _testCategory, _stopwatch.Elapsed, false, exception);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordTestExecution(_testName, _testCategory, _stopwatch.Elapsed, true);
            _disposed = true;
        }
    }
}