namespace GamificationEngine.Domain.Repositories;

using GamificationEngine.Domain.Users;

public interface IUserStateRepository
{
    Task<UserState?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task SaveAsync(UserState userState, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserState>> GetAllAsync(CancellationToken cancellationToken = default);
}