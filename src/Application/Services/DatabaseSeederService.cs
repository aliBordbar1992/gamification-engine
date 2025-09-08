using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.Configuration;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for seeding the database with initial configuration data
/// </summary>
public sealed class DatabaseSeederService : IDatabaseSeederService
{
    private readonly IPointCategoryRepository _pointCategoryRepository;
    private readonly IBadgeRepository _badgeRepository;
    private readonly ITrophyRepository _trophyRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IRuleRepository _ruleRepository;
    private readonly IConfigurationLoader _configurationLoader;
    private readonly IUserStateSeederService _userStateSeederService;
    private readonly ConditionFactory _conditionFactory;
    private readonly RewardFactory _rewardFactory;

    public DatabaseSeederService(
        IPointCategoryRepository pointCategoryRepository,
        IBadgeRepository badgeRepository,
        ITrophyRepository trophyRepository,
        ILevelRepository levelRepository,
        IRuleRepository ruleRepository,
        IConfigurationLoader configurationLoader,
        IUserStateSeederService userStateSeederService)
    {
        _pointCategoryRepository = pointCategoryRepository ?? throw new ArgumentNullException(nameof(pointCategoryRepository));
        _badgeRepository = badgeRepository ?? throw new ArgumentNullException(nameof(badgeRepository));
        _trophyRepository = trophyRepository ?? throw new ArgumentNullException(nameof(trophyRepository));
        _levelRepository = levelRepository ?? throw new ArgumentNullException(nameof(levelRepository));
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _configurationLoader = configurationLoader ?? throw new ArgumentNullException(nameof(configurationLoader));
        _userStateSeederService = userStateSeederService ?? throw new ArgumentNullException(nameof(userStateSeederService));
        _conditionFactory = new ConditionFactory();
        _rewardFactory = new RewardFactory();
    }

