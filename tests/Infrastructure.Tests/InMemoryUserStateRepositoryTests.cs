using GamificationEngine.Domain.Users;
using GamificationEngine.Infrastructure.Storage.InMemory;
using Shouldly;
using Xunit;

namespace GamificationEngine.Infrastructure.Tests;

public class InMemoryUserStateRepositoryTests
{
    [Fact]
    public async Task Save_Then_Get_Should_Return_Same_Instance()
    {
        var repo = new InMemoryUserStateRepository();
        var state = new UserState("user-1");
        state.AddPoints("xp", 10);
        state.GrantBadge("badge-commenter");

        await repo.SaveAsync(state);

        var loaded = await repo.GetByUserIdAsync("user-1");
        loaded.ShouldNotBeNull();
        loaded!.UserId.ShouldBe("user-1");
        loaded.PointsByCategory.TryGetValue("xp", out var points).ShouldBeTrue();
        points.ShouldBe(10);
        loaded.Badges.ShouldContain("badge-commenter");
    }
}
