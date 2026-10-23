using ArrowPuzzle.PlayerProgress.Domain;

namespace ArrowPuzzle.PlayerProgress.Application.Ports;

public interface IPlayerProgressRepository
{
    Task<PlayerProgressState?> GetAsync(string userId, CancellationToken cancellationToken = default);

    Task SaveAsync(PlayerProgressState state, CancellationToken cancellationToken = default);
}
