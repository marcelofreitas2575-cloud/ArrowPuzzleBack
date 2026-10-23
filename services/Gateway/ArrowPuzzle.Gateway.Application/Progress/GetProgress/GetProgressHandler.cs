using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Progress.GetProgress;

public sealed class GetProgressHandler : IRequestHandler<GetProgressQuery, PlayerProgressDto>
{
    private readonly IPlayerProgressBackend _playerProgress;

    public GetProgressHandler(IPlayerProgressBackend playerProgress)
    {
        _playerProgress = playerProgress;
    }

    public Task<PlayerProgressDto> Handle(GetProgressQuery request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);
        return _playerProgress.GetProgressAsync(request.UserId, cancellationToken);
    }
}
