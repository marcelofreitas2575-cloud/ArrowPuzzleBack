using ArrowPuzzle.LevelCatalog.Application.Ports;
using ArrowPuzzle.LevelCatalog.Domain;

namespace ArrowPuzzle.LevelCatalog.Infrastructure.Levels;

/// <summary>
/// Catálogo 100% procedural (IDs numéricos). Solvabilidade garantida em <see cref="ProceduralLevelGenerator"/>.
/// </summary>
public sealed class ProceduralLevelCatalogRepository : ILevelCatalogRepository
{
    private readonly ProceduralLevelGenerator _generator;

    public ProceduralLevelCatalogRepository(ProceduralLevelGenerator generator)
    {
        _generator = generator;
    }

    public Task<LevelDefinition?> GetByIdAsync(LevelId levelId, CancellationToken cancellationToken = default)
    {
        if (LevelIndex.TryParse(levelId.Value, out var index) && index >= 1)
        {
            var level = _generator.TryCreate(levelId);
            if (level is not null)
            {
                ProceduralLevelSolvabilityGuard.EnsureSolvable(level);
            }

            return Task.FromResult<LevelDefinition?>(level);
        }

        return Task.FromResult<LevelDefinition?>(null);
    }
}
