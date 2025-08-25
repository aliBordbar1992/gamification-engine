# Performance Testing Infrastructure

This directory contains the performance testing infrastructure for the Gamification Engine integration tests. It provides comprehensive tools for load testing, stress testing, baseline testing, and test execution monitoring.

## Overview

The performance testing infrastructure consists of several key components:

- **PerformanceTestHarness**: Provides load testing, stress testing, and baseline testing capabilities
- **TestExecutionMonitor**: Monitors individual test execution times and generates performance reports
- **PerformanceTestingConfiguration**: Configuration extensions for performance testing services
- **Test Models**: Data transfer objects for test configuration and results

## Features

### 1. Load Testing
- **Concurrent Request Simulation**: Simulate multiple users making requests simultaneously
- **Configurable Concurrency**: Set the number of concurrent users
- **Duration-based Testing**: Run tests for a specified duration
- **Request Timeout Handling**: Configure timeouts for individual requests
- **Success Rate Tracking**: Monitor success/failure rates across all requests

### 2. Stress Testing
- **Gradual Load Increase**: Start with low concurrency and gradually increase
- **Breaking Point Detection**: Automatically detect when the system starts failing
- **Configurable Steps**: Control the rate of concurrency increase
- **Success Rate Thresholds**: Set minimum acceptable success rates

### 3. Baseline Testing
- **Performance Benchmarking**: Establish baseline performance metrics
- **Multiple Iterations**: Run tests multiple times for statistical accuracy
- **Progress Tracking**: Monitor test execution progress
- **Statistical Analysis**: Calculate percentiles and averages

### 4. Test Execution Monitoring
- **Individual Test Tracking**: Monitor execution time for each test
- **Category-based Grouping**: Group tests by category for analysis
- **Performance Reports**: Generate comprehensive performance reports
- **Recommendations**: Get automated recommendations for performance improvements

## Usage Examples

### Basic Load Test

```csharp
[Fact]
public async Task Should_Handle_Load()
{
    var options = new LoadTestOptions
    {
        Concurrency = 10,
        Duration = TimeSpan.FromMinutes(1),
        RequestTimeout = TimeSpan.FromSeconds(30)
    };

    var result = await _performanceHarness.RunLoadTestAsync(
        async () =>
        {
            // Your test action here
            var response = await _httpClient.GetAsync("/api/events");
            response.EnsureSuccessStatusCode();
        },
        options);

    // Assertions
    result.SuccessRate.ShouldBeGreaterThan(0.95);
    result.AverageResponseTime.ShouldBeLessThan(TimeSpan.FromSeconds(1));
}
```

### Stress Test

```csharp
[Fact]
public async Task Should_Handle_Stress()
{
    var options = new StressTestOptions
    {
        InitialConcurrency = 1,
        MaxConcurrency = 50,
        ConcurrencyStep = 5,
        StepDuration = TimeSpan.FromSeconds(30),
        SuccessRateThreshold = 0.9
    };

    var result = await _performanceHarness.RunStressTestAsync(
        async () =>
        {
            // Your test action here
            var response = await _httpClient.PostAsync("/api/events", content);
            response.EnsureSuccessStatusCode();
        },
        options);

    // Assertions
    result.BreakingPoint.ShouldBeGreaterThan(10);
    result.OverallSuccessRate.ShouldBeGreaterThan(0.8);
}
```

### Baseline Test

```csharp
[Fact]
public async Task Should_Establish_Baseline()
{
    var options = new BaselineTestOptions
    {
        Iterations = 100,
        DelayBetweenIterations = TimeSpan.FromMilliseconds(100)
    };

    var baseline = await _performanceHarness.RunBaselineTestAsync(
        async () =>
        {
            // Your test action here
            await _service.ProcessEventAsync(testEvent);
        },
        options);

    // Store baseline for future comparison
    _baselineStorage.Store("event_processing", baseline);
}
```

### Test Execution Monitoring

```csharp
[Fact]
public async Task Should_Monitor_Execution()
{
    using var tracker = _executionMonitor.StartTestExecution("MyTest", "Integration");
    
    try
    {
        // Your test logic here
        await _service.ProcessEventAsync(testEvent);
        
        tracker.MarkSuccess();
    }
    catch (Exception ex)
    {
        tracker.MarkFailure(ex);
        throw;
    }
}
```

### Performance Comparison

