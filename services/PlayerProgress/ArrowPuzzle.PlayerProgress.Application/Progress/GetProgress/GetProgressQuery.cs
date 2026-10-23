using ArrowPuzzle.PlayerProgress.Domain;
using MediatR;

namespace ArrowPuzzle.PlayerProgress.Application.Progress.GetProgress;

public sealed record GetProgressQuery(string UserId) : IRequest<PlayerProgressState>;
