using ArrowPuzzle.Gateway.Application.Models;

namespace ArrowPuzzle.Gateway.Application.Ports;

public interface IGameSessionBackend
{
    Task<SessionDto> CreateSessionAsync(BoardDto initialBoard, CancellationToken cancellationToken = default);

    Task<ApplyMoveResultDto> ApplyMoveAsync(
        string sessionId,
        string arrowId,
        CancellationToken cancellationToken = default);

    Task<SessionDto?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);
}
