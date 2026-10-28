using ArrowPuzzle.Gateway.Application.Models;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Progress.CompleteLevel;

public sealed record CompleteLevelCommand(string UserId, string LevelId, string? SessionId) : IRequest<PlayerProgressDto>;
