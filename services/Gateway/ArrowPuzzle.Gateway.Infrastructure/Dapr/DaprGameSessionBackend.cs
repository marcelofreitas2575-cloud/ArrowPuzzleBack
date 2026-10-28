using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using ArrowPuzzle.Gateway.Infrastructure.Grpc;
using ArrowPuzzle.Gateway.Infrastructure.Options;
using ArrowPuzzle.Protos.GameSession.V1;
using Dapr.Client;

namespace ArrowPuzzle.Gateway.Infrastructure.Dapr;

public sealed class DaprGameSessionBackend : IGameSessionBackend
{
    private readonly DaprClient _daprClient;

    public DaprGameSessionBackend(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<SessionDto> CreateSessionAsync(
        string userId,
        string levelId,
        BoardDto initialBoard,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateSessionRequest
        {
            InitialBoard = ProtoDtoMapper.ToProto(initialBoard),
            UserId = userId,
            LevelId = levelId,
        };
        var response = await DaprProtoHttpInvoke.PostAsync<CreateSessionRequest, CreateSessionResponse>(
            _daprClient,
            DaprAppIds.GameSession,
            DaprInvokeMethods.GameSessionCreate,
            request,
            cancellationToken);

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
        var request = new ApplyMoveRequest { SessionId = sessionId, UserId = userId, ArrowId = arrowId };
        var response = await DaprProtoHttpInvoke.PostAsync<ApplyMoveRequest, ApplyMoveResponse>(
            _daprClient,
            DaprAppIds.GameSession,
            DaprInvokeMethods.GameSessionApplyMove,
            request,
            cancellationToken);

        return new ApplyMoveResultDto(
            ProtoDtoMapper.ToOutcomeName(response.Outcome),
            ProtoDtoMapper.ToDto(response.Board),
            response.IsSuccess,
            response.IsPuzzleComplete);
    }

    public async Task<SessionDto?> GetSessionAsync(
        string sessionId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetSessionRequest { SessionId = sessionId, UserId = userId };
            var response = await DaprProtoHttpInvoke.PostAsync<GetSessionRequest, GetSessionResponse>(
                _daprClient,
                DaprAppIds.GameSession,
                DaprInvokeMethods.GameSessionGet,
                request,
                cancellationToken);

            return new SessionDto(
                response.SessionId,
                ProtoDtoMapper.ToDto(response.Board),
                response.LevelId,
                response.IsPuzzleComplete);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}
