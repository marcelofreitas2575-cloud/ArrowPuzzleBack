using ArrowPuzzle.GameSession.Application.Ports;
using ArrowPuzzle.GameSession.Domain;
using MediatR;

namespace ArrowPuzzle.GameSession.Application.Sessions.GetSession;

public sealed class GetSessionHandler : IRequestHandler<GetSessionQuery, PlaySession?>
{
    private readonly IGameSessionRepository _repository;

    public GetSessionHandler(IGameSessionRepository repository)
    {
        _repository = repository;
    }

    public Task<PlaySession?> Handle(GetSessionQuery request, CancellationToken cancellationToken) =>
        _repository.GetAsync(request.SessionId, cancellationToken);
}
