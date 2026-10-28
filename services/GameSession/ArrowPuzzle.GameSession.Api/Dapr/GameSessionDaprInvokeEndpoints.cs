using ArrowPuzzle.BuildingBlocks.Dapr;
using ArrowPuzzle.GameSession.Api.Mapping;
using ArrowPuzzle.GameSession.Application.Sessions.ApplyMove;
using ArrowPuzzle.GameSession.Application.Sessions.CreateSession;
using ArrowPuzzle.GameSession.Application.Sessions.GetSession;
using ArrowPuzzle.GameSession.Domain;
using ArrowPuzzle.Protos.GameSession.V1;
using MediatR;

namespace ArrowPuzzle.GameSession.Api.Dapr;

public static class GameSessionDaprInvokeEndpoints
{
    public static WebApplication MapGameSessionDaprInvoke(this WebApplication app)
    {
        app.MapProtoPost<CreateSessionRequest, CreateSessionResponse>(
            "/dapr/create-session",
            async (request, cancellationToken) =>
            {
                if (request.InitialBoard is null)
                {
                    throw new ArgumentException("initial_board is required.");
                }

                if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.LevelId))
                {
                    throw new ArgumentException("user_id and level_id are required.");
                }

                var board = BoardProtoMapper.ToDomain(request.InitialBoard);
                var mediator = app.Services.GetRequiredService<IMediator>();
                var result = await mediator.Send(
                    new CreateSessionCommand(request.UserId, request.LevelId, board),
                    cancellationToken);

                return new CreateSessionResponse
                {
                    SessionId = result.SessionId.ToString(),
                    Board = BoardProtoMapper.ToProto(result.Board),
                };
            });

        app.MapProtoPost<ApplyMoveRequest, ApplyMoveResponse>(
            "/dapr/apply-move",
            async (request, cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    throw new ArgumentException("user_id is required.");
                }

                var sessionId = SessionId.From(request.SessionId);
                var arrowId = ArrowId.From(request.ArrowId);
                var mediator = app.Services.GetRequiredService<IMediator>();
                try
                {
                    var result = await mediator.Send(
                        new ApplyMoveCommand(sessionId, request.UserId, arrowId),
                        cancellationToken);

                    return new ApplyMoveResponse
                    {
                        Outcome = BoardProtoMapper.ToProto(result.Outcome),
                        Board = BoardProtoMapper.ToProto(result.Board),
                        IsSuccess = result.IsSuccess,
                        IsPuzzleComplete = result.IsPuzzleComplete,
                    };
                }
                catch (KeyNotFoundException)
                {
                    throw;
                }
                catch (Exception ex) when (ex is ArgumentException or FormatException)
                {
                    throw new ArgumentException(ex.Message, ex);
                }
            });

        app.MapProtoPost<GetSessionRequest, GetSessionResponse>(
            "/dapr/get-session",
            async (request, cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    throw new ArgumentException("user_id is required.");
                }

                var sessionId = SessionId.From(request.SessionId);
                var mediator = app.Services.GetRequiredService<IMediator>();
                var session = await mediator.Send(
                    new GetSessionQuery(sessionId, request.UserId),
                    cancellationToken);

                if (session is null)
                {
                    throw new KeyNotFoundException($"Session '{request.SessionId}' was not found.");
                }

                return new GetSessionResponse
                {
                    SessionId = session.Id.ToString(),
                    Board = BoardProtoMapper.ToProto(session.Board),
                    LevelId = session.LevelId,
                    IsPuzzleComplete = session.IsPuzzleComplete,
                };
            });

        return app;
    }
}
