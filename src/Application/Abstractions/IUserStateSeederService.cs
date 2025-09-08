using GamificationEngine.Shared;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Service interface for seeding UserState data from seed files
/// </summary>
public interface IUserStateSeederService
{
    /// <summary>
    /// Seeds UserState data from a YAML seed file
    /// </summary>
    /// <param name="seedFilePath">Path to the YAML seed file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success and number of users seeded</returns>
    Task<Result<int, string>> SeedUserStatesAsync(string seedFilePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds UserState data if the repository is empty
    /// </summary>
    /// <param name="seedFilePath">Path to the YAML seed file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success and whether seeding was performed</returns>
    Task<Result<bool, string>> SeedIfEmptyAsync(string seedFilePath, CancellationToken cancellationToken = default);
}
