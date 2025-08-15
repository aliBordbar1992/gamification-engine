# EF Core Infrastructure for Gamification Engine

This project provides Entity Framework Core implementation for the Gamification Engine with PostgreSQL support.

## Features

- **PostgreSQL Support**: Uses Npgsql.EntityFrameworkCore.PostgreSQL for database connectivity
- **DDD-Friendly Mapping**: Properly maps domain entities while preserving DDD principles
- **JSON Storage**: Uses PostgreSQL's JSONB type for flexible attribute storage
- **Retention Policy**: Automatic cleanup of old events based on configurable retention periods
- **Performance Optimized**: Includes proper indexing for common query patterns

## Architecture

### DbContext
The `GamificationEngineDbContext` handles the mapping between domain entities and database tables:

- **Event**: Stores user activity events with JSON attributes
- **UserState**: Stores user points, badges, and trophies as JSON
- **Rule**: Stores gamification rules with conditions and rewards
- **Condition**: Stores rule conditions with parameters as JSON
- **Reward**: Stores rule rewards with parameters as JSON

### Repositories
- **EventRepository**: Handles event storage, retrieval, and retention policies
- **UserStateRepository**: Manages user state persistence

### Background Services
- **EventRetentionService**: Automatically applies retention policies to clean up old events

## Configuration

### Connection String
Add to your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=gamification_engine;Username=postgres;Password=postgres"
  }
}
```

### Retention Policy
Configure event retention settings:

```json
{
  "EventRetention": {
    "RetentionPeriod": "90.00:00:00",
    "Interval": "24:00:00"
  }
}
```

## Usage

### 1. Add to Service Collection
```csharp
builder.Services.AddEntityFrameworkStorage(builder.Configuration);
```

### 2. Add Health Checks
```csharp
builder.Services.AddEntityFrameworkHealthChecks();
app.MapHealthChecks("/health");
```

### 3. Use in Controllers
```csharp
public class EventsController : ControllerBase
{
    private readonly IEventRepository _eventRepository;
    
    public EventsController(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        var @event = new Event(request.EventId, request.EventType, request.UserId, DateTimeOffset.UtcNow, request.Attributes);
        var result = await _eventRepository.StoreAsync(@event);
        
        if (result.IsSuccess)
            return Ok(result.Value);
            
        return BadRequest(result.Error.Message);
    }
}
```

## Database Schema

### Events Table
- `EventId` (PK, varchar(50))
- `EventType` (varchar(100), NOT NULL)
- `UserId` (varchar(50), NOT NULL)
- `OccurredAt` (timestamp with time zone, NOT NULL)
- `Attributes` (jsonb)

### UserStates Table
- `UserId` (PK, varchar(50))
- `PointsByCategory` (jsonb)
- `Badges` (jsonb)
- `Trophies` (jsonb)

### Rules Table
- `RuleId` (PK, varchar(50))
- `Name` (varchar(200), NOT NULL)
- `Triggers` (jsonb)

### Conditions Table
- `ConditionId` (PK, varchar(50))
- `Type` (varchar(100), NOT NULL)
- `Parameters` (jsonb)
- `RuleId` (FK to Rules)

### Rewards Table
- `RewardId` (PK, varchar(50))
- `Type` (varchar(100), NOT NULL)
- `Parameters` (jsonb)
- `RuleId` (FK to Rules)

## Indexes

The following indexes are created for performance:

- `UserId` on Events table
- `EventType` on Events table
- `OccurredAt` on Events table
- Composite index on `(UserId, EventType)` on Events table

## Migration

To create the initial database schema:

```bash
# From the Infrastructure/Storage.EntityFramework directory
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Testing

The project includes comprehensive tests using EF Core's in-memory provider:

- **DbContextConfigurationTests**: Verifies proper entity mapping
- **EventRepositoryTests**: Tests repository functionality
- **UserStateRepositoryTests**: Tests user state persistence

Run tests with:
```bash
dotnet test
```

## DDD Compliance

This implementation maintains DDD principles:

- **Domain Entities**: Remain pure and focused on business logic
- **Value Objects**: Stored as JSON for flexibility
- **Aggregates**: Properly bounded and encapsulated
- **Repository Pattern**: Clean separation between domain and infrastructure
- **No Infrastructure Dependencies**: Domain layer remains independent

## Performance Considerations

- **JSONB Indexing**: Consider adding GIN indexes on JSONB columns for complex queries
- **Batch Operations**: Use `StoreBatchAsync` for multiple events
- **Retention Policy**: Configure appropriate intervals to avoid performance impact
- **Connection Pooling**: PostgreSQL connection pooling is enabled by default

## Troubleshooting

### Common Issues

1. **Connection String**: Ensure PostgreSQL is running and accessible
2. **Database Permissions**: Verify user has CREATE, INSERT, UPDATE, DELETE permissions
3. **JSONB Support**: Ensure PostgreSQL version supports JSONB (9.4+)
4. **Migration Issues**: Clear migrations folder and recreate if schema changes

### Logging

Enable detailed EF Core logging in development:

```json
{
  "Logging": {
    "Microsoft.EntityFrameworkCore": "Information",
    "EnableSensitiveDataLogging": true
  }
}
``` 