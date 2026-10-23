using ArrowPuzzle.BuildingBlocks.Dapr;
using ArrowPuzzle.LevelCatalog.Api.Mapping;
using ArrowPuzzle.LevelCatalog.Application.Levels.GetLevel;
using ArrowPuzzle.LevelCatalog.Domain;
using ArrowPuzzle.Protos.LevelCatalog.V1;
using MediatR;

namespace ArrowPuzzle.LevelCatalog.Api.Dapr;

public static class LevelCatalogDaprInvokeEndpoints
{
    public static WebApplication MapLevelCatalogDaprInvoke(this WebApplication app)
    {
        app.MapProtoPost<GetLevelRequest, GetLevelResponse>(
            "/dapr/get-level",
            async (request, cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.LevelId))
                {
                    throw new ArgumentException("level_id is required.");
                }

                var levelId = LevelId.From(request.LevelId);
                var mediator = app.Services.GetRequiredService<IMediator>();
                var level = await mediator.Send(new GetLevelQuery(levelId), cancellationToken);

                if (level is null)
                {
                    throw new KeyNotFoundException($"Level '{request.LevelId}' was not found.");
                }

                return new GetLevelResponse
                {
                    LevelId = level.Id.Value,
                    Board = LevelProtoMapper.ToProto(level),
                };
            });

        return app;
    }
}
