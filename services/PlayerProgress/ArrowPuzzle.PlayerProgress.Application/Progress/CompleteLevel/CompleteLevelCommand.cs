using ArrowPuzzle.PlayerProgress.Domain;
using MediatR;

namespace ArrowPuzzle.PlayerProgress.Application.Progress.CompleteLevel;

public sealed record CompleteLevelCommand(string UserId, string LevelId) : IRequest<PlayerProgressState>;
