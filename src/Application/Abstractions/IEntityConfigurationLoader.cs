using GamificationEngine.Application.Configuration;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for loading entity definitions from configuration
/// </summary>
public interface IEntityConfigurationLoader
{
    /// <summary>
    /// Loads all entity definitions from the engine configuration
    /// </summary>
    Task<Result<bool, string>> LoadEntitiesFromConfigurationAsync(EngineConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads point categories from configuration
    /// </summary>
    Task<Result<bool, string>> LoadPointCategoriesAsync(IEnumerable<Application.Configuration.PointCategory> pointCategories, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads badges from configuration
    /// </summary>
    Task<Result<bool, string>> LoadBadgesAsync(IEnumerable<BadgeDefinition> badges, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads trophies from configuration
    /// </summary>
    Task<Result<bool, string>> LoadTrophiesAsync(IEnumerable<TrophyDefinition> trophies, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads levels from configuration
    /// </summary>
    Task<Result<bool, string>> LoadLevelsAsync(IEnumerable<LevelDefinition> levels, CancellationToken cancellationToken = default);
}
