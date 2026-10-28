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

    public async Task<PlaySession?> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);

        var session = await _repository.GetAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(session.UserId)
            || !string.Equals(session.UserId, request.UserId.Trim(), StringComparison.Ordinal))
        {
            throw new SessionAccessDeniedException(
                $"Session '{request.SessionId}' does not belong to the authenticated user.");
        }

        var puzzleComplete = session.IsPuzzleComplete || PuzzleRules.IsPuzzleComplete(session.Board);
        if (puzzleComplete == session.IsPuzzleComplete)
        {
            return session;
        }

        var reconciled = session.WithBoard(session.Board, puzzleComplete);
        await _repository.SaveAsync(reconciled, cancellationToken);
        return reconciled;
    }
}
