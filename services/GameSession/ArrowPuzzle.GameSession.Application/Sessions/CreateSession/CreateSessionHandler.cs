using ArrowPuzzle.GameSession.Application.Ports;
using ArrowPuzzle.GameSession.Domain;
using MediatR;

namespace ArrowPuzzle.GameSession.Application.Sessions.CreateSession;

public sealed class CreateSessionHandler : IRequestHandler<CreateSessionCommand, CreateSessionResult>
{
    private readonly IGameSessionRepository _repository;

    public CreateSessionHandler(IGameSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateSessionResult> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LevelId);
        ArgumentNullException.ThrowIfNull(request.InitialBoard);

        var sessionId = SessionId.New();
        var session = new PlaySession(sessionId, request.UserId.Trim(), request.LevelId.Trim(), request.InitialBoard);

        await _repository.AddAsync(session, cancellationToken);

        return new CreateSessionResult(sessionId, session.Board);
    }
}
