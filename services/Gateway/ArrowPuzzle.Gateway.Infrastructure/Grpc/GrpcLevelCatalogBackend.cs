using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using ArrowPuzzle.Protos.LevelCatalog.V1;
using Grpc.Core;

namespace ArrowPuzzle.Gateway.Infrastructure.Grpc;

public sealed class GrpcLevelCatalogBackend : ILevelCatalogBackend
{
    private readonly LevelCatalogService.LevelCatalogServiceClient _client;

    public GrpcLevelCatalogBackend(LevelCatalogService.LevelCatalogServiceClient client)
    {
        _client = client;
    }

    public async Task<LevelDto?> GetLevelAsync(string levelId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetLevelAsync(
                new GetLevelRequest { LevelId = levelId },
                cancellationToken: cancellationToken);

            if (response.Board is null)
            {
                return null;
            }

            return new LevelDto(response.LevelId, ProtoDtoMapper.ToDto(response.Board));
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
