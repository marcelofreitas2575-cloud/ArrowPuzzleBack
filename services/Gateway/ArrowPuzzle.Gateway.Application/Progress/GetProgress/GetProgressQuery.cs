using ArrowPuzzle.Gateway.Application.Models;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Progress.GetProgress;

public sealed record GetProgressQuery(string UserId) : IRequest<PlayerProgressDto>;
