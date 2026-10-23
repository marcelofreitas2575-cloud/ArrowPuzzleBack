using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using ArrowPuzzle.Gateway.Infrastructure.Options;
using ArrowPuzzle.Protos.PlayerProgress.V1;
using Dapr.Client;

namespace ArrowPuzzle.Gateway.Infrastructure.Dapr;

public sealed class DaprPlayerProgressBackend : IPlayerProgressBackend
{
    private readonly DaprClient _daprClient;

    public DaprPlayerProgressBackend(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<PlayerProgressDto> GetProgressAsync(string userId, CancellationToken cancellationToken = default)
    {
        var request = new GetProgressRequest { UserId = userId };
        var response = await DaprProtoHttpInvoke.PostAsync<GetProgressRequest, GetProgressResponse>(
            _daprClient,
            DaprAppIds.PlayerProgress,
            DaprInvokeMethods.PlayerProgressGet,
            request,
            cancellationToken);

        return new PlayerProgressDto(response.UserId, response.CompletedLevelIds.ToList());
    }

    public async Task<PlayerProgressDto> CompleteLevelAsync(
        string userId,
        string levelId,
        CancellationToken cancellationToken = default)
    {
        var request = new CompleteLevelRequest { UserId = userId, LevelId = levelId };
        var response = await DaprProtoHttpInvoke.PostAsync<CompleteLevelRequest, CompleteLevelResponse>(
            _daprClient,
            DaprAppIds.PlayerProgress,
            DaprInvokeMethods.PlayerProgressComplete,
            request,
            cancellationToken);

        return new PlayerProgressDto(response.UserId, response.CompletedLevelIds.ToList());
    }
}
