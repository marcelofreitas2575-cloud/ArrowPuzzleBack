using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Sessions.StartLevel;

public sealed class StartLevelHandler : IRequestHandler<StartLevelCommand, SessionDto>
{
    private readonly ILevelCatalogBackend _levelCatalog;
    private readonly IGameSessionBackend _gameSession;

    public StartLevelHandler(ILevelCatalogBackend levelCatalog, IGameSessionBackend gameSession)
    {
        _levelCatalog = levelCatalog;
        _gameSession = gameSession;
    }

    public async Task<SessionDto> Handle(StartLevelCommand request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LevelId);

        var level = await _levelCatalog.GetLevelAsync(request.LevelId, cancellationToken);
        if (level is null)
        {
            throw new KeyNotFoundException($"Level '{request.LevelId}' was not found.");
        }

        return await _gameSession.CreateSessionAsync(level.Board, cancellationToken);
    }
}
