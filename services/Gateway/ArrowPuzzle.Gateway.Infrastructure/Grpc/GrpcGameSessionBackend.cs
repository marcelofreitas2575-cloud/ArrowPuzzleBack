using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using ArrowPuzzle.Gateway.Application.Sessions;
using ArrowPuzzle.Protos.GameSession.V1;
using Grpc.Core;

namespace ArrowPuzzle.Gateway.Infrastructure.Grpc;

public sealed class GrpcGameSessionBackend : IGameSessionBackend
{
    private readonly GameSessionService.GameSessionServiceClient _client;

    public GrpcGameSessionBackend(GameSessionService.GameSessionServiceClient client)
    {
        _client = client;
    }

    public async Task<SessionDto> CreateSessionAsync(
        string userId,
        string levelId,
        BoardDto initialBoard,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.CreateSessionAsync(
            new CreateSessionRequest
            {
                InitialBoard = ProtoDtoMapper.ToProto(initialBoard),
                UserId = userId,
                LevelId = levelId,
            },
            cancellationToken: cancellationToken);

        return new SessionDto(
            response.SessionId,
            ProtoDtoMapper.ToDto(response.Board),
            levelId,
            false);
    }

    public async Task<ApplyMoveResultDto> ApplyMoveAsync(
        string sessionId,
        string userId,
        string arrowId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.ApplyMoveAsync(
                new ApplyMoveRequest { SessionId = sessionId, UserId = userId, ArrowId = arrowId },
                cancellationToken: cancellationToken);

            return new ApplyMoveResultDto(
                ProtoDtoMapper.ToOutcomeName(response.Outcome),
                ProtoDtoMapper.ToDto(response.Board),
                response.IsSuccess,
                response.IsPuzzleComplete);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new KeyNotFoundException(ex.Status.Detail, ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
        {
            throw new SessionAccessDeniedException(ex.Status.Detail);
        }
    }

    public async Task<SessionDto?> GetSessionAsync(
        string sessionId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetSessionAsync(
                new GetSessionRequest { SessionId = sessionId, UserId = userId },
                cancellationToken: cancellationToken);

            return new SessionDto(
                response.SessionId,
                ProtoDtoMapper.ToDto(response.Board),
                response.LevelId,
                response.IsPuzzleComplete);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
        {
            throw new SessionAccessDeniedException(ex.Status.Detail);
        }
    }
}
