using ArrowPuzzle.GameSession.Application.Ports;
using ArrowPuzzle.GameSession.Domain;
using MediatR;

namespace ArrowPuzzle.GameSession.Application.Sessions.ApplyMove;

public sealed class ApplyMoveHandler : IRequestHandler<ApplyMoveCommand, ApplyMoveResult>
{
    private readonly IGameSessionRepository _repository;

    public ApplyMoveHandler(IGameSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplyMoveResult> Handle(ApplyMoveCommand request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);

        var session = await _repository.GetAsync(request.SessionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Session '{request.SessionId}' was not found.");

        EnsureSessionOwner(session, request.UserId);

        var boardBefore = session.Board;
        var slideResult = boardBefore.Slide(request.ArrowId);
        var boardAfter = ShouldKeepBoardUnchanged(slideResult.Outcome)
            ? boardBefore
            : slideResult.Board;

        var puzzleComplete = session.IsPuzzleComplete || PuzzleRules.IsPuzzleComplete(boardAfter);
        var updated = session.WithBoard(boardAfter, puzzleComplete);
        await _repository.SaveAsync(updated, cancellationToken);

        return new ApplyMoveResult(
            slideResult.Outcome,
            boardAfter,
            slideResult.IsSuccess,
            puzzleComplete);
    }

    /// <summary>
    /// Colisão com outra seta: tabuleiro persistido permanece como antes do clique (cliente repõe visual).
    /// </summary>
    private static bool ShouldKeepBoardUnchanged(SlideOutcome outcome) =>
        outcome is SlideOutcome.BlockedAtStart or SlideOutcome.MovedAndStopped;

    private static void EnsureSessionOwner(PlaySession session, string userId)
    {
        if (string.IsNullOrWhiteSpace(session.UserId)
            || !string.Equals(session.UserId, userId.Trim(), StringComparison.Ordinal))
        {
            throw new SessionAccessDeniedException(
                $"Session '{session.Id}' does not belong to the authenticated user.");
        }
    }
}
