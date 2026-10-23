using ArrowPuzzle.PlayerProgress.Application.Ports;
using ArrowPuzzle.PlayerProgress.Domain;
using MediatR;

namespace ArrowPuzzle.PlayerProgress.Application.Progress.GetProgress;

public sealed class GetProgressHandler : IRequestHandler<GetProgressQuery, PlayerProgressState>
{
    private readonly IPlayerProgressRepository _repository;

    public GetProgressHandler(IPlayerProgressRepository repository)
    {
        _repository = repository;
    }

    public async Task<PlayerProgressState> Handle(GetProgressQuery request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);

        var state = await _repository.GetAsync(request.UserId, cancellationToken);
        return state ?? new PlayerProgressState(request.UserId, Array.Empty<string>());
    }
}
