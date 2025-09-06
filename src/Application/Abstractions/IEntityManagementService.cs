using GamificationEngine.Application.DTOs;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for managing gamification entities (point categories, badges, trophies, levels)
/// </summary>
public interface IEntityManagementService
{
    // Point Categories
    Task<Result<IEnumerable<PointCategoryDto>, string>> GetAllPointCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Result<PointCategoryDto, string>> GetPointCategoryByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<PointCategoryDto, string>> CreatePointCategoryAsync(CreatePointCategoryDto dto, CancellationToken cancellationToken = default);
    Task<Result<PointCategoryDto, string>> UpdatePointCategoryAsync(string id, UpdatePointCategoryDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool, string>> DeletePointCategoryAsync(string id, CancellationToken cancellationToken = default);

    // Badges
    Task<Result<IEnumerable<BadgeDto>, string>> GetAllBadgesAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BadgeDto>, string>> GetVisibleBadgesAsync(CancellationToken cancellationToken = default);
    Task<Result<BadgeDto, string>> GetBadgeByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<BadgeDto, string>> CreateBadgeAsync(CreateBadgeDto dto, CancellationToken cancellationToken = default);
    Task<Result<BadgeDto, string>> UpdateBadgeAsync(string id, UpdateBadgeDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool, string>> DeleteBadgeAsync(string id, CancellationToken cancellationToken = default);

    // Trophies
    Task<Result<IEnumerable<TrophyDto>, string>> GetAllTrophiesAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TrophyDto>, string>> GetVisibleTrophiesAsync(CancellationToken cancellationToken = default);
    Task<Result<TrophyDto, string>> GetTrophyByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<TrophyDto, string>> CreateTrophyAsync(CreateTrophyDto dto, CancellationToken cancellationToken = default);
    Task<Result<TrophyDto, string>> UpdateTrophyAsync(string id, UpdateTrophyDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool, string>> DeleteTrophyAsync(string id, CancellationToken cancellationToken = default);

    // Levels
    Task<Result<IEnumerable<LevelDto>, string>> GetAllLevelsAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<LevelDto>, string>> GetLevelsByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<Result<LevelDto, string>> GetLevelByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<LevelDto, string>> CreateLevelAsync(CreateLevelDto dto, CancellationToken cancellationToken = default);
    Task<Result<LevelDto, string>> UpdateLevelAsync(string id, UpdateLevelDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool, string>> DeleteLevelAsync(string id, CancellationToken cancellationToken = default);
}
