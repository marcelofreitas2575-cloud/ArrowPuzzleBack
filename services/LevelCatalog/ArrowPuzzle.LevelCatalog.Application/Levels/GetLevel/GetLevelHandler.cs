using ArrowPuzzle.LevelCatalog.Application.Ports;
using ArrowPuzzle.LevelCatalog.Domain;
using MediatR;

namespace ArrowPuzzle.LevelCatalog.Application.Levels.GetLevel;

public sealed class GetLevelHandler : IRequestHandler<GetLevelQuery, LevelDefinition?>
{
    private readonly ILevelCatalogRepository _repository;

    public GetLevelHandler(ILevelCatalogRepository repository)
    {
        _repository = repository;
    }

    public Task<LevelDefinition?> Handle(GetLevelQuery request, CancellationToken cancellationToken) =>
        _repository.GetByIdAsync(request.LevelId, cancellationToken);
}
