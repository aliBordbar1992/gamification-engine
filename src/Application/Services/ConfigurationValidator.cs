using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.Configuration;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Services;

public sealed class ConfigurationValidator : IConfigurationValidator
{
    public Result<EngineConfiguration, string[]> Validate(EngineConfiguration configuration)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(configuration.Engine.Id))
            errors.Add("engine.id is required");

        if (configuration.PointCategories.GroupBy(pc => pc.Id).Any(g => g.Count() > 1))
            errors.Add("point_categories contain duplicate ids");

        if (configuration.Badges.GroupBy(b => b.Id).Any(g => g.Count() > 1))
            errors.Add("badges contain duplicate ids");

        if (configuration.Events.GroupBy(e => e.Id).Any(g => g.Count() > 1))
            errors.Add("events contain duplicate ids");

        return errors.Count == 0
            ? Result.Success<EngineConfiguration, string[]>(configuration)
            : Result.Failure<EngineConfiguration, string[]>(errors.ToArray());
    }
}
