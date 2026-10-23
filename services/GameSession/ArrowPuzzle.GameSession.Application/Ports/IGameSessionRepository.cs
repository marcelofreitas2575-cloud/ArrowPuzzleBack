using ArrowPuzzle.GameSession.Domain;

namespace ArrowPuzzle.GameSession.Application.Ports;

public interface IGameSessionRepository
{
    Task AddAsync(PlaySession session, CancellationToken cancellationToken = default);

    Task<PlaySession?> GetAsync(SessionId sessionId, CancellationToken cancellationToken = default);

    Task SaveAsync(PlaySession session, CancellationToken cancellationToken = default);
}
