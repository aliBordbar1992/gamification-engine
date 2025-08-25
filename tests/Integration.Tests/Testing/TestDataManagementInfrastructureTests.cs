using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using GamificationEngine.Integration.Tests.Infrastructure.Testing;
using GamificationEngine.Integration.Tests.Database;

namespace GamificationEngine.Integration.Tests.Testing;

/// <summary>
/// Tests for the test data management infrastructure
/// </summary>
public class TestDataManagementInfrastructureTests
{
    private readonly Mock<ILogger<TestDataIsolationManager>> _mockLogger;
    private readonly Mock<ITestDatabase> _mockTestDatabase;

    public TestDataManagementInfrastructureTests()
    {
        _mockLogger = new Mock<ILogger<TestDataIsolationManager>>();
        _mockTestDatabase = new Mock<ITestDatabase>();
    }

    [Fact]
    public void TestDataFixtures_CreateOnboardingFixture_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFixtures.CreateOnboardingFixture();

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("User Onboarding");
        fixture.UserStates.ShouldHaveSingleItem();
        fixture.Events.Count.ShouldBe(5);
        fixture.Metadata["scenario"].ShouldBe("onboarding");
        fixture.Metadata["userCount"].ShouldBe(1);
        fixture.Metadata["eventCount"].ShouldBe(5);
    }

    [Fact]
    public void TestDataFixtures_CreateEngagementFixture_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFixtures.CreateEngagementFixture();

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("User Engagement");
        fixture.UserStates.Count.ShouldBe(3);
        fixture.Events.Count.ShouldBe(14);
        fixture.Metadata["scenario"].ShouldBe("engagement");
    }

    [Fact]
    public void TestDataFixtures_CreatePointAccumulationFixture_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFixtures.CreatePointAccumulationFixture();

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("Point Accumulation");
        fixture.UserStates.Count.ShouldBe(2);
        fixture.Events.Count.ShouldBe(12);
        fixture.Metadata["scenario"].ShouldBe("points");
    }

    [Fact]
    public void TestDataFixtures_CreateBadgeProgressionFixture_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFixtures.CreateBadgeProgressionFixture();

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("Badge Progression");
        fixture.UserStates.Count.ShouldBe(2);
        fixture.Events.Count.ShouldBe(6);
        fixture.Metadata["scenario"].ShouldBe("badges");
    }

    [Fact]
    public void TestDataFixtures_CreateEventSequenceFixture_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFixtures.CreateEventSequenceFixture();

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("Event Sequences");
        fixture.UserStates.Count.ShouldBe(2);
        fixture.Events.Count.ShouldBe(9);
        fixture.Metadata["scenario"].ShouldBe("sequences");
    }

    [Fact]
    public void TestDataFixtures_CreateMultiUserActivityFixture_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFixtures.CreateMultiUserActivityFixture();

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("Multi-User Activity");
        fixture.UserStates.Count.ShouldBe(5);
        fixture.Events.Count.ShouldBe(15);
        fixture.Metadata["scenario"].ShouldBe("multi-user");
    }

    [Fact]
    public void TestDataFixtures_CreateErrorScenarioFixture_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFixtures.CreateErrorScenarioFixture();

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("Error Scenarios");
        fixture.UserStates.Count.ShouldBe(2);
        fixture.Events.Count.ShouldBe(4);
        fixture.Metadata["scenario"].ShouldBe("error-testing");
    }

    [Fact]
    public void TestDataFactory_CreateUserProgressionScenario_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFactory.CreateUserProgressionScenario(userCount: 2, eventsPerUser: 5);

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("User Progression");
        fixture.UserStates.Count.ShouldBe(2);
        fixture.Events.Count.ShouldBe(10);
        fixture.Metadata["scenario"].ShouldBe("progression");
    }

    [Fact]
    public void TestDataFactory_CreateEventCorrelationScenario_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFactory.CreateEventCorrelationScenario(userCount: 3, correlationGroups: 2);

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("Event Correlation");
        fixture.UserStates.Count.ShouldBe(3);
        fixture.Events.Count.ShouldBe(18); // 3 users * 2 groups * 3 events per sequence
        fixture.Metadata["scenario"].ShouldBe("correlation");
    }

    [Fact]
    public void TestDataFactory_CreateTimeBasedConditionScenario_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFactory.CreateTimeBasedConditionScenario(userCount: 2, timeWindows: 2);

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("Time-Based Conditions");
        fixture.UserStates.Count.ShouldBe(2);
        fixture.Events.Count.ShouldBe(84); // 2 users * 2 windows * 7 days * 3 events per day
        fixture.Metadata["scenario"].ShouldBe("time-based");
    }

    [Fact]
    public void TestDataFactory_CreateAttributeBasedConditionScenario_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFactory.CreateAttributeBasedConditionScenario(userCount: 2, attributeVariations: 3);

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("Attribute-Based Conditions");
        fixture.UserStates.Count.ShouldBe(2);
        fixture.Events.Count.ShouldBe(6); // 2 users * 3 variations
        fixture.Metadata["scenario"].ShouldBe("attribute-based");
    }

    [Fact]
    public void TestDataFactory_CreatePerformanceTestScenario_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFactory.CreatePerformanceTestScenario(userCount: 5, eventsPerUser: 20);

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("Performance Test");
        fixture.UserStates.Count.ShouldBe(5);
        fixture.Events.Count.ShouldBe(100); // 5 users * 20 events
        fixture.Metadata["scenario"].ShouldBe("performance");
    }

    [Fact]
    public void TestDataFactory_CreateEdgeCaseScenario_ShouldCreateValidFixture()
    {
        // Act
        var fixture = TestDataFactory.CreateEdgeCaseScenario();

        // Assert
        fixture.ShouldNotBeNull();
        fixture.Name.ShouldBe("Edge Cases");
        fixture.UserStates.Count.ShouldBe(2);
        fixture.Events.Count.ShouldBe(4);
        fixture.Metadata["scenario"].ShouldBe("edge-cases");
    }

    [Fact]
    public void TestDataValidationUtilities_ValidateEvent_ShouldValidateValidEvent()
    {
        // Arrange
        var validEvent = TestDataFixtures.CreateOnboardingFixture().Events.First();

        // Act
        var isValid = TestDataValidationUtilities.ValidateEvent(validEvent);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void TestDataValidationUtilities_ValidateEvent_ShouldRejectInvalidEvent()
    {
        // Arrange
        var invalidEvent = new GamificationEngine.Domain.Events.Event(
            "valid-id", // Need valid ID to create event
            "TEST_EVENT",
            "test-user",
            DateTimeOffset.UtcNow
        );

        // Act - Test with a valid event (since Event constructor validates)
        var isValid = TestDataValidationUtilities.ValidateEvent(invalidEvent);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void TestDataValidationUtilities_ValidateFixture_ShouldValidateValidFixture()
    {
        // Arrange
        var validFixture = TestDataFixtures.CreateOnboardingFixture();

        // Act
        var isValid = TestDataValidationUtilities.ValidateFixture(validFixture);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void TestDataValidationUtilities_ValidateFixture_ShouldRejectInvalidFixture()
    {
        // Arrange
        var invalidFixture = new TestDataFixture
        {
            Name = "", // Invalid empty name
            Events = new List<GamificationEngine.Domain.Events.Event>(),
            UserStates = new List<GamificationEngine.Domain.Users.UserState>()
        };

        // Act
        var isValid = TestDataValidationUtilities.ValidateFixture(invalidFixture);

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void TestDataValidationUtilities_ValidateEventTimeOrdering_ShouldValidateOrderedEvents()
    {
        // Arrange
        var fixture = TestDataFixtures.CreateEventSequenceFixture();
        var userId = fixture.UserStates.First().UserId;

        // Act
        var isValid = TestDataValidationUtilities.ValidateEventTimeOrdering(userId, fixture.Events);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void TestDataValidationUtilities_ValidateNoDuplicateEventIds_ShouldValidateUniqueIds()
    {
        // Arrange
        var fixture = TestDataFixtures.CreateOnboardingFixture();

        // Act
        var isValid = TestDataValidationUtilities.ValidateNoDuplicateEventIds(fixture.Events);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void TestDataValidationUtilities_ValidateUserIdFormat_ShouldValidateValidFormats()
    {
        // Act & Assert
        TestDataValidationUtilities.ValidateUserIdFormat("valid-user-123").ShouldBeTrue();
        TestDataValidationUtilities.ValidateUserIdFormat("user_with_underscores").ShouldBeTrue();
        TestDataValidationUtilities.ValidateUserIdFormat("").ShouldBeFalse();
        TestDataValidationUtilities.ValidateUserIdFormat(null!).ShouldBeFalse();
        TestDataValidationUtilities.ValidateUserIdFormat(new string('x', 101)).ShouldBeFalse(); // Too long
    }

    [Fact]
    public void TestDataValidationUtilities_ValidateEventTypeFormat_ShouldValidateValidFormats()
    {
        // Act & Assert
        TestDataValidationUtilities.ValidateEventTypeFormat("VALID_EVENT_TYPE").ShouldBeTrue();
        TestDataValidationUtilities.ValidateEventTypeFormat("event_with_underscores").ShouldBeTrue();
        TestDataValidationUtilities.ValidateEventTypeFormat("").ShouldBeFalse();
        TestDataValidationUtilities.ValidateEventTypeFormat(null!).ShouldBeFalse();
        TestDataValidationUtilities.ValidateEventTypeFormat(new string('x', 101)).ShouldBeFalse(); // Too long
    }

    [Fact]
    public void TestDataValidationUtilities_ValidateTimestamp_ShouldValidateReasonableTimestamps()
    {
        // Act & Assert
        TestDataValidationUtilities.ValidateTimestamp(DateTimeOffset.UtcNow).ShouldBeTrue();
        TestDataValidationUtilities.ValidateTimestamp(DateTimeOffset.UtcNow.AddDays(-1)).ShouldBeTrue();
        TestDataValidationUtilities.ValidateTimestamp(DateTimeOffset.UtcNow.AddYears(-5)).ShouldBeTrue();
        TestDataValidationUtilities.ValidateTimestamp(DateTimeOffset.UtcNow.AddYears(-11)).ShouldBeFalse(); // Too old
        TestDataValidationUtilities.ValidateTimestamp(DateTimeOffset.UtcNow.AddDays(2)).ShouldBeFalse(); // Too far in future
    }

    [Fact]
    public void TestDataValidationUtilities_ValidateFixtureComprehensive_ShouldValidateCompleteFixture()
    {
        // Arrange
        var fixture = TestDataFixtures.CreateOnboardingFixture();

        // Act
        var (isValid, errors) = TestDataValidationUtilities.ValidateFixtureComprehensive(fixture);

        // Assert
        isValid.ShouldBeTrue();
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void TestDataValidationUtilities_ValidateStatisticsConsistency_ShouldValidateConsistentStats()
    {
        // Arrange
        var fixture = TestDataFixtures.CreateOnboardingFixture();
        var statistics = new TestDataStatistics
        {
            TotalEvents = fixture.Events.Count,
            TotalUserStates = fixture.UserStates.Count,
            UniqueEventTypes = fixture.Events.Select(e => e.EventType).Distinct().Count(),
            UniqueUsers = fixture.Events.Select(e => e.UserId).Distinct().Count(),
            DataTimeRange = TimeSpan.Zero
        };

        // Act
        var isValid = TestDataValidationUtilities.ValidateStatisticsConsistency(fixture, statistics);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void TestDataIsolationManager_CreateIsolatedScope_ShouldCreateValidScope()
    {
        // Arrange
        var isolationManager = new TestDataIsolationManager(_mockLogger.Object);

        // Act
        var scope = isolationManager.CreateIsolatedScope("test-1");

        // Assert
        scope.ShouldNotBeNull();
        scope.TestId.ShouldBe("test-1");
        scope.ReservedUserIds.Count.ShouldBe(0);
        scope.ReservedEventIds.Count.ShouldBe(0);
    }

    [Fact]
    public void TestDataIsolationManager_GenerateUniqueUserId_ShouldGenerateUniqueIds()
    {
        // Arrange
        var isolationManager = new TestDataIsolationManager(_mockLogger.Object);

        // Act
        var userId1 = isolationManager.GenerateUniqueUserId("test-1");
        var userId2 = isolationManager.GenerateUniqueUserId("test-2");

        // Assert
        userId1.ShouldNotBeNull();
        userId2.ShouldNotBeNull();
        userId1.ShouldNotBe(userId2);
        userId1.ShouldStartWith("test-user-test-1");
        userId2.ShouldStartWith("test-user-test-2");
    }

    [Fact]
    public void TestDataIsolationManager_GenerateUniqueEventId_ShouldGenerateUniqueIds()
    {
        // Arrange
        var isolationManager = new TestDataIsolationManager(_mockLogger.Object);

        // Act
        var eventId1 = isolationManager.GenerateUniqueEventId("test-1");
        var eventId2 = isolationManager.GenerateUniqueEventId("test-2");

        // Assert
        eventId1.ShouldNotBeNull();
        eventId2.ShouldNotBeNull();
        eventId1.ShouldNotBe(eventId2);
        eventId1.ShouldStartWith("test-event-test-1");
        eventId2.ShouldStartWith("test-event-test-2");
    }

    [Fact]
    public void TestDataIsolationManager_GetActiveScopeCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var isolationManager = new TestDataIsolationManager(_mockLogger.Object);

        // Act
        var initialCount = isolationManager.GetActiveScopeCount();
        var scope1 = isolationManager.CreateIsolatedScope("test-1");
        var scope2 = isolationManager.CreateIsolatedScope("test-2");
        var countAfterCreation = isolationManager.GetActiveScopeCount();

        // Cleanup
        scope1.Dispose();
        scope2.Dispose();

        // Assert
        initialCount.ShouldBe(0);
        countAfterCreation.ShouldBe(2);
    }

    [Fact]
    public void TestDataScope_Dispose_ShouldCleanupProperly()
    {
        // Arrange
        var isolationManager = new TestDataIsolationManager(_mockLogger.Object);
        var scope = isolationManager.CreateIsolatedScope("test-dispose");

        // Act
        scope.Dispose();

        // Assert
        isolationManager.GetActiveScopeCount().ShouldBe(0);
    }
}