using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using ArrowPuzzle.BuildingBlocks.Persistence;
using ArrowPuzzle.GameSession.Application.Ports;
using ArrowPuzzle.GameSession.Domain;

namespace ArrowPuzzle.GameSession.Infrastructure.Persistence;

public sealed class DynamoGameSessionRepository : IGameSessionRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IAmazonDynamoDB _client;

    public DynamoGameSessionRepository(IAmazonDynamoDB client)
    {
        _client = client;
    }

    public async Task AddAsync(PlaySession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        var document = ToDocument(session);
        var payload = JsonSerializer.Serialize(document, JsonOptions);

        try
        {
            await _client.PutItemAsync(
                new PutItemRequest
                {
                    TableName = ArrowPuzzleDynamoTables.GameSessions,
                    Item = CreateItem(session.Id.ToString(), payload),
                    ConditionExpression = "attribute_not_exists(#pk)",
                    ExpressionAttributeNames = new Dictionary<string, string> { ["#pk"] = ArrowPuzzleDynamoTables.PartitionKeyName },
                },
                cancellationToken);
        }
        catch (ConditionalCheckFailedException)
        {
            throw new InvalidOperationException($"Session '{session.Id}' already exists.");
        }
    }

    public async Task<PlaySession?> GetAsync(SessionId sessionId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetItemAsync(
            new GetItemRequest
            {
                TableName = ArrowPuzzleDynamoTables.GameSessions,
                Key = CreateKey(sessionId.ToString()),
            },
            cancellationToken);

        if (response.Item is null || response.Item.Count == 0)
        {
            return null;
        }

        return FromItem(response.Item);
    }

    public async Task SaveAsync(PlaySession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        var document = ToDocument(session);
        var payload = JsonSerializer.Serialize(document, JsonOptions);

        try
        {
            await _client.PutItemAsync(
                new PutItemRequest
                {
                    TableName = ArrowPuzzleDynamoTables.GameSessions,
                    Item = CreateItem(session.Id.ToString(), payload),
                    ConditionExpression = "attribute_exists(#pk)",
                    ExpressionAttributeNames = new Dictionary<string, string> { ["#pk"] = ArrowPuzzleDynamoTables.PartitionKeyName },
                },
                cancellationToken);
        }
        catch (ConditionalCheckFailedException)
        {
            throw new KeyNotFoundException($"Session '{session.Id}' was not found.");
        }
    }

    private static GameSessionDocument ToDocument(PlaySession session) =>
        new()
        {
            SessionId = session.Id.ToString(),
            Board = BoardStateMapper.ToDocument(session.Board),
        };

    private static PlaySession FromItem(Dictionary<string, AttributeValue> item)
    {
        var payload = item["Payload"].S;
        var document = JsonSerializer.Deserialize<GameSessionDocument>(payload, JsonOptions)
            ?? throw new InvalidOperationException("Stored session payload is invalid.");

        var sessionId = SessionId.From(document.SessionId);
        var board = BoardStateMapper.ToDomain(document.Board);
        return new PlaySession(sessionId, board);
    }

    private static Dictionary<string, AttributeValue> CreateKey(string partitionKey) =>
        new()
        {
            [ArrowPuzzleDynamoTables.PartitionKeyName] = new AttributeValue { S = partitionKey },
        };

    private static Dictionary<string, AttributeValue> CreateItem(string partitionKey, string payload) =>
        new(CreateKey(partitionKey))
        {
            ["Payload"] = new AttributeValue { S = payload },
        };
}
