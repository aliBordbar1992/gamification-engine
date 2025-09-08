using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for seeding the database with initial configuration data
/// </summary>
public interface IDatabaseSeederService
{
    /// <summary>
    /// Seeds the database with configuration data if it's empty
    /// </summary>
    /// <param name="configurationPath">Path to the configuration file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success and whether seeding was performed</returns>
    Task<Result<bool, string>> SeedIfEmptyAsync(string configurationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds UserState data from seed file if repository is empty
    /// </summary>
    /// <param name="userStateSeedFilePath">Path to the UserState seed file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success and whether seeding was performed</returns>
    Task<Result<bool, string>> SeedUserStatesIfEmptyAsync(string userStateSeedFilePath, CancellationToken cancellationToken = default);
}
