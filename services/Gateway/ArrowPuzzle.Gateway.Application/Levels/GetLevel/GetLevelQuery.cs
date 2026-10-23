using ArrowPuzzle.Gateway.Application.Models;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Levels.GetLevel;

public sealed record GetLevelQuery(string LevelId) : IRequest<LevelDto?>;
