using GamificationEngine.Application.Abstractions;
using GamificationEngine.Infrastructure.Configuration;
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
        Assert.True(File.Exists(fullPath));

        // act
        var result = await loader.LoadFromFileAsync(fullPath);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("default-gamification-engine", result.Value!.Engine.Id);
        Assert.Contains(result.Value.Events, e => e.Id == "USER_COMMENTED");
        Assert.Contains(result.Value.Badges, b => b.Id == "badge-commenter");
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
        // fall back to repo root relative to test project if possible
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", fileName));
    }
}