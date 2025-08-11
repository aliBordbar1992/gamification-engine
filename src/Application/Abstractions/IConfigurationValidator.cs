using GamificationEngine.Application.Configuration;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

public interface IConfigurationValidator
{
    Result<EngineConfiguration, string[]> Validate(EngineConfiguration configuration);
}