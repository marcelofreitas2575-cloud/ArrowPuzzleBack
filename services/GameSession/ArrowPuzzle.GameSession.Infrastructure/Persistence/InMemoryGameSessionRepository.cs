using System.Collections.Concurrent;
using ArrowPuzzle.GameSession.Application.Ports;
using ArrowPuzzle.GameSession.Domain;

namespace ArrowPuzzle.GameSession.Infrastructure.Persistence;

public sealed class InMemoryGameSessionRepository : IGameSessionRepository
{
    private readonly ConcurrentDictionary<SessionId, PlaySession> _store = new();

    public Task AddAsync(PlaySession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!_store.TryAdd(session.Id, session))
        {
            throw new InvalidOperationException($"Session '{session.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<PlaySession?> GetAsync(SessionId sessionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.GetValueOrDefault(sessionId));

    public Task SaveAsync(PlaySession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!_store.ContainsKey(session.Id))
        {
            throw new KeyNotFoundException($"Session '{session.Id}' was not found.");
        }

        _store[session.Id] = session;
        return Task.CompletedTask;
    }
}
