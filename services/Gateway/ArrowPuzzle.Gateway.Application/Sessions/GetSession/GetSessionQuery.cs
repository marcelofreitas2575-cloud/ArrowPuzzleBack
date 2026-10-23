using ArrowPuzzle.Gateway.Application.Models;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Sessions.GetSession;

public sealed record GetSessionQuery(string SessionId) : IRequest<SessionDto?>;
