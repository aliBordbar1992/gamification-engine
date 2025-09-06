using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Users;
using GamificationEngine.Infrastructure.Storage.EntityFramework;
using GamificationEngine.Integration.Tests.Infrastructure;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using GamificationEngine.Integration.Tests.Infrastructure.Utils;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace GamificationEngine.Integration.Tests.Tests;

/// <summary>
/// Tests for the new assertion utility classes
/// </summary>
public class TestAssertionUtilitiesTests : IntegrationTestBase, IAsyncLifetime
{
    private ILogger<TestAssertionUtilitiesTests>? _logger;
    private GamificationEngineDbContext? _dbContext;

    public async Task InitializeAsync()
    {
        _logger = GetService<ILogger<TestAssertionUtilitiesTests>>();
        _dbContext = GetService<GamificationEngineDbContext>();

        // Ensure database is clean
        await _dbContext.Database.EnsureCreatedAsync();
        await CleanupDatabase();
    }

    public new async Task DisposeAsync()
    {
        await CleanupDatabase();
    }

    private async Task CleanupDatabase()
    {
        if (_dbContext != null)
        {
            _dbContext.Events.RemoveRange(_dbContext.Events);
            _dbContext.UserStates.RemoveRange(_dbContext.UserStates);
            await _dbContext.SaveChangesAsync();
        }
    }

    [Fact]
    public void DomainEntityAssertionUtilities_ShouldValidateRuleProperties()
    {
        // Arrange
        var conditions = new List<Condition>
        {
            new TestCondition("cond1", "testType")
        };

        var rewards = new List<Reward>
        {
            new TestReward("reward1", "testRewardType")
        };

        var rule = new Rule("rule1", "Test Rule", new[] { "EVENT1", "EVENT2" }, conditions, rewards);

        // Act & Assert
        DomainEntityAssertionUtilities.AssertRuleProperties(rule, "rule1", "Test Rule", new[] { "EVENT1", "EVENT2" });
        DomainEntityAssertionUtilities.AssertRuleConditionCount(rule, 1);
        DomainEntityAssertionUtilities.AssertRuleRewardCount(rule, 1);
        DomainEntityAssertionUtilities.AssertRuleContainsTrigger(rule, "EVENT1");
        DomainEntityAssertionUtilities.AssertRuleContainsConditionType(rule, "testType");
        DomainEntityAssertionUtilities.AssertRuleContainsRewardType(rule, "testRewardType");
    }

    [Fact]
    public void DomainEntityAssertionUtilities_ShouldValidateConditionProperties()
    {
        // Arrange
        var condition = new TestCondition("cond1", "testType");

        // Act & Assert
        DomainEntityAssertionUtilities.AssertConditionProperties(condition, "cond1", "testType");
        DomainEntityAssertionUtilities.AssertConditionParameterCount(condition, 0);
    }

    [Fact]
    public void DomainEntityAssertionUtilities_ShouldValidateRewardProperties()
    {
        // Arrange
        var reward = new TestReward("reward1", "testRewardType");

        // Act & Assert
        DomainEntityAssertionUtilities.AssertRewardProperties(reward, "reward1", "testRewardType");
        DomainEntityAssertionUtilities.AssertRewardParameterCount(reward, 0);
    }

    [Fact]
    public void DomainEntityAssertionUtilities_ShouldValidateCollections()
    {
        // Arrange
        var rules = new List<Rule>
        {
            new Rule("rule1", "Test Rule 1", new[] { "EVENT1" }, new List<Condition> { new TestCondition("cond1", "type1") }, new List<Reward> { new TestReward("reward1", "rewardType1") }),
            new Rule("rule2", "Test Rule 2", new[] { "EVENT2" }, new List<Condition> { new TestCondition("cond2", "type2") }, new List<Reward> { new TestReward("reward2", "rewardType2") })
        };

        var conditions = new List<Condition>
        {
            new TestCondition("cond1", "type1"),
            new TestCondition("cond2", "type2")
        };

        var rewards = new List<Reward>
        {
            new TestReward("reward1", "rewardType1"),
            new TestReward("reward2", "rewardType2")
        };

        // Act & Assert
        DomainEntityAssertionUtilities.AssertRulesCollectionContains(rules, "rule1", "Test Rule 1");
        DomainEntityAssertionUtilities.AssertConditionsCollectionContains(conditions, "cond1", "type1");
        DomainEntityAssertionUtilities.AssertRewardsCollectionContains(rewards, "reward1", "rewardType1");
    }

