using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using ArrowPuzzle.BuildingBlocks.Persistence;
using ArrowPuzzle.PlayerProgress.Application.Ports;
using ArrowPuzzle.PlayerProgress.Domain;

namespace ArrowPuzzle.PlayerProgress.Infrastructure.Persistence;

public sealed class DynamoPlayerProgressRepository : IPlayerProgressRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IAmazonDynamoDB _client;

    public DynamoPlayerProgressRepository(IAmazonDynamoDB client)
    {
        _client = client;
    }

    public async Task<PlayerProgressState?> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var response = await _client.GetItemAsync(
            new GetItemRequest
            {
                TableName = ArrowPuzzleDynamoTables.PlayerProgress,
                Key = new Dictionary<string, AttributeValue>
                {
                    [ArrowPuzzleDynamoTables.PartitionKeyName] = new AttributeValue { S = userId },
                },
            },
            cancellationToken);

        if (response.Item is null || response.Item.Count == 0)
        {
            return null;
        }

        var payload = response.Item["Payload"].S;
        var document = JsonSerializer.Deserialize<PlayerProgressDocument>(payload, JsonOptions);
        if (document is null)
        {
            return null;
        }

        return new PlayerProgressState(document.UserId, document.CompletedLevelIds);
    }

    public async Task SaveAsync(PlayerProgressState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        var document = new PlayerProgressDocument
        {
            UserId = state.UserId,
            CompletedLevelIds = state.CompletedLevelIds.ToList(),
        };

        var payload = JsonSerializer.Serialize(document, JsonOptions);

        await _client.PutItemAsync(
            new PutItemRequest
            {
                TableName = ArrowPuzzleDynamoTables.PlayerProgress,
                Item = new Dictionary<string, AttributeValue>
                {
                    [ArrowPuzzleDynamoTables.PartitionKeyName] = new AttributeValue { S = state.UserId },
                    ["Payload"] = new AttributeValue { S = payload },
                },
            },
            cancellationToken);
    }
}
