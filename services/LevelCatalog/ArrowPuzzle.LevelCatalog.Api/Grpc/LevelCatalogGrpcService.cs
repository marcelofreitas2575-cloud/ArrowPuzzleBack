using ArrowPuzzle.LevelCatalog.Api.Mapping;
using ArrowPuzzle.LevelCatalog.Application.Levels.GetLevel;
using ArrowPuzzle.LevelCatalog.Domain;
using ArrowPuzzle.Protos.LevelCatalog.V1;
using Grpc.Core;
using MediatR;

namespace ArrowPuzzle.LevelCatalog.Api.Grpc;

public sealed class LevelCatalogGrpcService : LevelCatalogService.LevelCatalogServiceBase
{
    private readonly IMediator _mediator;

    public LevelCatalogGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<GetLevelResponse> GetLevel(GetLevelRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.LevelId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "level_id is required."));
        }

        try
        {
            var levelId = LevelId.From(request.LevelId);
            var level = await _mediator.Send(new GetLevelQuery(levelId), context.CancellationToken);

            if (level is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Level '{request.LevelId}' was not found."));
            }

            return new GetLevelResponse
            {
                LevelId = level.Id.Value,
                Board = LevelProtoMapper.ToProto(level),
            };
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }
}