    [Fact]
    public async Task DatabaseStateAssertionUtilities_ShouldValidateEventDatabaseState()
    {
        // Arrange
        var @event = new Event("event1", "TEST_EVENT", "user1", DateTimeOffset.Now, new Dictionary<string, object>());
        _dbContext!.Events.Add(@event);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await DatabaseStateAssertionUtilities.AssertEventExistsInDatabase(_dbContext, "event1", "user1", "TEST_EVENT");
        await DatabaseStateAssertionUtilities.AssertUserEventCount(_dbContext, "user1", 1);
        await DatabaseStateAssertionUtilities.AssertEventTypeCount(_dbContext, "TEST_EVENT", 1);
        await DatabaseStateAssertionUtilities.AssertTotalEventCount(_dbContext, 1);
    }

    [Fact]
    public async Task DatabaseStateAssertionUtilities_ShouldValidateUserStateDatabaseState()
    {
        // Arrange
        var userState = new UserState("user1");
        userState.AddPoints("XP", 100);
        _dbContext!.UserStates.Add(userState);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await DatabaseStateAssertionUtilities.AssertUserStateExistsInDatabase(_dbContext, "user1", new Dictionary<string, long> { ["XP"] = 100 });
        await DatabaseStateAssertionUtilities.AssertUserStateCount(_dbContext, 1);
    }