    /// <summary>
    /// Seeds the database with configuration data if it's empty
    /// </summary>
    public async Task<Result<bool, string>> SeedIfEmptyAsync(string configurationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if database is already seeded
            var isSeeded = await IsDatabaseSeededAsync(cancellationToken);
            if (isSeeded)
            {
                return Result.Success<bool, string>(false); // Already seeded
            }

            // Load configuration
            var configResult = await _configurationLoader.LoadFromFileAsync(configurationPath, cancellationToken);
            if (!configResult.IsSuccess)
            {
                return Result.Failure<bool, string>($"Failed to load configuration: {configResult.Error}");
            }

            var config = configResult.Value;

            // Seed point categories
            await SeedPointCategoriesAsync(config.PointCategories, cancellationToken);

            // Seed badges
            await SeedBadgesAsync(config.Badges, cancellationToken);

            // Seed trophies
            await SeedTrophiesAsync(config.Trophies, cancellationToken);

            // Seed levels
            await SeedLevelsAsync(config.Levels, cancellationToken);

            // Seed rules
            Console.WriteLine($"🔧 Seeding {config.Rules.Count} rules...");
            await SeedRulesAsync(config.Rules, cancellationToken);
            Console.WriteLine("✅ Rules seeding completed.");

            return Result.Success<bool, string>(true); // Successfully seeded
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Error seeding database: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the database has been seeded with initial data
    /// </summary>
    private async Task<bool> IsDatabaseSeededAsync(CancellationToken cancellationToken)
    {
        var pointCategories = await _pointCategoryRepository.GetAllAsync(cancellationToken);
        return pointCategories.Any();
    }

    /// <summary>
    /// Seeds point categories from configuration
    /// </summary>
    private async Task SeedPointCategoriesAsync(IEnumerable<Application.Configuration.PointCategory> pointCategories, CancellationToken cancellationToken)
    {
        foreach (var categoryConfig in pointCategories)
        {
            var exists = await _pointCategoryRepository.ExistsAsync(categoryConfig.Id, cancellationToken);
            if (!exists)
            {
                var category = new Domain.Entities.PointCategory(
                    categoryConfig.Id,
                    categoryConfig.Name,
                    categoryConfig.Description,
                    categoryConfig.Aggregation);

                await _pointCategoryRepository.AddAsync(category, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Seeds badges from configuration
    /// </summary>
    private async Task SeedBadgesAsync(IEnumerable<BadgeDefinition> badges, CancellationToken cancellationToken)
    {
        foreach (var badgeConfig in badges)
        {
            var exists = await _badgeRepository.ExistsAsync(badgeConfig.Id, cancellationToken);
            if (!exists)
            {
                var badge = new Badge(
                    badgeConfig.Id,
                    badgeConfig.Name,
                    badgeConfig.Description,
                    badgeConfig.Image,
                    badgeConfig.Visible);

                await _badgeRepository.AddAsync(badge, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Seeds trophies from configuration
    /// </summary>
    private async Task SeedTrophiesAsync(IEnumerable<TrophyDefinition> trophies, CancellationToken cancellationToken)
    {
        foreach (var trophyConfig in trophies)
        {
            var exists = await _trophyRepository.ExistsAsync(trophyConfig.Id, cancellationToken);
            if (!exists)
            {
                var trophy = new Trophy(
                    trophyConfig.Id,
                    trophyConfig.Name,
                    trophyConfig.Description,
                    trophyConfig.Image,
                    trophyConfig.Visible);

                await _trophyRepository.AddAsync(trophy, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Seeds levels from configuration
    /// </summary>
    private async Task SeedLevelsAsync(IEnumerable<LevelDefinition> levels, CancellationToken cancellationToken)
    {
        foreach (var levelConfig in levels)
        {
            var exists = await _levelRepository.ExistsAsync(levelConfig.Id, cancellationToken);
            if (!exists)
            {
                var level = new Level(
                    levelConfig.Id,
                    levelConfig.Name,
                    levelConfig.Criteria.Category,
                    levelConfig.Criteria.MinPoints);

                await _levelRepository.AddAsync(level, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Seeds rules from configuration
    /// </summary>
    private async Task SeedRulesAsync(IEnumerable<RuleDefinition> rules, CancellationToken cancellationToken)
    {
        foreach (var ruleConfig in rules)
        {
            Console.WriteLine($"🔧 Processing rule: {ruleConfig.Id} - {ruleConfig.Name}");
            var exists = await _ruleRepository.ExistsAsync(ruleConfig.Id);
            if (!exists)
            {
                try
                {
                    var triggers = ruleConfig.Triggers.Select(t => t.Event).ToArray();
                    var conditions = ConvertConditions(ruleConfig.Conditions);
                    var rewards = ConvertRewards(ruleConfig.Rewards);

                    var rule = new Rule(
                        ruleConfig.Id,
                        ruleConfig.Name,
                        triggers,
                        conditions,
                        rewards,
                        isActive: true,
                        ruleConfig.Description);

                    await _ruleRepository.StoreAsync(rule);
                    Console.WriteLine($"✅ Successfully seeded rule: {ruleConfig.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to seed rule {ruleConfig.Id}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"ℹ️ Rule {ruleConfig.Id} already exists, skipping.");
            }
        }
    }

    /// <summary>
    /// Converts configuration conditions to domain conditions
    /// </summary>
    private IReadOnlyCollection<Condition> ConvertConditions(IEnumerable<RuleCondition> conditions)
    {
        var result = new List<Condition>();

        foreach (var conditionConfig in conditions)
        {
            var conditionId = Guid.NewGuid().ToString();
            var condition = _conditionFactory.CreateCondition(
                conditionId,
                conditionConfig.Type,
                ConvertParameters(conditionConfig.Parameters));

            result.Add(condition);
        }

        return result;
    }

    /// <summary>
    /// Converts configuration rewards to domain rewards
    /// </summary>
    private IReadOnlyCollection<Reward> ConvertRewards(IEnumerable<RuleReward> rewards)
    {
        var result = new List<Reward>();

        foreach (var rewardConfig in rewards)
        {
            var rewardId = Guid.NewGuid().ToString();
            var reward = _rewardFactory.CreateReward(
                rewardId,
                rewardConfig.Type,
                ConvertParameters(rewardConfig.Parameters));

            result.Add(reward);
        }

        return result;
    }

    /// <summary>
    /// Converts dictionary parameters to a more structured format
    /// </summary>
    private Dictionary<string, object> ConvertParameters(Dictionary<string, object> parameters)
    {
        // For now, return as-is. In a more sophisticated implementation,
        // we might want to validate and transform the parameters based on type
        return parameters;
    }

    /// <summary>
    /// Seeds UserState data from seed file if repository is empty
    /// </summary>
    /// <param name="userStateSeedFilePath">Path to the UserState seed file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success and whether seeding was performed</returns>
    public async Task<Result<bool, string>> SeedUserStatesIfEmptyAsync(string userStateSeedFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(userStateSeedFilePath))
            {
                return Result.Failure<bool, string>($"UserState seed file not found: {userStateSeedFilePath}");
            }

            var result = await _userStateSeederService.SeedIfEmptyAsync(userStateSeedFilePath, cancellationToken);
            if (!result.IsSuccess)
            {
                return Result.Failure<bool, string>($"Failed to seed UserState data: {result.Error}");
            }

            if (result.Value)
            {
                Console.WriteLine("✅ UserState seeding completed.");
            }
            else
            {
                Console.WriteLine("ℹ️ UserState data already exists, skipping seeding.");
            }

            return Result.Success<bool, string>(result.Value);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Error seeding UserState data: {ex.Message}");
        }
    }
}
