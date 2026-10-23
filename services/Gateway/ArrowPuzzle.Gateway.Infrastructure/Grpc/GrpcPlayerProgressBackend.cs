using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using ArrowPuzzle.Protos.PlayerProgress.V1;

namespace ArrowPuzzle.Gateway.Infrastructure.Grpc;

public sealed class GrpcPlayerProgressBackend : IPlayerProgressBackend
{
    private readonly PlayerProgressService.PlayerProgressServiceClient _client;

    public GrpcPlayerProgressBackend(PlayerProgressService.PlayerProgressServiceClient client)
    {
        _client = client;
    }

    public async Task<PlayerProgressDto> GetProgressAsync(string userId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetProgressAsync(
            new GetProgressRequest { UserId = userId },
            cancellationToken: cancellationToken);

        return new PlayerProgressDto(response.UserId, response.CompletedLevelIds.ToList());
    }

    public async Task<PlayerProgressDto> CompleteLevelAsync(
        string userId,
        string levelId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.CompleteLevelAsync(
            new CompleteLevelRequest { UserId = userId, LevelId = levelId },
            cancellationToken: cancellationToken);

        return new PlayerProgressDto(response.UserId, response.CompletedLevelIds.ToList());
    }
}
