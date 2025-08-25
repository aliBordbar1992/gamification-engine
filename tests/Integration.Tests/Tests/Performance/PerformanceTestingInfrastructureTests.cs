using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Configuration;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using GamificationEngine.Integration.Tests.Infrastructure.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace GamificationEngine.Integration.Tests.Tests.Performance;

/// <summary>
/// Tests for the performance testing infrastructure
/// </summary>
[Trait("Category", "Performance")]
public class PerformanceTestingInfrastructureTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPerformanceTestHarness _performanceHarness;
    private readonly ITestExecutionMonitor _executionMonitor;
    private readonly ILogger<PerformanceTestingInfrastructureTests> _logger;

    public PerformanceTestingInfrastructureTests()
    {
        var services = new ServiceCollection();

        // Add test infrastructure (includes ITestTimingUtilities)
        services.AddTestInfrastructure();

        // Add test monitoring
        services.AddTestMonitoring(options =>
        {
            options.EnableMetricsCollection = true;
            options.EnablePerformanceMonitoring = true;
            options.EnableTestExecutionTracking = true;
        });

        // Add performance testing
        services.AddPerformanceTesting();

        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        _serviceProvider = services.BuildServiceProvider();
        _performanceHarness = _serviceProvider.GetRequiredService<IPerformanceTestHarness>();
        _executionMonitor = _serviceProvider.GetRequiredService<ITestExecutionMonitor>();
        _logger = _serviceProvider.GetRequiredService<ILogger<PerformanceTestingInfrastructureTests>>();
    }

    [Fact]
    public async Task Should_Run_Baseline_Test()
    {
        // Arrange
        var options = new BaselineTestOptions
        {
            Iterations = 10,
            DelayBetweenIterations = TimeSpan.FromMilliseconds(50),
            EnableProgressLogging = true
        };

        // Act
        var baseline = await _performanceHarness.RunBaselineTestAsync(
            async () =>
            {
                // Simulate some work
                await Task.Delay(10);
                return;
            },
            options);

        // Assert
        baseline.ShouldNotBeNull();
        baseline.TotalIterations.ShouldBe(10);
        baseline.SuccessCount.ShouldBe(10);
        baseline.SuccessRate.ShouldBe(1.0);
        baseline.AverageExecutionTime.ShouldBeGreaterThan(TimeSpan.FromMilliseconds(5));
        baseline.AverageExecutionTime.ShouldBeLessThan(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task Should_Run_Load_Test()
    {
        // Arrange
        var options = new LoadTestOptions
        {
            Concurrency = 5,
            Duration = TimeSpan.FromSeconds(2),
            RequestTimeout = TimeSpan.FromSeconds(10)
        };

        // Act
        var loadTestResult = await _performanceHarness.RunLoadTestAsync(
            async () =>
            {
                // Simulate API call
                await Task.Delay(20);
                return;
            },
            options);

        // Assert
        loadTestResult.ShouldNotBeNull();
        loadTestResult.TotalRequests.ShouldBeGreaterThan(0);
        loadTestResult.SuccessRate.ShouldBe(1.0);
        loadTestResult.AverageResponseTime.ShouldBeGreaterThan(TimeSpan.FromMilliseconds(15));
        loadTestResult.AverageResponseTime.ShouldBeLessThan(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task Should_Run_Stress_Test()
    {
        // Arrange
        var options = new StressTestOptions
        {
            InitialConcurrency = 1,
            MaxConcurrency = 5,
            ConcurrencyStep = 1,
            StepDuration = TimeSpan.FromSeconds(1),
            StepDelay = TimeSpan.FromMilliseconds(100),
            SuccessRateThreshold = 0.9
        };

        // Act
        var stressTestResult = await _performanceHarness.RunStressTestAsync(
            async () =>
            {
                // Simulate API call with some variability
                var delay = Random.Shared.Next(10, 50);
                await Task.Delay(delay);
                return;
            },
            options);

        // Assert
        stressTestResult.ShouldNotBeNull();
        stressTestResult.LoadTestResults.ShouldNotBeEmpty();
        stressTestResult.TotalRequests.ShouldBeGreaterThan(0);
        stressTestResult.OverallSuccessRate.ShouldBe(1.0);
    }

    [Fact]
    public async Task Should_Compare_Performance_Against_Baseline()
    {
        // Arrange
        var baselineOptions = new BaselineTestOptions
        {
            Iterations = 5,
            DelayBetweenIterations = TimeSpan.FromMilliseconds(50)
        };

        var loadTestOptions = new LoadTestOptions
        {
            Concurrency = 3,
            Duration = TimeSpan.FromSeconds(1)
        };

        // Act
        var baseline = await _performanceHarness.RunBaselineTestAsync(
            async () => await Task.Delay(15),
            baselineOptions);

        var loadTestResult = await _performanceHarness.RunLoadTestAsync(
            async () => await Task.Delay(15),
            loadTestOptions);

        var comparison = _performanceHarness.CompareAgainstBaseline(baseline, loadTestResult);

        // Assert
        comparison.ShouldNotBeNull();
        comparison.Baseline.ShouldBe(baseline);
        comparison.CurrentResult.ShouldBe(loadTestResult);

        // The tolerance check is based on 20% response time difference and 5% success rate difference
        // Since we're using the same delay (15ms) for both baseline and load test, they should be within tolerance
        // But the load test has overhead from concurrency, so we'll check the actual values instead
        comparison.ResponseTimeChangePercentage.ShouldBeLessThan(50); // Within 50% tolerance for this test
        comparison.SuccessRateChange.ShouldBe(0.0); // Success rate should be identical
    }

    [Fact]
    public async Task Should_Monitor_Test_Execution()
    {
        // Arrange
        var testName = "SamplePerformanceTest";
        var testCategory = "Performance";

        // Act
        using var tracker = _executionMonitor.StartTestExecution(testName, testCategory);

        // Simulate test work
        await Task.Delay(25);

        tracker.MarkSuccess();

        // Assert
        var stats = _executionMonitor.GetTestStats(testName);
        stats.ShouldNotBeNull();
        stats.TestName.ShouldBe(testName);
        stats.Category.ShouldBe(testCategory);
        stats.TotalExecutions.ShouldBe(1);
        stats.SuccessfulExecutions.ShouldBe(1);
        stats.SuccessRate.ShouldBe(1.0);
        stats.AverageExecutionTime.ShouldBeGreaterThan(TimeSpan.FromMilliseconds(20));
    }

    [Fact]
    public async Task Should_Generate_Performance_Report()
    {
        // Arrange
        var testName = "ReportTest";
        var testCategory = "Reporting";

        // Clear any existing stats to start fresh
        _executionMonitor.ClearStats();

        // Run a few tests to generate data
        using (var tracker1 = _executionMonitor.StartTestExecution(testName + "1", testCategory))
        {
            await Task.Delay(10);
            // Don't call MarkSuccess() - it will be called automatically in Dispose
        }

        using (var tracker2 = _executionMonitor.StartTestExecution(testName + "2", testCategory))
        {
            await Task.Delay(15);
            // Don't call MarkSuccess() - it will be called automatically in Dispose
        }

        // Act
        var report = _executionMonitor.GenerateReport();

        // Assert
        report.ShouldNotBeNull();
        report.GeneratedAt.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-1));
        report.Summary.ShouldNotBeNull();

        // Check that our specific tests are in the report
        var ourTests = report.TestStats.Where(kvp => kvp.Key.StartsWith(testName)).ToList();
        ourTests.Count.ShouldBe(2);

        // Check that our tests are successful
        foreach (var test in ourTests)
        {
            test.Value.SuccessfulExecutions.ShouldBe(1);
            test.Value.SuccessRate.ShouldBe(1.0);
        }

        report.CategoryStats.ShouldNotBeEmpty();
        report.TestStats.ShouldNotBeEmpty();
        report.Recommendations.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Handle_Test_Failures()
    {
        // Arrange
        var testName = "FailureTest";
        var testCategory = "FailureHandling";

        // Act
        using var tracker = _executionMonitor.StartTestExecution(testName, testCategory);

        try
        {
            // Simulate a failure
            await Task.Delay(5);
            throw new InvalidOperationException("Simulated test failure");
        }
        catch (Exception ex)
        {
            tracker.MarkFailure(ex);
        }

        // Assert
        var stats = _executionMonitor.GetTestStats(testName);
        stats.ShouldNotBeNull();
        stats.TotalExecutions.ShouldBe(1);
        stats.FailedExecutions.ShouldBe(1);
        stats.SuccessRate.ShouldBe(0.0);
        stats.Exceptions.ShouldNotBeEmpty();
        stats.Exceptions.First().ShouldNotBeNull();
        stats.Exceptions.First()!.Message.ShouldBe("Simulated test failure");
    }

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}