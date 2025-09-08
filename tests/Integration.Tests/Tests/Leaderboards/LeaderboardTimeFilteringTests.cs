using GamificationEngine.Integration.Tests.Infrastructure;
using GamificationEngine.Integration.Tests.Infrastructure.Utils;
using GamificationEngine.Integration.Tests.Infrastructure.Models;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;
using GamificationEngine.Infrastructure.Storage.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GamificationEngine.Integration.Tests.Tests.Leaderboards;

public class LeaderboardTimeFilteringTests : EndToEndTestBase
{
    private IUserStateRepository _userStateRepository = null!;
    private IRewardHistoryRepository _rewardHistoryRepository = null!;
    private ILeaderboardRepository _leaderboardRepository = null!;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        // Remove the in-memory services that the API registers by default
        var inMemoryRepoDescriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(IEventRepository) &&
                 d.ImplementationType == typeof(GamificationEngine.Infrastructure.Storage.InMemory.EventRepository));
        if (inMemoryRepoDescriptor != null)
        {
            services.Remove(inMemoryRepoDescriptor);
        }

        // Register repositories
        services.AddScoped<IEventRepository, GamificationEngine.Infrastructure.Storage.InMemory.EventRepository>();
        services.AddScoped<IUserStateRepository, InMemoryUserStateRepository>();
        services.AddScoped<IRewardHistoryRepository, InMemoryRewardHistoryRepository>();
        services.AddScoped<ILeaderboardRepository, InMemoryLeaderboardRepository>();
    }

    protected override async Task SetUpAsync()
    {
        await base.SetUpAsync();

        _userStateRepository = GetService<IUserStateRepository>();
        _rewardHistoryRepository = GetService<IRewardHistoryRepository>();
        _leaderboardRepository = GetService<ILeaderboardRepository>();

        // Create test users and reward history
        await SetupTestDataAsync();
    }

    private async Task SetupTestDataAsync()
    {
        // Create users
        var user1 = new UserState("user1");
        var user2 = new UserState("user2");
        var user3 = new UserState("user3");

        // Add some initial points to users
        user1.AddPoints("xp", 100);
        user2.AddPoints("xp", 200);
        user3.AddPoints("xp", 300);

        await _userStateRepository.SaveAsync(user1);
        await _userStateRepository.SaveAsync(user2);
        await _userStateRepository.SaveAsync(user3);

        // Create reward history entries with different timestamps
        var now = DateTimeOffset.UtcNow;
        var yesterday = now.AddDays(-1);
        var lastWeek = now.AddDays(-7);
        var lastMonth = now.AddDays(-30);

        // User1: Earned 50 points yesterday
        var reward1 = new RewardHistory(
            "reward1", "user1", "points-reward-1", "points", "event1",
            yesterday, true, "Awarded 50 xp points",
            new Dictionary<string, object> { { "amount", 50L }, { "category", "xp" } });
        await _rewardHistoryRepository.StoreAsync(reward1);

        // User2: Earned 100 points last week
        var reward2 = new RewardHistory(
            "reward2", "user2", "points-reward-2", "points", "event2",
            lastWeek, true, "Awarded 100 xp points",
            new Dictionary<string, object> { { "amount", 100L }, { "category", "xp" } });
        await _rewardHistoryRepository.StoreAsync(reward2);

        // User3: Earned 150 points last month
        var reward3 = new RewardHistory(
            "reward3", "user3", "points-reward-3", "points", "event3",
            lastMonth, true, "Awarded 150 xp points",
            new Dictionary<string, object> { { "amount", 150L }, { "category", "xp" } });
        await _rewardHistoryRepository.StoreAsync(reward3);

        // User1: Earned a badge yesterday
        var badgeReward1 = new RewardHistory(
            "badge1", "user1", "badge-reward-1", "badge", "event4",
            yesterday, true, "Awarded badge 'first-steps'",
            new Dictionary<string, object> { { "badgeId", "first-steps" } });
        await _rewardHistoryRepository.StoreAsync(badgeReward1);

        // User2: Earned a trophy last week
        var trophyReward1 = new RewardHistory(
            "trophy1", "user2", "trophy-reward-1", "trophy", "event5",
            lastWeek, true, "Awarded trophy 'weekly-champion'",
            new Dictionary<string, object> { { "trophyId", "weekly-champion" } });
        await _rewardHistoryRepository.StoreAsync(trophyReward1);
    }

    [Fact]
    public async Task GetLeaderboard_DailyTimeRange_ShouldOnlyIncludeUsersWithRewardsFromToday()
    {
        // Arrange
        var query = new GamificationEngine.Domain.Leaderboards.LeaderboardQuery(
            "points", "xp", "daily", 1, 50, DateTime.UtcNow);

        // Act
        var result = await _leaderboardRepository.GetLeaderboardAsync(query);

        // Assert
        result.ShouldNotBeNull();
        result.Entries.ShouldNotBeEmpty();

        // Only user1 should appear in daily leaderboard (earned points yesterday, which is within daily range)
        var user1Entry = result.Entries.FirstOrDefault(e => e.UserId == "user1");
        user1Entry.ShouldNotBeNull();
        user1Entry.Points.ShouldBe(50); // Only the points earned in the time range

        // User2 and user3 should not appear (earned rewards outside daily range)
        result.Entries.ShouldNotContain(e => e.UserId == "user2");
        result.Entries.ShouldNotContain(e => e.UserId == "user3");
    }

    [Fact]
    public async Task GetLeaderboard_WeeklyTimeRange_ShouldIncludeUsersWithRewardsFromThisWeek()
    {
        // Arrange
        var query = new GamificationEngine.Domain.Leaderboards.LeaderboardQuery(
            "points", "xp", "weekly", 1, 50, DateTime.UtcNow);

        // Act
        var result = await _leaderboardRepository.GetLeaderboardAsync(query);

        // Assert
        result.ShouldNotBeNull();
        result.Entries.ShouldNotBeEmpty();

        // User1 and user2 should appear (earned rewards within weekly range)
        var user1Entry = result.Entries.FirstOrDefault(e => e.UserId == "user1");
        user1Entry.ShouldNotBeNull();
        user1Entry.Points.ShouldBe(50);

        var user2Entry = result.Entries.FirstOrDefault(e => e.UserId == "user2");
        user2Entry.ShouldNotBeNull();
        user2Entry.Points.ShouldBe(100);

        // User3 should not appear (earned rewards outside weekly range)
        result.Entries.ShouldNotContain(e => e.UserId == "user3");
    }

    [Fact]
    public async Task GetLeaderboard_MonthlyTimeRange_ShouldIncludeUsersWithRewardsFromThisMonth()
    {
        // Arrange
        var query = new GamificationEngine.Domain.Leaderboards.LeaderboardQuery(
            "points", "xp", "monthly", 1, 50, DateTime.UtcNow);

        // Act
        var result = await _leaderboardRepository.GetLeaderboardAsync(query);

        // Assert
        result.ShouldNotBeNull();
        result.Entries.ShouldNotBeEmpty();

        // All users should appear (earned rewards within monthly range)
        result.Entries.ShouldContain(e => e.UserId == "user1");
        result.Entries.ShouldContain(e => e.UserId == "user2");
        result.Entries.ShouldContain(e => e.UserId == "user3");

        // Verify points are correct for time range
        var user1Entry = result.Entries.FirstOrDefault(e => e.UserId == "user1");
        user1Entry!.Points.ShouldBe(50);

        var user2Entry = result.Entries.FirstOrDefault(e => e.UserId == "user2");
        user2Entry!.Points.ShouldBe(100);

        var user3Entry = result.Entries.FirstOrDefault(e => e.UserId == "user3");
        user3Entry!.Points.ShouldBe(150);
    }

    [Fact]
    public async Task GetLeaderboard_AllTimeTimeRange_ShouldIncludeAllUsersWithCurrentState()
    {
        // Arrange
        var query = new GamificationEngine.Domain.Leaderboards.LeaderboardQuery(
            "points", "xp", "alltime", 1, 50, DateTime.UtcNow);

        // Act
        var result = await _leaderboardRepository.GetLeaderboardAsync(query);

        // Assert
        result.ShouldNotBeNull();
        result.Entries.ShouldNotBeEmpty();

        // All users should appear with their current total points
        result.Entries.ShouldContain(e => e.UserId == "user1");
        result.Entries.ShouldContain(e => e.UserId == "user2");
        result.Entries.ShouldContain(e => e.UserId == "user3");

        // Verify points are current totals (initial + earned)
        var user1Entry = result.Entries.FirstOrDefault(e => e.UserId == "user1");
        user1Entry!.Points.ShouldBe(150); // 100 initial + 50 earned

        var user2Entry = result.Entries.FirstOrDefault(e => e.UserId == "user2");
        user2Entry!.Points.ShouldBe(300); // 200 initial + 100 earned

        var user3Entry = result.Entries.FirstOrDefault(e => e.UserId == "user3");
        user3Entry!.Points.ShouldBe(450); // 300 initial + 150 earned
    }

    [Fact]
    public async Task GetLeaderboard_BadgesTimeRange_ShouldFilterBadgesByTimeRange()
    {
        // Arrange
        var query = new GamificationEngine.Domain.Leaderboards.LeaderboardQuery(
            "badges", null, "daily", 1, 50, DateTime.UtcNow);

        // Act
        var result = await _leaderboardRepository.GetLeaderboardAsync(query);

        // Assert
        result.ShouldNotBeNull();
        result.Entries.ShouldNotBeEmpty();

        // Only user1 should appear (earned badge yesterday)
        var user1Entry = result.Entries.FirstOrDefault(e => e.UserId == "user1");
        user1Entry.ShouldNotBeNull();
        user1Entry.Points.ShouldBe(1); // 1 badge earned in time range

        // User2 should not appear (earned trophy, not badge, and outside daily range)
        result.Entries.ShouldNotContain(e => e.UserId == "user2");
    }

    [Fact]
    public async Task GetLeaderboard_TrophiesTimeRange_ShouldFilterTrophiesByTimeRange()
    {
        // Arrange
        var query = new GamificationEngine.Domain.Leaderboards.LeaderboardQuery(
            "trophies", null, "weekly", 1, 50, DateTime.UtcNow);

        // Act
        var result = await _leaderboardRepository.GetLeaderboardAsync(query);

        // Assert
        result.ShouldNotBeNull();
        result.Entries.ShouldNotBeEmpty();

        // Only user2 should appear (earned trophy last week)
        var user2Entry = result.Entries.FirstOrDefault(e => e.UserId == "user2");
        user2Entry.ShouldNotBeNull();
        user2Entry.Points.ShouldBe(1); // 1 trophy earned in time range

        // User1 should not appear (earned badge, not trophy, and outside weekly range)
        result.Entries.ShouldNotContain(e => e.UserId == "user1");
    }
}
