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
        var session = await _repository.GetAsync(request.SessionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Session '{request.SessionId}' was not found.");

        var boardBefore = session.Board;
        var slideResult = boardBefore.Slide(request.ArrowId);
        var boardAfter = ShouldKeepBoardUnchanged(slideResult.Outcome)
            ? boardBefore
            : slideResult.Board;

        var updated = session.WithBoard(boardAfter);
        await _repository.SaveAsync(updated, cancellationToken);

        return new ApplyMoveResult(
            slideResult.Outcome,
            boardAfter,
            slideResult.IsSuccess,
            PuzzleRules.IsPuzzleComplete(boardAfter));
    }

    /// <summary>
    /// Colisão com outra seta: tabuleiro persistido permanece como antes do clique (cliente repõe visual).
    /// </summary>
    private static bool ShouldKeepBoardUnchanged(SlideOutcome outcome) =>
        outcome is SlideOutcome.BlockedAtStart or SlideOutcome.MovedAndStopped;
}
