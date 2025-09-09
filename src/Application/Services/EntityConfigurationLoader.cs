using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.Configuration;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for loading entity definitions from configuration into repositories
/// </summary>
public sealed class EntityConfigurationLoader : IEntityConfigurationLoader
{
    private readonly IPointCategoryRepository _pointCategoryRepository;
    private readonly IBadgeRepository _badgeRepository;
    private readonly ITrophyRepository _trophyRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IEventDefinitionRepository _eventDefinitionRepository;

    public EntityConfigurationLoader(
        IPointCategoryRepository pointCategoryRepository,
        IBadgeRepository badgeRepository,
        ITrophyRepository trophyRepository,
        ILevelRepository levelRepository,
        IEventDefinitionRepository eventDefinitionRepository)
    {
        _pointCategoryRepository = pointCategoryRepository ?? throw new ArgumentNullException(nameof(pointCategoryRepository));
        _badgeRepository = badgeRepository ?? throw new ArgumentNullException(nameof(badgeRepository));
        _trophyRepository = trophyRepository ?? throw new ArgumentNullException(nameof(trophyRepository));
        _levelRepository = levelRepository ?? throw new ArgumentNullException(nameof(levelRepository));
        _eventDefinitionRepository = eventDefinitionRepository ?? throw new ArgumentNullException(nameof(eventDefinitionRepository));
    }

    public async Task<Result<bool, string>> LoadEntitiesFromConfigurationAsync(EngineConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration == null)
            return Result.Failure<bool, string>("Configuration cannot be null");

