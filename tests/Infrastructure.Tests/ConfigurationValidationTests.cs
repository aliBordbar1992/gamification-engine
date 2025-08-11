using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.Services;
using GamificationEngine.Infrastructure.Configuration;
using Shouldly;
using Xunit;

namespace GamificationEngine.Infrastructure.Tests;

public class ConfigurationValidationTests
{
    [Fact]
    public async Task Validate_Should_Succeed_For_Sample_Config()
    {
        IConfigurationLoader loader = new YamlConfigurationLoader();
        var path = FindUpwards("configuration-example.yml");
        var result = await loader.LoadFromFileAsync(path);
        result.IsSuccess.ShouldBeTrue(result.Error);

        var validator = new ConfigurationValidator();
        var validation = validator.Validate(result.Value!);

        validation.IsSuccess.ShouldBeTrue(validation.Error != null ? string.Join(", ", validation.Error) : null);
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
