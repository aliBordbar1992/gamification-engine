# Test Database Infrastructure

This directory contains the test database infrastructure for integration testing, supporting both InMemory and PostgreSQL database providers.

## Overview

The test database infrastructure provides:
- **Multiple database providers**: InMemory (fast) and PostgreSQL (real database testing)
- **Automatic setup/teardown**: Database initialization and cleanup for each test
- **Data seeding utilities**: Common test data creation and management
- **Factory pattern**: Easy creation of different database types
- **Configuration-driven**: Database selection via configuration files

## Components

### Core Interfaces and Classes

- **`ITestDatabase`**: Interface defining the contract for test databases
- **`TestDatabaseBase`**: Abstract base class with common functionality
- **`InMemoryTestDatabase`**: Fast in-memory database for unit-style integration tests
- **`PostgreSqlTestDatabase`**: Real PostgreSQL database for full integration testing
- **`TestDatabaseFactory`**: Factory for creating database instances
- **`TestDatabaseUtilities`**: Utilities for seeding, cleaning, and managing test data

### Database Types

#### InMemory Database
- **Use case**: Fast tests that don't require real database features
- **Pros**: No external dependencies, instant startup, isolated per test
- **Cons**: Limited SQL features, no real database behavior testing

#### PostgreSQL Database
- **Use case**: Full integration testing with real database behavior
- **Pros**: Real SQL features, actual database constraints, production-like behavior
- **Cons**: Requires PostgreSQL instance, slower startup, external dependency

## Usage

### Basic Usage

```csharp
public class MyIntegrationTest : IntegrationTestBase, IAsyncDisposable
{
    private ITestDatabase _testDatabase;

    public MyIntegrationTest()
    {
        _testDatabase = TestDatabaseFactory.CreateFromConfiguration(Services);
    }

    public async Task InitializeAsync()
    {
        await _testDatabase.InitializeAsync();
        await _testDatabase.SeedAsync();
    }

    [Fact]
    public async Task MyTest()
    {
        // Arrange
        var context = _testDatabase.Context;
        
        // Act & Assert
        // Your test logic here
    }

    public async ValueTask DisposeAsync()
    {
        if (_testDatabase is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
    }
}
```

### Explicit Database Type Selection

```csharp
// Create specific database type
var inMemoryDb = TestDatabaseFactory.CreateInMemory(Services);
var postgreSqlDb = TestDatabaseFactory.CreatePostgreSql(Services);

// Or by string
var database = TestDatabaseFactory.Create("PostgreSQL", Services);
```

### Data Seeding and Management

```csharp
// Seed common test data
await TestDatabaseUtilities.SeedCommonTestDataAsync(context);

// Seed specific test data
await TestDatabaseUtilities.SeedEventTestDataAsync(context, "user-1", 5);

// Clean up data
await TestDatabaseUtilities.CleanupAllTestDataAsync(context);

// Check database state
var (eventCount, userStateCount) = await TestDatabaseUtilities.GetEntityCountsAsync(context);
```

## Configuration

### Test Settings

Configure database selection in `appsettings.Testing.json`:

```json
{
  "TestSettings": {
    "DatabaseProvider": "InMemory",
    "UseInMemoryDatabase": true,
    "EnableDetailedLogging": true
  },
  "ConnectionStrings": {
    "TestPostgreSql": "Host=localhost;Port=5432;Database=gamification_test;Username=test_user;Password=test_password"
  }
}
```

### Environment Variables

Override settings with environment variables:

```bash
# Use PostgreSQL for testing
set TEST_DatabaseProvider=PostgreSQL
set TEST_ConnectionStrings__TestPostgreSql="Host=localhost;Port=5432;Database=gamification_test;Username=test_user;Password=test_password"

# Run tests
dotnet test
```

## Best Practices

### Test Isolation
- Each test should use a fresh database instance
- Clean up data after each test
- Use unique database names for parallel execution

### Performance
- Use InMemory for fast feedback during development
- Use PostgreSQL for CI/CD and pre-release testing
- Consider test data size and cleanup strategies

### Data Management
- Seed only the data needed for each test
- Use utilities for common data patterns
- Avoid hardcoded test data in individual tests

### Error Handling
- Always dispose of database resources
- Handle database connection failures gracefully
- Provide meaningful error messages for configuration issues

## Troubleshooting

### Common Issues

1. **Connection String Not Found**
   - Ensure `TestPostgreSql` connection string is configured
   - Check environment variable overrides

2. **Database Already Exists**
   - Use unique database names for parallel tests
   - Ensure proper cleanup between tests

3. **Permission Denied**
   - Verify PostgreSQL user has necessary permissions
   - Check database creation rights

4. **Test Data Persistence**
   - Ensure `CleanupAsync()` is called
   - Check for transaction rollback issues

### Debugging

Enable detailed logging in test configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

## Future Enhancements

- **SQLite support**: Lightweight alternative to PostgreSQL
- **Docker integration**: Automatic PostgreSQL container management
- **Migration testing**: Test EF Core migrations
- **Performance benchmarking**: Database performance metrics
- **Multi-tenant testing**: Support for multiple test databases 