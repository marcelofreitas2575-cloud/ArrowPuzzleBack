using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using ArrowPuzzle.Gateway.Infrastructure.Grpc;
using ArrowPuzzle.Gateway.Infrastructure.Options;
using ArrowPuzzle.Protos.LevelCatalog.V1;
using Dapr.Client;

namespace ArrowPuzzle.Gateway.Infrastructure.Dapr;

public sealed class DaprLevelCatalogBackend : ILevelCatalogBackend
{
    private readonly DaprClient _daprClient;

    public DaprLevelCatalogBackend(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<LevelDto?> GetLevelAsync(string levelId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetLevelRequest { LevelId = levelId };
            var response = await DaprProtoHttpInvoke.PostAsync<GetLevelRequest, GetLevelResponse>(
                _daprClient,
                DaprAppIds.LevelCatalog,
                DaprInvokeMethods.LevelCatalogGetLevel,
                request,
                cancellationToken);

            if (response.Board is null)
            {
                return null;
            }

            return new LevelDto(response.LevelId, ProtoDtoMapper.ToDto(response.Board));
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}
