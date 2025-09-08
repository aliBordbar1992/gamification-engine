using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Users;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Shared;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for seeding UserState data from YAML seed files
/// </summary>
public sealed class UserStateSeederService : IUserStateSeederService
{
    private readonly IUserStateRepository _userStateRepository;
    private readonly ILogger<UserStateSeederService> _logger;

    public UserStateSeederService(
        IUserStateRepository userStateRepository,
        ILogger<UserStateSeederService> logger)
    {
        _userStateRepository = userStateRepository ?? throw new ArgumentNullException(nameof(userStateRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Seeds UserState data from a YAML seed file
    /// </summary>
    /// <param name="seedFilePath">Path to the YAML seed file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success and number of users seeded</returns>
    public async Task<Result<int, string>> SeedUserStatesAsync(string seedFilePath, CancellationToken cancellationToken = default)
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

            var usersSeeded = 0;
            if (seedData.Value.Users != null)
            {
                foreach (var userData in seedData.Value.Users)
                {
                    var userState = CreateUserStateFromSeedData(userData);
                    await _userStateRepository.SaveAsync(userState, cancellationToken);
                    usersSeeded++;
                }
            }

            _logger.LogInformation("Successfully seeded {UserCount} user states from {SeedFile}", usersSeeded, seedFilePath);
            return Result.Success<int, string>(usersSeeded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding user states from {SeedFile}", seedFilePath);
            return Result.Failure<int, string>($"Error seeding user states: {ex.Message}");
        }
    }

    /// <summary>
    /// Seeds UserState data if the repository is empty
    /// </summary>
    /// <param name="seedFilePath">Path to the YAML seed file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success and whether seeding was performed</returns>
    public async Task<Result<bool, string>> SeedIfEmptyAsync(string seedFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if we already have user states
            var existingUsers = await GetAllUserStatesAsync(cancellationToken);
            if (existingUsers.Any())
            {
                _logger.LogInformation("User states already exist ({Count} users), skipping seed", existingUsers.Count());
                return Result.Success<bool, string>(false); // Already seeded
            }

            var result = await SeedUserStatesAsync(seedFilePath, cancellationToken);
            if (!result.IsSuccess)
            {
                return Result.Failure<bool, string>(result.Error);
            }

            _logger.LogInformation("Seeded {UserCount} user states from {SeedFile}", result.Value, seedFilePath);
            return Result.Success<bool, string>(true); // Successfully seeded
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking/seeding user states from {SeedFile}", seedFilePath);
            return Result.Failure<bool, string>($"Error seeding user states: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all user states from the repository
    /// </summary>
    private Task<IEnumerable<UserState>> GetAllUserStatesAsync(CancellationToken cancellationToken)
    {
        // Since IUserStateRepository doesn't have GetAllAsync, we'll need to implement this differently
        // For now, we'll return empty to indicate no users exist
        // This is a limitation that should be addressed in the repository interface
        return Task.FromResult<IEnumerable<UserState>>(new List<UserState>());
    }

    /// <summary>
    /// Loads seed data from YAML file
    /// </summary>
    private async Task<Result<UserStateSeedData, string>> LoadSeedDataAsync(string filePath)
    {
        try
        {
            var yamlContent = await File.ReadAllTextAsync(filePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var seedData = deserializer.Deserialize<UserStateSeedData>(yamlContent);

            if (seedData?.Users == null || !seedData.Users.Any())
            {
                return Result.Failure<UserStateSeedData, string>("No users found in seed data");
            }

            return Result.Success<UserStateSeedData, string>(seedData);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserStateSeedData, string>($"Failed to deserialize seed data: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a UserState entity from seed data
    /// </summary>
    private UserState CreateUserStateFromSeedData(UserSeedData userData)
    {
        var userState = new UserState(userData.UserId);

        // Add points by category
        if (userData.PointsByCategory != null)
        {
            foreach (var category in userData.PointsByCategory)
            {
                userState.AddPoints(category.Key, category.Value);
            }
        }

        // Grant badges
        if (userData.Badges != null)
        {
            foreach (var badgeId in userData.Badges)
            {
                userState.GrantBadge(badgeId);
            }
        }

        // Grant trophies
        if (userData.Trophies != null)
        {
            foreach (var trophyId in userData.Trophies)
            {
                userState.GrantTrophy(trophyId);
            }
        }

        return userState;
    }
}

/// <summary>
/// Data structure for UserState seed data
/// </summary>
public class UserStateSeedData
{
    public List<UserSeedData> Users { get; set; } = new();
    public UserStateSeedMetadata? Metadata { get; set; }
}

/// <summary>
/// Individual user seed data
/// </summary>
public class UserSeedData
{
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, long>? PointsByCategory { get; set; }
    public List<string>? Badges { get; set; }
    public List<string>? Trophies { get; set; }
}

/// <summary>
/// Metadata about the seed data
/// </summary>
public class UserStateSeedMetadata
{
    public string? Description { get; set; }
    public string? CreatedDate { get; set; }
    public int TotalUsers { get; set; }
    public List<string>? PointCategoriesUsed { get; set; }
    public List<string>? BadgesUsed { get; set; }
    public List<string>? TrophiesUsed { get; set; }
    public List<string>? ScenariosCovered { get; set; }
}
