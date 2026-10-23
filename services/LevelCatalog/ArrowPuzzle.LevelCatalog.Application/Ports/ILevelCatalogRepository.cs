using ArrowPuzzle.LevelCatalog.Domain;

namespace ArrowPuzzle.LevelCatalog.Application.Ports;

public interface ILevelCatalogRepository
{
    Task<LevelDefinition?> GetByIdAsync(LevelId levelId, CancellationToken cancellationToken = default);
}
