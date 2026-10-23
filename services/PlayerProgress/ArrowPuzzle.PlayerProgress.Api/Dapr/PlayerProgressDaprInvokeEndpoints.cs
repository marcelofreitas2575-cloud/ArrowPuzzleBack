using ArrowPuzzle.BuildingBlocks.Dapr;
using ArrowPuzzle.PlayerProgress.Application.Progress.CompleteLevel;
using ArrowPuzzle.PlayerProgress.Application.Progress.GetProgress;
using ArrowPuzzle.Protos.PlayerProgress.V1;
using MediatR;

namespace ArrowPuzzle.PlayerProgress.Api.Dapr;

public static class PlayerProgressDaprInvokeEndpoints
{
    public static WebApplication MapPlayerProgressDaprInvoke(this WebApplication app)
    {
        app.MapProtoPost<GetProgressRequest, GetProgressResponse>(
            "/dapr/get-progress",
            async (request, cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    throw new ArgumentException("user_id is required.");
                }

                var mediator = app.Services.GetRequiredService<IMediator>();
                var state = await mediator.Send(new GetProgressQuery(request.UserId), cancellationToken);

                var response = new GetProgressResponse { UserId = state.UserId };
                response.CompletedLevelIds.AddRange(state.CompletedLevelIds);
                return response;
            });

        app.MapProtoPost<CompleteLevelRequest, CompleteLevelResponse>(
            "/dapr/complete-level",
            async (request, cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    throw new ArgumentException("user_id is required.");
                }

                if (string.IsNullOrWhiteSpace(request.LevelId))
                {
                    throw new ArgumentException("level_id is required.");
                }

                var mediator = app.Services.GetRequiredService<IMediator>();
                var state = await mediator.Send(
                    new CompleteLevelCommand(request.UserId, request.LevelId),
                    cancellationToken);

                var response = new CompleteLevelResponse { UserId = state.UserId };
                response.CompletedLevelIds.AddRange(state.CompletedLevelIds);
                return response;
            });

        return app;
    }
}
