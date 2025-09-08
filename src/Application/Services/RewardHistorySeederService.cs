using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Shared;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for seeding RewardHistory data from YAML seed files
/// </summary>
public sealed class RewardHistorySeederService
{
    private readonly IRewardHistoryRepository _rewardHistoryRepository;
    private readonly ILogger<RewardHistorySeederService> _logger;

    public RewardHistorySeederService(
        IRewardHistoryRepository rewardHistoryRepository,
        ILogger<RewardHistorySeederService> logger)
    {
        _rewardHistoryRepository = rewardHistoryRepository ?? throw new ArgumentNullException(nameof(rewardHistoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Seeds RewardHistory data from a YAML seed file
    /// </summary>
    /// <param name="seedFilePath">Path to the YAML seed file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success and number of entries seeded</returns>
    public async Task<Result<int, string>> SeedRewardHistoryAsync(string seedFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(seedFilePath))
            {
                return Result.Failure<int, string>($"Seed file not found: {seedFilePath}");
            }

            var seedData = await LoadSeedDataAsync(seedFilePath);
            if (!seedData.IsSuccess)
            {
                return Result.Failure<int, string>($"Failed to load seed data: {seedData.Error}");
            }

            var entriesSeeded = 0;
            if (seedData.Value.RewardHistory != null)
            {
                foreach (var rewardHistoryData in seedData.Value.RewardHistory)
                {
                    var rewardHistory = CreateRewardHistoryFromSeedData(rewardHistoryData);
                    await _rewardHistoryRepository.StoreAsync(rewardHistory, cancellationToken);
                    entriesSeeded++;
                }
            }

            _logger.LogInformation("Successfully seeded {EntryCount} reward history entries from {SeedFile}", entriesSeeded, seedFilePath);
            return Result.Success<int, string>(entriesSeeded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding reward history from {SeedFile}", seedFilePath);
            return Result.Failure<int, string>($"Error seeding reward history: {ex.Message}");
        }
    }

    /// <summary>
    /// Seeds RewardHistory data if the repository is empty
    /// </summary>
    /// <param name="seedFilePath">Path to the YAML seed file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success and whether seeding was performed</returns>
    public async Task<Result<bool, string>> SeedIfEmptyAsync(string seedFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if we already have reward history entries
            var existingEntries = await GetAllRewardHistoryAsync(cancellationToken);
            if (existingEntries.Any())
            {
                _logger.LogInformation("Reward history already exists ({Count} entries), skipping seed", existingEntries.Count());
                return Result.Success<bool, string>(false); // Already seeded
            }

            var result = await SeedRewardHistoryAsync(seedFilePath, cancellationToken);
            if (!result.IsSuccess)
            {
                return Result.Failure<bool, string>(result.Error);
            }

            _logger.LogInformation("Seeded {EntryCount} reward history entries from {SeedFile}", result.Value, seedFilePath);
            return Result.Success<bool, string>(true); // Seeding was performed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking and seeding reward history from {SeedFile}", seedFilePath);
            return Result.Failure<bool, string>($"Error checking and seeding reward history: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads seed data from a YAML file
    /// </summary>
    private async Task<Result<RewardHistorySeedData, string>> LoadSeedDataAsync(string seedFilePath)
    {
        try
        {
            var yamlContent = await File.ReadAllTextAsync(seedFilePath);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var seedData = deserializer.Deserialize<RewardHistorySeedData>(yamlContent);
            return Result.Success<RewardHistorySeedData, string>(seedData);
        }
        catch (Exception ex)
        {
            return Result.Failure<RewardHistorySeedData, string>($"Failed to deserialize seed data: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all reward history entries (for checking if already seeded)
    /// </summary>
    private async Task<IEnumerable<RewardHistory>> GetAllRewardHistoryAsync(CancellationToken cancellationToken)
    {
        // Since we don't have a GetAll method in the interface, we'll use a date range that covers all possible dates
        var startDate = DateTimeOffset.MinValue;
        var endDate = DateTimeOffset.MaxValue;
        return await _rewardHistoryRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
    }

    /// <summary>
    /// Creates a RewardHistory entity from seed data
    /// </summary>
    private RewardHistory CreateRewardHistoryFromSeedData(RewardHistorySeedEntry rewardHistoryData)
    {
        var details = new Dictionary<string, object>();
        if (rewardHistoryData.Details != null)
        {
            foreach (var detail in rewardHistoryData.Details)
            {
                details[detail.Key] = detail.Value;
            }
        }

        return new RewardHistory(
            rewardHistoryData.RewardHistoryId,
            rewardHistoryData.UserId,
            rewardHistoryData.RewardId,
            rewardHistoryData.RewardType,
            rewardHistoryData.TriggerEventId,
            rewardHistoryData.AwardedAt,
            rewardHistoryData.Success,
            rewardHistoryData.Message,
            details);
    }
}

/// <summary>
/// Data structure for RewardHistory seed data
/// </summary>
public class RewardHistorySeedData
{
    public List<RewardHistorySeedEntry> RewardHistory { get; set; } = new();
    public RewardHistorySeedMetadata? Metadata { get; set; }
}

/// <summary>
/// Individual reward history seed entry
/// </summary>
public class RewardHistorySeedEntry
{
    public string RewardHistoryId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string RewardId { get; set; } = string.Empty;
    public string RewardType { get; set; } = string.Empty;
    public string TriggerEventId { get; set; } = string.Empty;
    public DateTimeOffset AwardedAt { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

/// <summary>
/// Metadata about the reward history seed data
/// </summary>
public class RewardHistorySeedMetadata
{
    public string? Description { get; set; }
    public string? CreatedDate { get; set; }
    public int TotalEntries { get; set; }
    public DateTimeOffset? TimeRangeStart { get; set; }
    public DateTimeOffset? TimeRangeEnd { get; set; }
    public List<string>? RewardTypesUsed { get; set; }
    public List<string>? PointCategoriesUsed { get; set; }
    public List<string>? BadgesAwarded { get; set; }
    public List<string>? UsersCovered { get; set; }
}
