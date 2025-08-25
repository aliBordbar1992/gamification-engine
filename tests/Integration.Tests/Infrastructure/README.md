# Integration Test Infrastructure

This directory contains the infrastructure components for running integration tests against the Gamification Engine API.

## Overview

The integration test infrastructure provides:
- Test database management (InMemory and PostgreSQL)
- Test configuration management
- Test logging and monitoring
- Test data builders and utilities
- HTTP client factories for API testing

## Test Configuration Management

### TestConfigurationManager

The `TestConfigurationManager` class provides centralized configuration management for tests:

- Loads test-specific configuration from `appsettings.Testing.json`
- Supports environment variable overrides with `TEST_` prefix
- Provides strongly-typed access to configuration sections
- Validates configuration requirements
- Supports runtime configuration overrides

### TestLoggingConfiguration

The `TestLoggingConfiguration` class configures test-specific logging:

- Configurable log levels for different components
- Structured logging support
- Custom test logging provider that captures log entries
- Console output with color-coded log levels
- Debug logging for development

### TestMonitoringConfiguration

The `TestMonitoringConfiguration` class provides test monitoring and metrics:

- Metrics collection for test operations
- Performance monitoring with timing statistics
- Test execution tracking
- Configurable monitoring options
- Real-time metrics reporting

### TestConfigurationUtilities

The `TestConfigurationUtilities` class provides helper methods:

- Test environment setup and cleanup
- Configuration validation
- Scenario-based configuration creation
- Test service configuration
- Host builder creation for testing

## Configuration Files

### appsettings.Testing.json

Base test configuration file containing:
- Test settings (database provider, logging, timeouts)
- Database connection strings
- Event queue configuration
- Event retention settings
- Logging configuration

### Environment Variables

Test configuration can be overridden using environment variables with the `TEST_` prefix:

```bash
TEST_DATABASE_PROVIDER=PostgreSQL
TEST_ENABLE_DETAILED_LOGGING=true
TEST_TIMEOUT_SECONDS=60
```

## Usage Examples

### Basic Test Configuration

```csharp
public class MyIntegrationTest : IntegrationTestBase
{
    public MyIntegrationTest()
    {
        // Configuration is automatically loaded and services are configured
    }
    
    [Fact]
    public async Task Should_Test_Something()
    {
        // Use ConfigurationManager to access test settings
        var dbProvider = ConfigurationManager.TestSettings.DatabaseProvider;
        
        // Use monitoring services if available
        if (PerformanceMonitor != null)
        {
            using var operation = PerformanceMonitor.StartOperation("test_operation");
            // ... test logic ...
            operation.MarkSuccess();
        }
    }
}
```

### Custom Configuration

```csharp
var config = TestConfigurationUtilities.CreateScenarioConfiguration("performance", new Dictionary<string, string>
{
    ["TestSettings:EnableParallelExecution"] = "true",
    ["EventQueue:MaxConcurrentProcessing"] = "4"
});
```

### Test Service Configuration

```csharp
var services = new ServiceCollection();
services.ConfigureTestServices(configuration);
services.AddTestMonitoring(options =>
{
    options.EnableMetricsCollection = true;
    options.EnablePerformanceMonitoring = true;
});
```

## Test Database Infrastructure

### Supported Providers

- **InMemory**: Fast, no external dependencies, suitable for unit tests
- **PostgreSQL**: Full database testing, suitable for integration tests

### Database Selection

The database provider is configured via:
1. `appsettings.Testing.json` - `TestSettings:DatabaseProvider`
2. Environment variable - `TEST_DATABASE_PROVIDER`
3. Runtime configuration override

### Database Management

```csharp
// Get database from factory
var database = TestDatabaseFactory.CreateFromConfiguration(Services);

// Use specific provider
var inMemoryDb = TestDatabaseFactory.CreateInMemory(Services);
var postgresDb = TestDatabaseFactory.CreatePostgreSql(Services, connectionString);
```

## Test Data Management

### TestDataBuilder

The `TestDataBuilder` class provides utilities for creating test data:

- Event creation with realistic data
- User state generation
- Configuration data setup
- Random data generation for stress testing

### Test Assertion Utilities

The `TestAssertionUtilities` class provides common assertion patterns:

- JSON response validation
- Database state verification
- Event sequence validation
- Performance assertion helpers

## Best Practices

1. **Configuration**: Always use the `TestConfigurationManager` for accessing test settings
2. **Database**: Use appropriate database provider for test type (InMemory for unit, PostgreSQL for integration)
3. **Monitoring**: Enable monitoring for performance-critical tests
4. **Cleanup**: Ensure proper cleanup in test teardown
5. **Isolation**: Each test should have isolated data and state
6. **Logging**: Use appropriate log levels to avoid noise in test output

## Troubleshooting

### Common Issues

1. **Configuration Not Loading**: Check file paths and environment variables
2. **Database Connection Failures**: Verify connection strings and database availability
3. **Service Resolution Errors**: Ensure required services are registered
4. **Performance Issues**: Consider using InMemory database for faster tests

### Debug Mode

Enable detailed logging by setting:
```bash
TEST_ENABLE_DETAILED_LOGGING=true
TEST_LOGGING_DEFAULT_LEVEL=Debug
```

## Future Enhancements

- Support for additional database providers (SQL Server, MySQL)
- Advanced metrics collection and reporting
- Test result aggregation and analysis
- Performance benchmarking tools
- Configuration validation rules engine 