using ArrowPuzzle.LevelCatalog.Domain;
using MediatR;

namespace ArrowPuzzle.LevelCatalog.Application.Levels.GetLevel;

public sealed record GetLevelQuery(LevelId LevelId) : IRequest<LevelDefinition?>;
