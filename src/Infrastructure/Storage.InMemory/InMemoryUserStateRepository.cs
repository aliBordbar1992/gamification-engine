using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Users;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

/// <summary>
/// In-memory implementation of IUserStateRepository
/// </summary>
public class InMemoryUserStateRepository : IUserStateRepository
{
    private readonly Dictionary<string, UserState> _userStates = new();

    public Task<UserState?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        _userStates.TryGetValue(userId, out var userState);
        return Task.FromResult(userState);
    }

    public Task SaveAsync(UserState userState, CancellationToken cancellationToken = default)
    {
        _userStates[userState.UserId] = userState;
        return Task.CompletedTask;
    }
}