```csharp
[Fact]
public async Task Should_Maintain_Performance()
{
    // Get stored baseline
    var baseline = _baselineStorage.Get("event_processing");
    
    // Run current test
    var currentResult = await _performanceHarness.RunLoadTestAsync(
        async () => await _service.ProcessEventAsync(testEvent),
        new LoadTestOptions { Concurrency = 5, Duration = TimeSpan.FromSeconds(10) });
    
    // Compare against baseline
    var comparison = _performanceHarness.CompareAgainstBaseline(baseline, currentResult);
    
    // Assertions
    comparison.IsWithinTolerance.ShouldBeTrue();
    comparison.ResponseTimeChangePercentage.ShouldBeLessThan(20); // Within 20%
}
```

## Configuration

### Test Settings

Add performance testing configuration to your `appsettings.Testing.json`:

```json
{
  "TestSettings": {
    "EnablePerformanceTesting": true,
    "EnableLoadTesting": true,
    "EnableStressTesting": true,
    "EnableBaselineTesting": true,
    "EnableTestExecutionMonitoring": true
  }
}
```

### Service Registration

```csharp
var services = new ServiceCollection();

// Add test monitoring
services.AddTestMonitoring(new TestMonitoringOptions
{
    EnableMetricsCollection = true,
    EnablePerformanceMonitoring = true,
    EnableTestExecutionTracking = true
});

// Add performance testing
services.AddPerformanceTesting(options =>
{
    options.EnableLoadTesting = true;
    options.EnableStressTesting = true;
    options.DefaultLoadTestConcurrency = 10;
    options.DefaultLoadTestDuration = TimeSpan.FromMinutes(1);
    options.Thresholds.MaxResponseTimeMs = 1000;
    options.Thresholds.MinSuccessRate = 0.95;
});
```

## Performance Thresholds

The infrastructure includes configurable performance thresholds:

- **MaxResponseTimeMs**: Maximum acceptable response time (default: 1000ms)
- **MaxTestExecutionTimeMs**: Maximum acceptable test execution time (default: 30000ms)
- **MinSuccessRate**: Minimum acceptable success rate (default: 0.95)
- **MaxMemoryUsageMb**: Maximum acceptable memory usage (default: 512MB)
- **MaxCpuUsagePercent**: Maximum acceptable CPU usage (default: 80%)

## Metrics and Reporting

### Metrics Collection

The infrastructure automatically collects:

- Request durations and success rates
- Test execution times
- Memory and CPU usage (when available)
- Error rates and exception details

### Performance Reports

Generate comprehensive reports including:

- Overall performance summary
- Category-based statistics
- Individual test performance
- Automated recommendations
- Performance trends over time

### Real-time Monitoring

Enable real-time monitoring for:

- Live performance metrics
- Immediate failure detection
- Performance alerts
- Resource usage tracking

## Best Practices

### 1. Test Isolation
- Each performance test should be independent
- Use unique test data for each run
- Clean up resources after tests

### 2. Realistic Scenarios
- Test with realistic data volumes
- Simulate actual user behavior patterns
- Include edge cases and error conditions

### 3. Baseline Management
- Establish baselines in controlled environments
- Store baselines for regression testing
- Update baselines when making intentional changes

### 4. Resource Monitoring
- Monitor memory usage during tests
- Track CPU utilization
- Watch for resource leaks

### 5. Test Data Management
- Use appropriate data sizes for testing
- Clean up test data after tests
- Avoid sharing test data between tests

## Troubleshooting

### Common Issues

1. **High Memory Usage**
   - Check for memory leaks in test code
   - Reduce test data size
   - Increase garbage collection frequency

2. **Slow Test Execution**
   - Check database performance
   - Verify network latency
   - Review test complexity

3. **Inconsistent Results**
   - Ensure test isolation
   - Check for external dependencies
   - Verify test data consistency

### Debug Mode

Enable detailed logging:

```json
{
  "Logging": {
    "DefaultLevel": "Debug",
    "EnableDetailedLogging": true
  }
}
```

## Integration with CI/CD

### Automated Performance Testing

```yaml
# GitHub Actions example
- name: Run Performance Tests
  run: |
    dotnet test --filter "Category=Performance" --logger "trx;LogFileName=performance.xml"
  
- name: Generate Performance Report
  run: |
    dotnet run --project tools/PerformanceReporter --input performance.xml --output report.html
```

### Performance Gates

Set up performance gates in your pipeline:

```yaml
- name: Check Performance Thresholds
  run: |
    dotnet run --project tools/PerformanceValidator --baseline baseline.json --current performance.xml
```

## Future Enhancements

- **Distributed Load Testing**: Support for distributed test execution
- **Advanced Metrics**: Integration with APM tools
- **Performance Regression Detection**: Automated detection of performance regressions
- **Load Profile Simulation**: More realistic user behavior simulation
- **Cloud Integration**: Support for cloud-based load testing services 