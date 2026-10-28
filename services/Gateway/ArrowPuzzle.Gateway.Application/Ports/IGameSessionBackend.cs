using ArrowPuzzle.Gateway.Application.Models;

namespace ArrowPuzzle.Gateway.Application.Ports;

public interface IGameSessionBackend
{
    Task<SessionDto> CreateSessionAsync(
        string userId,
        string levelId,
        BoardDto initialBoard,
        CancellationToken cancellationToken = default);

    Task<ApplyMoveResultDto> ApplyMoveAsync(
        string sessionId,
        string userId,
        string arrowId,
        CancellationToken cancellationToken = default);

    Task<SessionDto?> GetSessionAsync(
        string sessionId,
        string userId,
        CancellationToken cancellationToken = default);
}