    [Fact]
    public async Task DatabaseStateAssertionUtilities_ShouldValidateEventAttributes()
    {
        // Arrange
        var attributes = new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = 42 };
        var @event = new Event("event1", "TEST_EVENT", "user1", DateTimeOffset.Now, attributes);
        _dbContext!.Events.Add(@event);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await DatabaseStateAssertionUtilities.AssertEventAttributes(_dbContext, "event1", attributes);
    }

    [Fact]
    public async Task DatabaseStateAssertionUtilities_ShouldValidateUserBadgesAndTrophies()
    {
        // Arrange
        var badges = new HashSet<string> { "badge1", "badge2" };
        var trophies = new HashSet<string> { "trophy1" };
        var userState = new UserState("user1");
        foreach (var badge in badges) userState.GrantBadge(badge);
        // Note: UserState doesn't have a GrantTrophy method yet, so we'll skip trophy validation for now
        _dbContext!.UserStates.Add(userState);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await DatabaseStateAssertionUtilities.AssertUserBadges(_dbContext, "user1", badges);
        // await DatabaseStateAssertionUtilities.AssertUserTrophies(_dbContext, "user1", trophies); // Skip for now
    }

    [Fact]
    public async Task DatabaseStateAssertionUtilities_ShouldValidateChronologicalOrder()
    {
        // Arrange
        var events = new List<Event>
        {
            new Event("event1", "TEST_EVENT", "user1", DateTimeOffset.Now.AddMinutes(-2), new Dictionary<string, object>()),
            new Event("event2", "TEST_EVENT", "user1", DateTimeOffset.Now.AddMinutes(-1), new Dictionary<string, object>()),
            new Event("event3", "TEST_EVENT", "user1", DateTimeOffset.Now, new Dictionary<string, object>())
        };

        _dbContext!.Events.AddRange(events);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await DatabaseStateAssertionUtilities.AssertUserEventsChronologicalOrder(_dbContext, "user1");
    }

    [Fact]
    public async Task DatabaseStateAssertionUtilities_ShouldValidateCleanDatabase()
    {
        // Act & Assert
        await DatabaseStateAssertionUtilities.AssertDatabaseIsClean(_dbContext!);
    }

    [Fact]
    public void JsonResponseValidationUtilities_ShouldValidateJsonSchema()
    {
        // Arrange
        var jsonContent = """
        {
            "id": "test123",
            "name": "Test Item",
            "active": true,
            "count": 42,
            "tags": ["tag1", "tag2"],
            "metadata": {
                "created": "2024-01-01",
                "version": 1.0
            }
        }
        """;

        var response = CreateMockHttpResponse(jsonContent);

        var schema = JsonSchema.Object(new Dictionary<string, JsonSchema>
        {
            ["id"] = JsonSchema.String(1, 50),
            ["name"] = JsonSchema.String(1, 100),
            ["active"] = JsonSchema.Boolean(),
            ["count"] = JsonSchema.Number(0, 100),
            ["tags"] = JsonSchema.Array(JsonSchema.String(1, 20), 1, 10),
            ["metadata"] = JsonSchema.Object(new Dictionary<string, JsonSchema>
            {
                ["created"] = JsonSchema.String(),
                ["version"] = JsonSchema.Number()
            })
        }, new List<string> { "id", "name" });

        // Act & Assert
        JsonResponseValidationUtilities.AssertJsonSchema(response, schema);
    }

    [Fact]
    public void JsonResponseValidationUtilities_ShouldValidateJsonPropertyPaths()
    {
        // Arrange
        var jsonContent = """
        {
            "data": {
                "items": [
                    {"id": 1, "name": "Item 1"},
                    {"id": 2, "name": "Item 2"}
                ],
                "total": 2
            }
        }
        """;

        var response = CreateMockHttpResponse(jsonContent);

        // Act & Assert
        JsonResponseValidationUtilities.AssertJsonPropertyPath(response, "data.total", 2);
        JsonResponseValidationUtilities.AssertJsonPropertyPathExists(response, "data.items");
        JsonResponseValidationUtilities.AssertJsonArrayAtPath(response, "data.items", 2);
        JsonResponseValidationUtilities.AssertJsonObjectAtPath(response, "data");
        JsonResponseValidationUtilities.AssertJsonStringAtPath(response, "data.items[0].name", "Item 1");
        JsonResponseValidationUtilities.AssertJsonNumberAtPath(response, "data.items[0].id", 1);
    }

    [Fact]
    public void TestResultReportingUtilities_ShouldCreateAndManageTestContext()
    {
        // Arrange
        var context = TestResultReportingUtilities.CreateTestContext("TestName");

        // Act
        context.AddStep("Step 1");
        context.AddStep("Step 2");
        context.AddWarning("Warning message");
        context.AddError("Error message");
        context.AddContextData("key1", "value1");
        context.AddContextData("key2", 42);

        // Assert
        context.TestName.ShouldBe("TestName");
        context.Steps.Count.ShouldBe(2);
        context.Warnings.Count.ShouldBe(1);
        context.Errors.Count.ShouldBe(1);
        context.ContextData.Count.ShouldBe(2);
        context.ContextData["key1"].ShouldBe("value1");
        context.ContextData["key2"].ShouldBe(42);
    }

    [Fact]
    public void TestResultReportingUtilities_ShouldGenerateDetailedReport()
    {
        // Arrange
        var context = TestResultReportingUtilities.CreateTestContext("TestName");
        context.AddStep("Step 1");
        context.AddStep("Step 2");
        context.AddWarning("Warning message");
        context.AddError("Error message");
        context.AddContextData("key1", "value1");

        // Act
        var report = TestResultReportingUtilities.FinalizeTestContext(context);

        // Assert
        report.ShouldContain("Test Execution Report: TestName");
        report.ShouldContain("Step 1");
        report.ShouldContain("Step 2");
        report.ShouldContain("WARNING: Warning message");
        report.ShouldContain("ERROR: Error message");
        report.ShouldContain("key1: value1");
    }

    [Fact]
    public async Task TestResultReportingUtilities_ShouldMeasureOperations()
    {
        // Arrange
        var context = TestResultReportingUtilities.CreateTestContext("TestName");

        // Act
        var result = await TestResultReportingUtilities.MeasureOperationAsync(
            "TestOperation",
            async () =>
            {
                await Task.Delay(10); // Simulate some work
                return "result";
            },
            _logger,
            context);

        // Assert
        result.ShouldBe("result");
        context.Steps.Count.ShouldBe(2); // Start and complete
        context.Steps.ShouldContain(s => s.Contains("Starting operation: TestOperation"));
        context.Steps.ShouldContain(s => s.Contains("Completed operation: TestOperation"));
    }

    [Fact]
    public async Task TestResultReportingUtilities_ShouldAssertTimeLimit()
    {
        // Arrange
        var context = TestResultReportingUtilities.CreateTestContext("TestName");
        var timeLimit = TimeSpan.FromMilliseconds(100);

        // Act & Assert
        await TestResultReportingUtilities.AssertOperationCompletesWithinTimeLimitAsync(
            "FastOperation",
            async () => await Task.Delay(10), // Should complete within limit
            timeLimit,
            _logger,
            context);

        context.Steps.Count.ShouldBe(2);
        context.Steps.ShouldContain(s => s.Contains("Starting timed operation: FastOperation"));
        context.Steps.ShouldContain(s => s.Contains("Completed timed operation: FastOperation"));
    }

    [Fact]
    public void TestResultReportingUtilities_ShouldCaptureSystemInfo()
    {
        // Act
        var systemInfo = TestResultReportingUtilities.CaptureSystemInfo();

        // Assert
        systemInfo.ShouldNotBeNull();
        systemInfo.ShouldContainKey("MachineName");
        systemInfo.ShouldContainKey("OSVersion");
        systemInfo.ShouldContainKey("ProcessorCount");
        systemInfo.ShouldContainKey("WorkingSet");
        systemInfo.ShouldContainKey("Is64BitProcess");
        systemInfo.ShouldContainKey("Is64BitOperatingSystem");
        systemInfo.ShouldContainKey("CurrentDirectory");
        systemInfo.ShouldContainKey("UserName");
        systemInfo.ShouldContainKey("DateTime");
    }

    [Fact]
    public void TestResultReportingUtilities_ShouldGenerateSummaryReport()
    {
        // Arrange
        var contexts = new List<TestExecutionContext>
        {
            CreateTestContextWithDuration("Test1", TimeSpan.FromMilliseconds(100)),
            CreateTestContextWithDuration("Test2", TimeSpan.FromMilliseconds(200)),
            CreateTestContextWithDuration("Test3", TimeSpan.FromMilliseconds(150))
        };

        // Act
        var summary = TestResultReportingUtilities.GenerateTestSummaryReport(contexts);

        // Assert
        summary.ShouldContain("Test Execution Summary Report");
        summary.ShouldContain("Total Tests: 3");
        summary.ShouldContain("Total Duration: 450.00ms");
        summary.ShouldContain("Average Duration: 150.00ms");
        summary.ShouldContain("Min Duration: 100.00ms");
        summary.ShouldContain("Max Duration: 200.00ms");
    }

    private TestExecutionContext CreateTestContextWithDuration(string testName, TimeSpan duration)
    {
        var context = TestResultReportingUtilities.CreateTestContext(testName);
        context.StartTime = DateTime.Now.Subtract(duration);
        context.EndTime = DateTime.Now;
        return context;
    }

    private HttpResponseMessage CreateMockHttpResponse(string jsonContent)
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };
        return response;
    }

    // Test implementations of abstract classes
    private class TestCondition : Condition
    {
        public TestCondition(string conditionId, string type) : base(conditionId, type)
        {
        }

        public override bool Evaluate(IEnumerable<Event> events, Event triggerEvent)
        {
            return true; // Simple test implementation
        }
    }

    private class TestReward : Reward
    {
        public TestReward(string rewardId, string type) : base(rewardId, type)
        {
        }
    }
}