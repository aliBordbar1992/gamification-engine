using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using System.Diagnostics;

namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Implementation of test operation monitor
/// </summary>
public class TestOperationMonitor : ITestOperationMonitor
{
    private readonly TestPerformanceMonitor _monitor;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    public TestOperationMonitor(TestPerformanceMonitor monitor, string operationName)
    {
        _monitor = monitor;
        _operationName = operationName;
        _stopwatch = Stopwatch.StartNew();
    }

    public void MarkSuccess()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordOperation(_operationName, _stopwatch.Elapsed, true);
        }
    }

    public void MarkFailure()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordOperation(_operationName, _stopwatch.Elapsed, false);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _monitor.RecordOperation(_operationName, _stopwatch.Elapsed, true);
            _disposed = true;
        }
    }
}