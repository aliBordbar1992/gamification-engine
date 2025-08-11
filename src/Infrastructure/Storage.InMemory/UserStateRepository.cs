using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Users;

namespace GamificationEngine.Infrastructure.Storage.InMemory;

public sealed class UserStateRepository : IUserStateRepository
{
    private readonly Dictionary<string, UserState> _store = new(StringComparer.Ordinal);

    public Task<UserState?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(userId, out var state);
        return Task.FromResult(state);
    }

    public Task SaveAsync(UserState userState, CancellationToken cancellationToken = default)
    {
        _store[userState.UserId] = userState;
        return Task.CompletedTask;
    }
}
