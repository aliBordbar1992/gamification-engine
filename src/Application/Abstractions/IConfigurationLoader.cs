using GamificationEngine.Application.Configuration;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

public interface IConfigurationLoader
{
    Task<Result<EngineConfiguration, string>> LoadFromFileAsync(string path, CancellationToken cancellationToken = default);
}