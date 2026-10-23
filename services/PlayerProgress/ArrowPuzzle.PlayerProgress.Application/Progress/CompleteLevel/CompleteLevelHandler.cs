using ArrowPuzzle.PlayerProgress.Application.Ports;
using ArrowPuzzle.PlayerProgress.Domain;
using MediatR;

namespace ArrowPuzzle.PlayerProgress.Application.Progress.CompleteLevel;

public sealed class CompleteLevelHandler : IRequestHandler<CompleteLevelCommand, PlayerProgressState>
{
    private readonly IPlayerProgressRepository _repository;

    public CompleteLevelHandler(IPlayerProgressRepository repository)
    {
        _repository = repository;
    }

    public async Task<PlayerProgressState> Handle(CompleteLevelCommand request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LevelId);

        var current = await _repository.GetAsync(request.UserId, cancellationToken);
        var state = current ?? new PlayerProgressState(request.UserId, Array.Empty<string>());
        var updated = state.WithLevelCompleted(request.LevelId);

        if (!ReferenceEquals(updated, state))
        {
            await _repository.SaveAsync(updated, cancellationToken);
        }

        return updated;
    }
}
