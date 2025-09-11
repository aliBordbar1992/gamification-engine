using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for managing gamification entities
/// </summary>
public sealed class EntityManagementService : IEntityManagementService
{
    private readonly IPointCategoryRepository _pointCategoryRepository;
    private readonly IBadgeRepository _badgeRepository;
    private readonly ITrophyRepository _trophyRepository;
    private readonly ILevelRepository _levelRepository;

    public EntityManagementService(
        IPointCategoryRepository pointCategoryRepository,
        IBadgeRepository badgeRepository,
        ITrophyRepository trophyRepository,
        ILevelRepository levelRepository)
    {
        _pointCategoryRepository = pointCategoryRepository ?? throw new ArgumentNullException(nameof(pointCategoryRepository));
        _badgeRepository = badgeRepository ?? throw new ArgumentNullException(nameof(badgeRepository));
        _trophyRepository = trophyRepository ?? throw new ArgumentNullException(nameof(trophyRepository));
        _levelRepository = levelRepository ?? throw new ArgumentNullException(nameof(levelRepository));
    }

    #region Point Categories

    public async Task<Result<IEnumerable<PointCategoryDto>, string>> GetAllPointCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _pointCategoryRepository.GetAllAsync(cancellationToken);
            var dtos = categories.Select(MapToDto);
            return Result.Success<IEnumerable<PointCategoryDto>, string>(dtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<PointCategoryDto>, string>($"Failed to get point categories: {ex.Message}");
        }
    }

    public async Task<Result<PointCategoryDto, string>> GetPointCategoryByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<PointCategoryDto, string>("ID cannot be empty");

        try
        {
            var category = await _pointCategoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null)
                return Result.Failure<PointCategoryDto, string>($"Point category with ID '{id}' not found");

            return Result.Success<PointCategoryDto, string>(MapToDto(category));
        }
        catch (Exception ex)
        {
            return Result.Failure<PointCategoryDto, string>($"Failed to get point category: {ex.Message}");
        }
    }

    public async Task<Result<PointCategoryDto, string>> CreatePointCategoryAsync(CreatePointCategoryDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Result.Failure<PointCategoryDto, string>("DTO cannot be null");

        try
        {
            var exists = await _pointCategoryRepository.ExistsAsync(dto.Id, cancellationToken);
            if (exists)
                return Result.Failure<PointCategoryDto, string>($"Point category with ID '{dto.Id}' already exists");

            var category = new PointCategory(dto.Id, dto.Name, dto.Description, dto.Aggregation.ToPointCategoryAggregation(), dto.IsSpendable, dto.NegativeBalanceAllowed);
            if (!category.IsValid())
                return Result.Failure<PointCategoryDto, string>("Invalid point category data");

            await _pointCategoryRepository.AddAsync(category, cancellationToken);
            return Result.Success<PointCategoryDto, string>(MapToDto(category));
        }
        catch (Exception ex)
        {
            return Result.Failure<PointCategoryDto, string>($"Failed to create point category: {ex.Message}");
        }
    }

    public async Task<Result<PointCategoryDto, string>> UpdatePointCategoryAsync(string id, UpdatePointCategoryDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<PointCategoryDto, string>("ID cannot be empty");
        if (dto == null)
            return Result.Failure<PointCategoryDto, string>("DTO cannot be null");

        try
        {
            var category = await _pointCategoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null)
                return Result.Failure<PointCategoryDto, string>($"Point category with ID '{id}' not found");

            category.UpdateInfo(dto.Name, dto.Description, dto.Aggregation.ToPointCategoryAggregation(), dto.IsSpendable, dto.NegativeBalanceAllowed);
            if (!category.IsValid())
                return Result.Failure<PointCategoryDto, string>("Invalid point category data");

            await _pointCategoryRepository.UpdateAsync(category, cancellationToken);
            return Result.Success<PointCategoryDto, string>(MapToDto(category));
        }
        catch (Exception ex)
        {
            return Result.Failure<PointCategoryDto, string>($"Failed to update point category: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> DeletePointCategoryAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<bool, string>("ID cannot be empty");

        try
        {
            var exists = await _pointCategoryRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
                return Result.Failure<bool, string>($"Point category with ID '{id}' not found");

            await _pointCategoryRepository.DeleteAsync(id, cancellationToken);
            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to delete point category: {ex.Message}");
        }
    }

    #endregion

    #region Badges

    public async Task<Result<IEnumerable<BadgeDto>, string>> GetAllBadgesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var badges = await _badgeRepository.GetAllAsync(cancellationToken);
            var dtos = badges.Select(MapToDto);
            return Result.Success<IEnumerable<BadgeDto>, string>(dtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<BadgeDto>, string>($"Failed to get badges: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BadgeDto>, string>> GetVisibleBadgesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var badges = await _badgeRepository.GetVisibleAsync(cancellationToken);
            var dtos = badges.Select(MapToDto);
            return Result.Success<IEnumerable<BadgeDto>, string>(dtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<BadgeDto>, string>($"Failed to get visible badges: {ex.Message}");
        }
    }

    public async Task<Result<BadgeDto, string>> GetBadgeByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<BadgeDto, string>("ID cannot be empty");

        try
        {
            var badge = await _badgeRepository.GetByIdAsync(id, cancellationToken);
            if (badge == null)
                return Result.Failure<BadgeDto, string>($"Badge with ID '{id}' not found");

            return Result.Success<BadgeDto, string>(MapToDto(badge));
        }
        catch (Exception ex)
        {
            return Result.Failure<BadgeDto, string>($"Failed to get badge: {ex.Message}");
        }
    }

    public async Task<Result<BadgeDto, string>> CreateBadgeAsync(CreateBadgeDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Result.Failure<BadgeDto, string>("DTO cannot be null");

        try
        {
            var exists = await _badgeRepository.ExistsAsync(dto.Id, cancellationToken);
            if (exists)
                return Result.Failure<BadgeDto, string>($"Badge with ID '{dto.Id}' already exists");

            var badge = new Badge(dto.Id, dto.Name, dto.Description, dto.Image, dto.Visible);
            if (!badge.IsValid())
                return Result.Failure<BadgeDto, string>("Invalid badge data");

            await _badgeRepository.AddAsync(badge, cancellationToken);
            return Result.Success<BadgeDto, string>(MapToDto(badge));
        }
        catch (Exception ex)
        {
            return Result.Failure<BadgeDto, string>($"Failed to create badge: {ex.Message}");
        }
    }

    public async Task<Result<BadgeDto, string>> UpdateBadgeAsync(string id, UpdateBadgeDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<BadgeDto, string>("ID cannot be empty");
        if (dto == null)
            return Result.Failure<BadgeDto, string>("DTO cannot be null");

        try
        {
            var badge = await _badgeRepository.GetByIdAsync(id, cancellationToken);
            if (badge == null)
                return Result.Failure<BadgeDto, string>($"Badge with ID '{id}' not found");

            badge.UpdateInfo(dto.Name, dto.Description, dto.Image, dto.Visible);
            if (!badge.IsValid())
                return Result.Failure<BadgeDto, string>("Invalid badge data");

            await _badgeRepository.UpdateAsync(badge, cancellationToken);
            return Result.Success<BadgeDto, string>(MapToDto(badge));
        }
        catch (Exception ex)
        {
            return Result.Failure<BadgeDto, string>($"Failed to update badge: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> DeleteBadgeAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<bool, string>("ID cannot be empty");

        try
        {
            var exists = await _badgeRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
                return Result.Failure<bool, string>($"Badge with ID '{id}' not found");

            await _badgeRepository.DeleteAsync(id, cancellationToken);
            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to delete badge: {ex.Message}");
        }
    }

    #endregion

    #region Trophies

    public async Task<Result<IEnumerable<TrophyDto>, string>> GetAllTrophiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var trophies = await _trophyRepository.GetAllAsync(cancellationToken);
            var dtos = trophies.Select(MapToDto);
            return Result.Success<IEnumerable<TrophyDto>, string>(dtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<TrophyDto>, string>($"Failed to get trophies: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TrophyDto>, string>> GetVisibleTrophiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var trophies = await _trophyRepository.GetVisibleAsync(cancellationToken);
            var dtos = trophies.Select(MapToDto);
            return Result.Success<IEnumerable<TrophyDto>, string>(dtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<TrophyDto>, string>($"Failed to get visible trophies: {ex.Message}");
        }
    }

    public async Task<Result<TrophyDto, string>> GetTrophyByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<TrophyDto, string>("ID cannot be empty");

        try
        {
            var trophy = await _trophyRepository.GetByIdAsync(id, cancellationToken);
            if (trophy == null)
                return Result.Failure<TrophyDto, string>($"Trophy with ID '{id}' not found");

            return Result.Success<TrophyDto, string>(MapToDto(trophy));
        }
        catch (Exception ex)
        {
            return Result.Failure<TrophyDto, string>($"Failed to get trophy: {ex.Message}");
        }
    }

    public async Task<Result<TrophyDto, string>> CreateTrophyAsync(CreateTrophyDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Result.Failure<TrophyDto, string>("DTO cannot be null");

        try
        {
            var exists = await _trophyRepository.ExistsAsync(dto.Id, cancellationToken);
            if (exists)
                return Result.Failure<TrophyDto, string>($"Trophy with ID '{dto.Id}' already exists");

            var trophy = new Trophy(dto.Id, dto.Name, dto.Description, dto.Image, dto.Visible);
            if (!trophy.IsValid())
                return Result.Failure<TrophyDto, string>("Invalid trophy data");

            await _trophyRepository.AddAsync(trophy, cancellationToken);
            return Result.Success<TrophyDto, string>(MapToDto(trophy));
        }
        catch (Exception ex)
        {
            return Result.Failure<TrophyDto, string>($"Failed to create trophy: {ex.Message}");
        }
    }

    public async Task<Result<TrophyDto, string>> UpdateTrophyAsync(string id, UpdateTrophyDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<TrophyDto, string>("ID cannot be empty");
        if (dto == null)
            return Result.Failure<TrophyDto, string>("DTO cannot be null");

        try
        {
            var trophy = await _trophyRepository.GetByIdAsync(id, cancellationToken);
            if (trophy == null)
                return Result.Failure<TrophyDto, string>($"Trophy with ID '{id}' not found");

            trophy.UpdateInfo(dto.Name, dto.Description, dto.Image, dto.Visible);
            if (!trophy.IsValid())
                return Result.Failure<TrophyDto, string>("Invalid trophy data");

            await _trophyRepository.UpdateAsync(trophy, cancellationToken);
            return Result.Success<TrophyDto, string>(MapToDto(trophy));
        }
        catch (Exception ex)
        {
            return Result.Failure<TrophyDto, string>($"Failed to update trophy: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> DeleteTrophyAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<bool, string>("ID cannot be empty");

        try
        {
            var exists = await _trophyRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
                return Result.Failure<bool, string>($"Trophy with ID '{id}' not found");

            await _trophyRepository.DeleteAsync(id, cancellationToken);
            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to delete trophy: {ex.Message}");
        }
    }

    #endregion

    #region Levels

    public async Task<Result<IEnumerable<LevelDto>, string>> GetAllLevelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var levels = await _levelRepository.GetAllAsync(cancellationToken);
            var dtos = levels.Select(MapToDto);
            return Result.Success<IEnumerable<LevelDto>, string>(dtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<LevelDto>, string>($"Failed to get levels: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<LevelDto>, string>> GetLevelsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category))
            return Result.Failure<IEnumerable<LevelDto>, string>("Category cannot be empty");

        try
        {
            var levels = await _levelRepository.GetByCategoryOrderedAsync(category, cancellationToken);
            var dtos = levels.Select(MapToDto);
            return Result.Success<IEnumerable<LevelDto>, string>(dtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<LevelDto>, string>($"Failed to get levels by category: {ex.Message}");
        }
    }

    public async Task<Result<LevelDto, string>> GetLevelByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<LevelDto, string>("ID cannot be empty");

        try
        {
            var level = await _levelRepository.GetByIdAsync(id, cancellationToken);
            if (level == null)
                return Result.Failure<LevelDto, string>($"Level with ID '{id}' not found");

            return Result.Success<LevelDto, string>(MapToDto(level));
        }
        catch (Exception ex)
        {
            return Result.Failure<LevelDto, string>($"Failed to get level: {ex.Message}");
        }
    }

    public async Task<Result<LevelDto, string>> CreateLevelAsync(CreateLevelDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Result.Failure<LevelDto, string>("DTO cannot be null");

        try
        {
            var exists = await _levelRepository.ExistsAsync(dto.Id, cancellationToken);
            if (exists)
                return Result.Failure<LevelDto, string>($"Level with ID '{dto.Id}' already exists");

            //TODO: dto.category should exist as a  point category

            var level = new Level(dto.Id, dto.Name, dto.Category, dto.MinPoints);
            if (!level.IsValid())
                return Result.Failure<LevelDto, string>("Invalid level data");

            await _levelRepository.AddAsync(level, cancellationToken);
            return Result.Success<LevelDto, string>(MapToDto(level));
        }
        catch (Exception ex)
        {
            return Result.Failure<LevelDto, string>($"Failed to create level: {ex.Message}");
        }
    }

    public async Task<Result<LevelDto, string>> UpdateLevelAsync(string id, UpdateLevelDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<LevelDto, string>("ID cannot be empty");
        if (dto == null)
            return Result.Failure<LevelDto, string>("DTO cannot be null");

        try
        {
            var level = await _levelRepository.GetByIdAsync(id, cancellationToken);
            if (level == null)
                return Result.Failure<LevelDto, string>($"Level with ID '{id}' not found");

            //TODO: dto.category should exist as a  point category

            level.UpdateInfo(dto.Name, dto.Category, dto.MinPoints);
            if (!level.IsValid())
                return Result.Failure<LevelDto, string>("Invalid level data");

            await _levelRepository.UpdateAsync(level, cancellationToken);
            return Result.Success<LevelDto, string>(MapToDto(level));
        }
        catch (Exception ex)
        {
            return Result.Failure<LevelDto, string>($"Failed to update level: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> DeleteLevelAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<bool, string>("ID cannot be empty");

        try
        {
            var exists = await _levelRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
                return Result.Failure<bool, string>($"Level with ID '{id}' not found");

            await _levelRepository.DeleteAsync(id, cancellationToken);
            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to delete level: {ex.Message}");
        }
    }

    #endregion

    #region Mapping Methods

    private static PointCategoryDto MapToDto(PointCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        Aggregation = category.Aggregation.ToAggregationString(),
        IsSpendable = category.IsSpendable,
        NegativeBalanceAllowed = category.NegativeBalanceAllowed
    };

    private static BadgeDto MapToDto(Badge badge) => new()
    {
        Id = badge.Id,
        Name = badge.Name,
        Description = badge.Description,
        Image = badge.Image,
        Visible = badge.Visible
    };

    private static TrophyDto MapToDto(Trophy trophy) => new()
    {
        Id = trophy.Id,
        Name = trophy.Name,
        Description = trophy.Description,
        Image = trophy.Image,
        Visible = trophy.Visible
    };

    private static LevelDto MapToDto(Level level) => new()
    {
        Id = level.Id,
        Name = level.Name,
        Category = level.Category,
        MinPoints = level.MinPoints
    };

    #endregion
}