        try
        {
            // Load point categories
            if (configuration.PointCategories?.Any() == true)
            {
                var pointCategoryResult = await LoadPointCategoriesAsync(configuration.PointCategories, cancellationToken);
                if (!pointCategoryResult.IsSuccess)
                    return Result.Failure<bool, string>($"Failed to load point categories: {pointCategoryResult.Error}");
            }

            // Load badges
            if (configuration.Badges?.Any() == true)
            {
                var badgeResult = await LoadBadgesAsync(configuration.Badges, cancellationToken);
                if (!badgeResult.IsSuccess)
                    return Result.Failure<bool, string>($"Failed to load badges: {badgeResult.Error}");
            }

            // Load trophies
            if (configuration.Trophies?.Any() == true)
            {
                var trophyResult = await LoadTrophiesAsync(configuration.Trophies, cancellationToken);
                if (!trophyResult.IsSuccess)
                    return Result.Failure<bool, string>($"Failed to load trophies: {trophyResult.Error}");
            }

            // Load levels
            if (configuration.Levels?.Any() == true)
            {
                var levelResult = await LoadLevelsAsync(configuration.Levels, cancellationToken);
                if (!levelResult.IsSuccess)
                    return Result.Failure<bool, string>($"Failed to load levels: {levelResult.Error}");
            }

            // Load event definitions
            if (configuration.Events?.Any() == true)
            {
                var eventDefinitionResult = await LoadEventDefinitionsAsync(configuration.Events, cancellationToken);
                if (!eventDefinitionResult.IsSuccess)
                    return Result.Failure<bool, string>($"Failed to load event definitions: {eventDefinitionResult.Error}");
            }

            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to load entities from configuration: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> LoadPointCategoriesAsync(IEnumerable<Application.Configuration.PointCategory> pointCategories, CancellationToken cancellationToken = default)
    {
        if (pointCategories == null)
            return Result.Failure<bool, string>("Point categories cannot be null");

        try
        {
            foreach (var configCategory in pointCategories)
            {
                if (string.IsNullOrWhiteSpace(configCategory.Id) ||
                    string.IsNullOrWhiteSpace(configCategory.Name) ||
                    string.IsNullOrWhiteSpace(configCategory.Description) ||
                    string.IsNullOrWhiteSpace(configCategory.Aggregation))
                {
                    return Result.Failure<bool, string>($"Invalid point category configuration: {configCategory.Id}");
                }

                var exists = await _pointCategoryRepository.ExistsAsync(configCategory.Id, cancellationToken);
                if (!exists)
                {
                    var domainCategory = new Domain.Entities.PointCategory(
                        configCategory.Id,
                        configCategory.Name,
                        configCategory.Description,
                        configCategory.Aggregation);

                    if (!domainCategory.IsValid())
                        return Result.Failure<bool, string>($"Invalid point category data: {configCategory.Id}");

                    await _pointCategoryRepository.AddAsync(domainCategory, cancellationToken);
                }
            }

            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to load point categories: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> LoadBadgesAsync(IEnumerable<BadgeDefinition> badges, CancellationToken cancellationToken = default)
    {
        if (badges == null)
            return Result.Failure<bool, string>("Badges cannot be null");

        try
        {
            foreach (var configBadge in badges)
            {
                if (string.IsNullOrWhiteSpace(configBadge.Id) ||
                    string.IsNullOrWhiteSpace(configBadge.Name) ||
                    string.IsNullOrWhiteSpace(configBadge.Description) ||
                    string.IsNullOrWhiteSpace(configBadge.Image))
                {
                    return Result.Failure<bool, string>($"Invalid badge configuration: {configBadge.Id}");
                }

                var exists = await _badgeRepository.ExistsAsync(configBadge.Id, cancellationToken);
                if (!exists)
                {
                    var domainBadge = new Domain.Entities.Badge(
                        configBadge.Id,
                        configBadge.Name,
                        configBadge.Description,
                        configBadge.Image,
                        configBadge.Visible);

                    if (!domainBadge.IsValid())
                        return Result.Failure<bool, string>($"Invalid badge data: {configBadge.Id}");

                    await _badgeRepository.AddAsync(domainBadge, cancellationToken);
                }
            }

            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to load badges: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> LoadTrophiesAsync(IEnumerable<TrophyDefinition> trophies, CancellationToken cancellationToken = default)
    {
        if (trophies == null)
            return Result.Failure<bool, string>("Trophies cannot be null");

        try
        {
            foreach (var configTrophy in trophies)
            {
                if (string.IsNullOrWhiteSpace(configTrophy.Id) ||
                    string.IsNullOrWhiteSpace(configTrophy.Name) ||
                    string.IsNullOrWhiteSpace(configTrophy.Description) ||
                    string.IsNullOrWhiteSpace(configTrophy.Image))
                {
                    return Result.Failure<bool, string>($"Invalid trophy configuration: {configTrophy.Id}");
                }

                var exists = await _trophyRepository.ExistsAsync(configTrophy.Id, cancellationToken);
                if (!exists)
                {
                    var domainTrophy = new Domain.Entities.Trophy(
                        configTrophy.Id,
                        configTrophy.Name,
                        configTrophy.Description,
                        configTrophy.Image,
                        configTrophy.Visible);

                    if (!domainTrophy.IsValid())
                        return Result.Failure<bool, string>($"Invalid trophy data: {configTrophy.Id}");

                    await _trophyRepository.AddAsync(domainTrophy, cancellationToken);
                }
            }

            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to load trophies: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> LoadLevelsAsync(IEnumerable<LevelDefinition> levels, CancellationToken cancellationToken = default)
    {
        if (levels == null)
            return Result.Failure<bool, string>("Levels cannot be null");

        try
        {
            foreach (var configLevel in levels)
            {
                if (string.IsNullOrWhiteSpace(configLevel.Id) ||
                    string.IsNullOrWhiteSpace(configLevel.Name) ||
                    configLevel.Criteria == null ||
                    string.IsNullOrWhiteSpace(configLevel.Criteria.Category) ||
                    configLevel.Criteria.MinPoints < 0)
                {
                    return Result.Failure<bool, string>($"Invalid level configuration: {configLevel.Id}");
                }

                var exists = await _levelRepository.ExistsAsync(configLevel.Id, cancellationToken);
                if (!exists)
                {
                    var domainLevel = new Domain.Entities.Level(
                        configLevel.Id,
                        configLevel.Name,
                        configLevel.Criteria.Category,
                        configLevel.Criteria.MinPoints);

                    if (!domainLevel.IsValid())
                        return Result.Failure<bool, string>($"Invalid level data: {configLevel.Id}");

                    await _levelRepository.AddAsync(domainLevel, cancellationToken);
                }
            }

            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to load levels: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> LoadEventDefinitionsAsync(IEnumerable<Application.Configuration.EventDefinition> eventDefinitions, CancellationToken cancellationToken = default)
    {
        if (eventDefinitions == null)
            return Result.Failure<bool, string>("Event definitions cannot be null");

        try
        {
            foreach (var configEventDefinition in eventDefinitions)
            {
                if (string.IsNullOrWhiteSpace(configEventDefinition.Id) ||
                    string.IsNullOrWhiteSpace(configEventDefinition.Description))
                {
                    return Result.Failure<bool, string>($"Invalid event definition configuration: {configEventDefinition.Id}");
                }

                var exists = await _eventDefinitionRepository.ExistsAsync(configEventDefinition.Id, cancellationToken);
                if (!exists)
                {
                    var domainEventDefinition = new Domain.Entities.EventDefinition(
                        configEventDefinition.Id,
                        configEventDefinition.Description,
                        configEventDefinition.PayloadSchema);

                    await _eventDefinitionRepository.AddAsync(domainEventDefinition, cancellationToken);
                }
            }

            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to load event definitions: {ex.Message}");
        }
    }
}
