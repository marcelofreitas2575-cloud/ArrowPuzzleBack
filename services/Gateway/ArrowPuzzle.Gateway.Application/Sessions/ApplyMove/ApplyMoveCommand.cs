using ArrowPuzzle.Gateway.Application.Models;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Sessions.ApplyMove;

public sealed record ApplyMoveCommand(string SessionId, string ArrowId) : IRequest<ApplyMoveResultDto>;
