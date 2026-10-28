using ArrowPuzzle.GameSession.Domain;
using MediatR;

namespace ArrowPuzzle.GameSession.Application.Sessions.ApplyMove;

public sealed record ApplyMoveCommand(SessionId SessionId, string UserId, ArrowId ArrowId) : IRequest<ApplyMoveResult>;

public sealed record ApplyMoveResult(
    SlideOutcome Outcome,
    Board Board,
    bool IsSuccess,
    bool IsPuzzleComplete);
