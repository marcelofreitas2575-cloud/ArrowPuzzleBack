using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Progress.CompleteLevel;

public sealed class CompleteLevelHandler : IRequestHandler<CompleteLevelCommand, PlayerProgressDto>
{
    private readonly IPlayerProgressBackend _playerProgress;

    public CompleteLevelHandler(IPlayerProgressBackend playerProgress)
    {
        _playerProgress = playerProgress;
    }

    public Task<PlayerProgressDto> Handle(CompleteLevelCommand request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LevelId);
        return _playerProgress.CompleteLevelAsync(request.UserId, request.LevelId, cancellationToken);
    }
}
