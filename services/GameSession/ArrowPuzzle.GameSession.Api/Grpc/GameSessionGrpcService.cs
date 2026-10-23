using ArrowPuzzle.GameSession.Api.Mapping;
using ArrowPuzzle.GameSession.Application.Sessions.ApplyMove;
using ArrowPuzzle.GameSession.Application.Sessions.CreateSession;
using ArrowPuzzle.GameSession.Application.Sessions.GetSession;
using ArrowPuzzle.GameSession.Domain;
using ArrowPuzzle.Protos.GameSession.V1;
using Grpc.Core;
using MediatR;

namespace ArrowPuzzle.GameSession.Api.Grpc;

public sealed class GameSessionGrpcService : GameSessionService.GameSessionServiceBase
{
    private readonly IMediator _mediator;

    public GameSessionGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<CreateSessionResponse> CreateSession(
        CreateSessionRequest request,
        ServerCallContext context)
    {
        if (request.InitialBoard is null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "initial_board is required."));
        }

        var board = BoardProtoMapper.ToDomain(request.InitialBoard);
        var result = await _mediator.Send(new CreateSessionCommand(board), context.CancellationToken);

        return new CreateSessionResponse
        {
            SessionId = result.SessionId.ToString(),
            Board = BoardProtoMapper.ToProto(result.Board),
        };
    }

    public override async Task<ApplyMoveResponse> ApplyMove(ApplyMoveRequest request, ServerCallContext context)
    {
        try
        {
            var sessionId = SessionId.From(request.SessionId);
            var arrowId = ArrowId.From(request.ArrowId);
            var result = await _mediator.Send(
                new ApplyMoveCommand(sessionId, arrowId),
                context.CancellationToken);

            return new ApplyMoveResponse
            {
                Outcome = BoardProtoMapper.ToProto(result.Outcome),
                Board = BoardProtoMapper.ToProto(result.Board),
                IsSuccess = result.IsSuccess,
                IsPuzzleComplete = result.IsPuzzleComplete,
            };
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<GetSessionResponse> GetSession(GetSessionRequest request, ServerCallContext context)
    {
        try
        {
            var sessionId = SessionId.From(request.SessionId);
            var session = await _mediator.Send(new GetSessionQuery(sessionId), context.CancellationToken);

            if (session is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Session '{request.SessionId}' was not found."));
            }

            return new GetSessionResponse
            {
                SessionId = session.Id.ToString(),
                Board = BoardProtoMapper.ToProto(session.Board),
            };
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }
}
