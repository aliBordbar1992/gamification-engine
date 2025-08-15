using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GamificationEngine.Infrastructure.Storage.EntityFramework;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;
using Xunit;
using Shouldly;

namespace GamificationEngine.Infrastructure.EntityFramework.Tests;

/// <summary>
/// Tests for DbContext configuration and entity mapping
/// </summary>
public class DbContextConfigurationTests
{
    private readonly DbContextOptions<GamificationEngineDbContext> _options;

    public DbContextConfigurationTests()
    {
        _options = new DbContextOptionsBuilder<GamificationEngineDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private GamificationEngineDbContext CreateContext()
    {
        var context = new GamificationEngineDbContext(_options);
        return context;
    }

    [Fact]
    public void Event_ShouldBeConfiguredWithCorrectProperties()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Event));

        // Assert
        entityType.ShouldNotBeNull();
        var properties = entityType.GetProperties();
        properties.ShouldContain(p => p.Name == "EventId");

        var eventIdProperty = entityType.FindProperty("EventId");
        eventIdProperty.ShouldNotBeNull();
        eventIdProperty.GetMaxLength().ShouldBe(50);
        eventIdProperty.IsNullable.ShouldBeFalse();

        var eventTypeProperty = entityType.FindProperty("EventType");
        eventTypeProperty.ShouldNotBeNull();
        eventTypeProperty.GetMaxLength().ShouldBe(100);
        eventTypeProperty.IsNullable.ShouldBeFalse();

        var userIdProperty = entityType.FindProperty("UserId");
        userIdProperty.ShouldNotBeNull();
        userIdProperty.GetMaxLength().ShouldBe(50);
        userIdProperty.IsNullable.ShouldBeFalse();

        var occurredAtProperty = entityType.FindProperty("OccurredAt");
        occurredAtProperty.ShouldNotBeNull();
        occurredAtProperty.IsNullable.ShouldBeFalse();

        // Note: GetColumnType() is not supported by in-memory database
        // In a real PostgreSQL database, this would return "jsonb"
        var attributesProperty = entityType.FindProperty("Attributes");
        attributesProperty.ShouldNotBeNull();
    }

    [Fact]
    public void UserState_ShouldBeConfiguredWithCorrectProperties()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(UserState));

        // Assert
        entityType.ShouldNotBeNull();
        var properties = entityType.GetProperties();
        properties.ShouldContain(p => p.Name == "UserId");

        var userIdProperty = entityType.FindProperty("UserId");
        userIdProperty.ShouldNotBeNull();
        userIdProperty.GetMaxLength().ShouldBe(50);
        userIdProperty.IsNullable.ShouldBeFalse();

        // Note: GetColumnType() is not supported by in-memory database
        // In a real PostgreSQL database, these would return "jsonb"
        var pointsProperty = entityType.FindProperty("PointsByCategory");
        pointsProperty.ShouldNotBeNull();

        var badgesProperty = entityType.FindProperty("Badges");
        badgesProperty.ShouldNotBeNull();

        var trophiesProperty = entityType.FindProperty("Trophies");
        trophiesProperty.ShouldNotBeNull();
    }

    [Fact]
    public void Event_ShouldHaveCorrectIndexes()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Event));

        // Assert
        entityType.ShouldNotBeNull();
        var indexes = entityType.GetIndexes();

        // Check for UserId index
        indexes.ShouldContain(i => i.Properties.Any(p => p.Name == "UserId"));

        // Check for EventType index
        indexes.ShouldContain(i => i.Properties.Any(p => p.Name == "EventType"));

        // Check for OccurredAt index
        indexes.ShouldContain(i => i.Properties.Any(p => p.Name == "OccurredAt"));

        // Check for composite index on UserId and EventType
        indexes.ShouldContain(i => i.Properties.Count == 2 &&
                                   i.Properties.Any(p => p.Name == "UserId") &&
                                   i.Properties.Any(p => p.Name == "EventType"));
    }

    [Fact]
    public void Database_ShouldBeCreatedSuccessfully()
    {
        // Arrange & Act
        using var context = CreateContext();
        context.Database.EnsureCreated();

        // Assert
        context.Events.ShouldNotBeNull();
        context.UserStates.ShouldNotBeNull();
    }
}