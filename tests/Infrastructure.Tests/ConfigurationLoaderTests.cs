using GamificationEngine.Application.Abstractions;
using GamificationEngine.Infrastructure.Configuration;
using Shouldly;
using Xunit;

namespace GamificationEngine.Infrastructure.Tests;

public class ConfigurationLoaderTests
{
    [Fact]
    public async Task LoadFromFileAsync_Should_Load_Configuration_Successfully()
    {
        // arrange
        IConfigurationLoader loader = new YamlConfigurationLoader();
        var fullPath = FindUpwards("configuration-example.yml");
        File.Exists(fullPath).ShouldBeTrue();

        // act
        var result = await loader.LoadFromFileAsync(fullPath);

        // assert
        result.IsSuccess.ShouldBeTrue(result.Error);
        result.Value.ShouldNotBeNull();
        result.Value!.Engine.Id.ShouldBe("default-gamification-engine");
        result.Value.Events.ShouldContain(e => e.Id == "USER_COMMENTED");
        result.Value.Badges.ShouldContain(b => b.Id == "badge-commenter");
    }

    private static string FindUpwards(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 10 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir.FullName, fileName);
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", fileName));
    }
}