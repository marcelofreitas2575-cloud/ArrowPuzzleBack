using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Progress.CompleteLevel;

public sealed class CompleteLevelHandler : IRequestHandler<CompleteLevelCommand, PlayerProgressDto>
{
    private readonly IGameSessionBackend _gameSession;
    private readonly IPlayerProgressBackend _playerProgress;

    public CompleteLevelHandler(IGameSessionBackend gameSession, IPlayerProgressBackend playerProgress)
    {
        _gameSession = gameSession;
        _playerProgress = playerProgress;
    }

    public async Task<PlayerProgressDto> Handle(CompleteLevelCommand request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LevelId);

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return await _playerProgress.CompleteLevelAsync(request.UserId, request.LevelId, cancellationToken);
        }

        var session = await _gameSession.GetSessionAsync(
            request.SessionId,
            request.UserId,
            cancellationToken);

        if (session is null)
        {
            throw new KeyNotFoundException($"Session '{request.SessionId}' was not found.");
        }

        if (!session.IsPuzzleComplete)
        {
            throw new LevelCompletionNotVerifiedException(
                "Complete the puzzle in this session before saving progress.");
        }

        if (!string.Equals(session.LevelId, request.LevelId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new LevelCompletionNotVerifiedException(
                "The session level does not match the level you are trying to complete.");
        }

        return await _playerProgress.CompleteLevelAsync(request.UserId, request.LevelId, cancellationToken);
    }
}
