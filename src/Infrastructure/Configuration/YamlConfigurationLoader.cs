using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.Configuration;
using GamificationEngine.Shared;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GamificationEngine.Infrastructure.Configuration;

public sealed class YamlConfigurationLoader : IConfigurationLoader
{
    private readonly IDeserializer _deserializer;

    public YamlConfigurationLoader()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithTypeMapping<SimulationSettings, SimulationSettings>()
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public async Task<Result<EngineConfiguration, string>> LoadFromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Result.Failure<EngineConfiguration, string>("Path is null or empty");
        }

        if (!File.Exists(path))
        {
            return Result.Failure<EngineConfiguration, string>($"File not found: {path}");
        }

        try
        {
            using var stream = File.OpenRead(path);
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            var config = _deserializer.Deserialize<EngineConfiguration>(content);
            if (config is null)
            {
                return Result.Failure<EngineConfiguration, string>("Failed to deserialize configuration");
            }

            // Debug logging
            Console.WriteLine($"YAML Config loaded - Engine.Id: {config.Engine?.Id}");
            Console.WriteLine($"YAML Config loaded - Simulation.Enabled: {config.Simulation?.Enabled}");

            return Result.Success<EngineConfiguration, string>(config);
        }
        catch (Exception ex)
        {
            return Result.Failure<EngineConfiguration, string>($"Error reading configuration: {ex.Message}");
        }
    }
}