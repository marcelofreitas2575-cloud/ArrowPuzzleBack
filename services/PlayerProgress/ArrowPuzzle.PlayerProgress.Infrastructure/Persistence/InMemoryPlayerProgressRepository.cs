using System.Collections.Concurrent;
using ArrowPuzzle.PlayerProgress.Application.Ports;
using ArrowPuzzle.PlayerProgress.Domain;

namespace ArrowPuzzle.PlayerProgress.Infrastructure.Persistence;

public sealed class InMemoryPlayerProgressRepository : IPlayerProgressRepository
{
    private readonly ConcurrentDictionary<string, PlayerProgressState> _store = new(StringComparer.Ordinal);

    public Task<PlayerProgressState?> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(userId, out var state);
        return Task.FromResult(state);
    }

    public Task SaveAsync(PlayerProgressState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        _store[state.UserId] = state;
        return Task.CompletedTask;
    }
}
