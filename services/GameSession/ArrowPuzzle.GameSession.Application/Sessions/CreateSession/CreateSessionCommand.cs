using ArrowPuzzle.GameSession.Domain;
using MediatR;

namespace ArrowPuzzle.GameSession.Application.Sessions.CreateSession;

public sealed record CreateSessionCommand(Board InitialBoard) : IRequest<CreateSessionResult>;

public sealed record CreateSessionResult(SessionId SessionId, Board Board);
