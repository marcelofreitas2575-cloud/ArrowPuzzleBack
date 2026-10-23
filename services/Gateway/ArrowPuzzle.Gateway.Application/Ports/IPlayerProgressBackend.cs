using ArrowPuzzle.Gateway.Application.Models;

namespace ArrowPuzzle.Gateway.Application.Ports;

public interface IPlayerProgressBackend
{
    Task<PlayerProgressDto> GetProgressAsync(string userId, CancellationToken cancellationToken = default);

    Task<PlayerProgressDto> CompleteLevelAsync(
        string userId,
        string levelId,
        CancellationToken cancellationToken = default);
}
