using ArrowPuzzle.PlayerProgress.Application.Progress.CompleteLevel;
using ArrowPuzzle.PlayerProgress.Application.Progress.GetProgress;
using ArrowPuzzle.Protos.PlayerProgress.V1;
using Grpc.Core;
using MediatR;

namespace ArrowPuzzle.PlayerProgress.Api.Grpc;

public sealed class PlayerProgressGrpcService : PlayerProgressService.PlayerProgressServiceBase
{
    private readonly IMediator _mediator;

    public PlayerProgressGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<GetProgressResponse> GetProgress(
        GetProgressRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "user_id is required."));
        }

        try
        {
            var state = await _mediator.Send(new GetProgressQuery(request.UserId), context.CancellationToken);

            var response = new GetProgressResponse
            {
                UserId = state.UserId,
            };
            response.CompletedLevelIds.AddRange(state.CompletedLevelIds);
            return response;
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<CompleteLevelResponse> CompleteLevel(
        CompleteLevelRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "user_id is required."));
        }

        if (string.IsNullOrWhiteSpace(request.LevelId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "level_id is required."));
        }

        try
        {
            var state = await _mediator.Send(
                new CompleteLevelCommand(request.UserId, request.LevelId),
                context.CancellationToken);

            var response = new CompleteLevelResponse { UserId = state.UserId };
            response.CompletedLevelIds.AddRange(state.CompletedLevelIds);
            return response;
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }
}
