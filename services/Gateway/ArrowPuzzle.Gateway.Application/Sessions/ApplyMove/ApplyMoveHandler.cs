using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Sessions.ApplyMove;

public sealed class ApplyMoveHandler : IRequestHandler<ApplyMoveCommand, ApplyMoveResultDto>
{
    private readonly IGameSessionBackend _gameSession;

    public ApplyMoveHandler(IGameSessionBackend gameSession)
    {
        _gameSession = gameSession;
    }

    public Task<ApplyMoveResultDto> Handle(ApplyMoveCommand request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ArrowId);

        return _gameSession.ApplyMoveAsync(request.SessionId, request.ArrowId, cancellationToken);
    }
}
