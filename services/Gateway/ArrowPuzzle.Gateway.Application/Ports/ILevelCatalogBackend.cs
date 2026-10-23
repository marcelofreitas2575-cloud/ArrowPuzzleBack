using ArrowPuzzle.Gateway.Application.Models;

namespace ArrowPuzzle.Gateway.Application.Ports;

public interface ILevelCatalogBackend
{
    Task<LevelDto?> GetLevelAsync(string levelId, CancellationToken cancellationToken = default);
}
