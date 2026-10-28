using ArrowPuzzle.GameSession.Domain;
using MediatR;

namespace ArrowPuzzle.GameSession.Application.Sessions.GetSession;

public sealed record GetSessionQuery(SessionId SessionId, string UserId) : IRequest<PlaySession?>;
