using ArrowPuzzle.Gateway.Application.Models;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Sessions.StartLevel;

public sealed record StartLevelCommand(string LevelId) : IRequest<SessionDto>;
