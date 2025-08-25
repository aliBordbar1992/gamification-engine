using System.Diagnostics;
using System.Text;

namespace GamificationEngine.Integration.Tests.Infrastructure.Utils;

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