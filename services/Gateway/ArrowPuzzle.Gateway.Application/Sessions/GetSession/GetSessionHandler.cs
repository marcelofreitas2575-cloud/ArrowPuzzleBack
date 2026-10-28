using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Sessions.GetSession;

public sealed class GetSessionHandler : IRequestHandler<GetSessionQuery, SessionDto?>
{
    private readonly IGameSessionBackend _gameSession;

    public GetSessionHandler(IGameSessionBackend gameSession)
    {
        _gameSession = gameSession;
    }

    public Task<SessionDto?> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);
        return _gameSession.GetSessionAsync(request.SessionId, request.UserId, cancellationToken);
    }
}
