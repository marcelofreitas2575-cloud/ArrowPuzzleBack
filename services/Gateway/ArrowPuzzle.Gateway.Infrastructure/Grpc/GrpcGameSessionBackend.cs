using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
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

    public async Task<SessionDto> CreateSessionAsync(BoardDto initialBoard, CancellationToken cancellationToken = default)
    {
        var response = await _client.CreateSessionAsync(
            new CreateSessionRequest { InitialBoard = ProtoDtoMapper.ToProto(initialBoard) },
            cancellationToken: cancellationToken);

        return new SessionDto(response.SessionId, ProtoDtoMapper.ToDto(response.Board));
    }

    public async Task<ApplyMoveResultDto> ApplyMoveAsync(
        string sessionId,
        string arrowId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.ApplyMoveAsync(
                new ApplyMoveRequest { SessionId = sessionId, ArrowId = arrowId },
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
    }

    public async Task<SessionDto?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetSessionAsync(
                new GetSessionRequest { SessionId = sessionId },
                cancellationToken: cancellationToken);

            return new SessionDto(response.SessionId, ProtoDtoMapper.ToDto(response.Board));
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
