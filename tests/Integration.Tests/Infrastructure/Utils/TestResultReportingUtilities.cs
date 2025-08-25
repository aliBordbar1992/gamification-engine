using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using Shouldly;

namespace GamificationEngine.Integration.Tests.Infrastructure.Utils;

/// <summary>
/// Utility class providing test result reporting and debugging tools
/// </summary>
public static class TestResultReportingUtilities
{
    /// <summary>
    /// Captures detailed test execution context for debugging
    /// </summary>
    public class TestExecutionContext
    {
        public string TestName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public List<string> Steps { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, object> ContextData { get; set; } = new();
        public Stopwatch? Stopwatch { get; set; }

        public void AddStep(string step)
        {
            Steps.Add($"[{DateTime.Now:HH:mm:ss.fff}] {step}");
        }

        public void AddWarning(string warning)
        {
            Warnings.Add($"[{DateTime.Now:HH:mm:ss.fff}] WARNING: {warning}");
        }

        public void AddError(string error)
        {
            Errors.Add($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {error}");
        }

        public void AddContextData(string key, object value)
        {
            ContextData[key] = value;
        }

        public string GenerateReport()
        {
            var report = new StringBuilder();
            report.AppendLine($"=== Test Execution Report: {TestName} ===");
            report.AppendLine($"Duration: {Duration.TotalMilliseconds:F2}ms");
            report.AppendLine($"Start Time: {StartTime:yyyy-MM-dd HH:mm:ss.fff}");
            report.AppendLine($"End Time: {EndTime:yyyy-MM-dd HH:mm:ss.fff}");
            report.AppendLine();

            if (ContextData.Any())
            {
                report.AppendLine("=== Context Data ===");
                foreach (var kvp in ContextData)
                {
                    report.AppendLine($"{kvp.Key}: {kvp.Value}");
                }
                report.AppendLine();
            }

            if (Steps.Any())
            {
                report.AppendLine("=== Execution Steps ===");
                foreach (var step in Steps)
                {
                    report.AppendLine(step);
                }
                report.AppendLine();
            }

            if (Warnings.Any())
            {
                report.AppendLine("=== Warnings ===");
                foreach (var warning in Warnings)
                {
                    report.AppendLine(warning);
                }
                report.AppendLine();
            }

            if (Errors.Any())
            {
                report.AppendLine("=== Errors ===");
                foreach (var error in Errors)
                {
                    report.AppendLine(error);
                }
                report.AppendLine();
            }

            return report.ToString();
        }
    }

    /// <summary>
    /// Creates a new test execution context
    /// </summary>
    public static TestExecutionContext CreateTestContext(string testName)
    {
        return new TestExecutionContext
        {
            TestName = testName,
            StartTime = DateTime.Now,
            Stopwatch = Stopwatch.StartNew()
        };
    }

    /// <summary>
    /// Finalizes a test execution context and generates a report
    /// </summary>
    public static string FinalizeTestContext(TestExecutionContext context)
    {
        context.EndTime = DateTime.Now;
        context.Stopwatch?.Stop();

        return context.GenerateReport();
    }

    /// <summary>
    /// Logs test execution context to the provided logger
    /// </summary>
    public static void LogTestContext(ILogger logger, TestExecutionContext context, LogLevel level = LogLevel.Information)
    {
        var report = FinalizeTestContext(context);
        logger.Log(level, "Test Execution Report:\n{Report}", report);
    }

    /// <summary>
    /// Captures performance metrics for a test operation
    /// </summary>
    public static async Task<T> MeasureOperationAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        ILogger? logger = null,
        TestExecutionContext? context = null)
    {
        var stopwatch = Stopwatch.StartNew();
        context?.AddStep($"Starting operation: {operationName}");

        try
        {
            var result = await operation();
            stopwatch.Stop();

            var duration = stopwatch.Elapsed;
            context?.AddStep($"Completed operation: {operationName} in {duration.TotalMilliseconds:F2}ms");

            logger?.LogDebug("Operation '{OperationName}' completed in {Duration}ms",
                operationName, duration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed;

            context?.AddError($"Operation '{operationName}' failed after {duration.TotalMilliseconds:F2}ms: {ex.Message}");
            logger?.LogError(ex, "Operation '{OperationName}' failed after {Duration}ms",
                operationName, duration.TotalMilliseconds);

            throw;
        }
    }

    /// <summary>
    /// Captures performance metrics for a synchronous test operation
    /// </summary>
    public static T MeasureOperation<T>(
        string operationName,
        Func<T> operation,
        ILogger? logger = null,
        TestExecutionContext? context = null)
    {
        var stopwatch = Stopwatch.StartNew();
        context?.AddStep($"Starting operation: {operationName}");

        try
        {
            var result = operation();
            stopwatch.Stop();

            var duration = stopwatch.Elapsed;
            context?.AddStep($"Completed operation: {operationName} in {duration.TotalMilliseconds:F2}ms");

            logger?.LogDebug("Operation '{OperationName}' completed in {Duration}ms",
                operationName, duration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed;

            context?.AddError($"Operation '{operationName}' failed after {duration.TotalMilliseconds:F2}ms: {ex.Message}");
            logger?.LogError(ex, "Operation '{OperationName}' failed after {Duration}ms",
                operationName, duration.TotalMilliseconds);

            throw;
        }
    }

    /// <summary>
    /// Asserts that an operation completes within the expected time limit
    /// </summary>
    public static async Task AssertOperationCompletesWithinTimeLimitAsync(
        string operationName,
        Func<Task> operation,
        TimeSpan timeLimit,
        ILogger? logger = null,
        TestExecutionContext? context = null)
    {
        var stopwatch = Stopwatch.StartNew();
        context?.AddStep($"Starting timed operation: {operationName} (limit: {timeLimit.TotalMilliseconds:F2}ms)");

        try
        {
            await operation();
            stopwatch.Stop();

            var duration = stopwatch.Elapsed;
            context?.AddStep($"Completed timed operation: {operationName} in {duration.TotalMilliseconds:F2}ms");

            duration.TotalMilliseconds.ShouldBeLessThanOrEqualTo(timeLimit.TotalMilliseconds,
                $"Operation '{operationName}' should complete within {timeLimit.TotalMilliseconds:F2}ms, but took {duration.TotalMilliseconds:F2}ms");

            logger?.LogDebug("Operation '{OperationName}' completed in {Duration}ms (limit: {TimeLimit}ms)",
                operationName, duration.TotalMilliseconds, timeLimit.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed;

            context?.AddError($"Timed operation '{operationName}' failed after {duration.TotalMilliseconds:F2}ms: {ex.Message}");
            logger?.LogError(ex, "Timed operation '{OperationName}' failed after {Duration}ms",
                operationName, duration.TotalMilliseconds);

            throw;
        }
    }

    /// <summary>
    /// Generates a summary report for multiple test executions
    /// </summary>
    public static string GenerateTestSummaryReport(IEnumerable<TestExecutionContext> contexts)
    {
        var report = new StringBuilder();
        var contextsList = contexts.ToList();

        report.AppendLine("=== Test Execution Summary Report ===");
        report.AppendLine($"Total Tests: {contextsList.Count}");
        report.AppendLine($"Total Duration: {contextsList.Sum(c => c.Duration.TotalMilliseconds):F2}ms");
        report.AppendLine($"Average Duration: {contextsList.Average(c => c.Duration.TotalMilliseconds):F2}ms");
        report.AppendLine($"Min Duration: {contextsList.Min(c => c.Duration.TotalMilliseconds):F2}ms");
        report.AppendLine($"Max Duration: {contextsList.Max(c => c.Duration.TotalMilliseconds):F2}ms");
        report.AppendLine();

        var totalWarnings = contextsList.Sum(c => c.Warnings.Count);
        var totalErrors = contextsList.Sum(c => c.Errors.Count);

        report.AppendLine($"Total Warnings: {totalWarnings}");
        report.AppendLine($"Total Errors: {totalErrors}");
        report.AppendLine();

        if (totalErrors > 0)
        {
            report.AppendLine("=== Tests with Errors ===");
            foreach (var context in contextsList.Where(c => c.Errors.Any()))
            {
                report.AppendLine($"- {context.TestName}: {context.Errors.Count} errors");
            }
            report.AppendLine();
        }

        if (totalWarnings > 0)
        {
            report.AppendLine("=== Tests with Warnings ===");
            foreach (var context in contextsList.Where(c => c.Warnings.Any()))
            {
                report.AppendLine($"- {context.TestName}: {context.Warnings.Count} warnings");
            }
            report.AppendLine();
        }

        report.AppendLine("=== Performance Analysis ===");
        var slowTests = contextsList.OrderByDescending(c => c.Duration).Take(5);
        foreach (var context in slowTests)
        {
            report.AppendLine($"- {context.TestName}: {context.Duration.TotalMilliseconds:F2}ms");
        }

        return report.ToString();
    }

    /// <summary>
    /// Captures system information for debugging purposes
    /// </summary>
    public static Dictionary<string, object> CaptureSystemInfo()
    {
        return new Dictionary<string, object>
        {
            ["MachineName"] = Environment.MachineName,
            ["OSVersion"] = Environment.OSVersion.ToString(),
            ["ProcessorCount"] = Environment.ProcessorCount,
            ["WorkingSet"] = Environment.WorkingSet,
            ["Is64BitProcess"] = Environment.Is64BitProcess,
            ["Is64BitOperatingSystem"] = Environment.Is64BitOperatingSystem,
            ["CurrentDirectory"] = Environment.CurrentDirectory,
            ["UserName"] = Environment.UserName,
            ["DateTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        };
    }
}