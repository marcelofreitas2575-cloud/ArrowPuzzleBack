using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using MediatR;

namespace ArrowPuzzle.Gateway.Application.Levels.GetLevel;

public sealed class GetLevelHandler : IRequestHandler<GetLevelQuery, LevelDto?>
{
    private readonly ILevelCatalogBackend _levelCatalog;

    public GetLevelHandler(ILevelCatalogBackend levelCatalog)
    {
        _levelCatalog = levelCatalog;
    }

    public Task<LevelDto?> Handle(GetLevelQuery request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LevelId);
        return _levelCatalog.GetLevelAsync(request.LevelId, cancellationToken);
    }
}
