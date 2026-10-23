using Microsoft.Extensions.Hosting;

namespace ArrowPuzzle.BuildingBlocks.Persistence;

public sealed class DynamoDbTablesHostedService : IHostedService
{
    private readonly DynamoDbTableManager _tableManager;

    public DynamoDbTablesHostedService(DynamoDbTableManager tableManager)
    {
        _tableManager = tableManager;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _tableManager.EnsureTableExistsAsync(ArrowPuzzleDynamoTables.GameSessions, cancellationToken);
        await _tableManager.EnsureTableExistsAsync(ArrowPuzzleDynamoTables.PlayerProgress, cancellationToken);
        await _tableManager.EnsureTableExistsAsync(ArrowPuzzleDynamoTables.LevelCatalog, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
