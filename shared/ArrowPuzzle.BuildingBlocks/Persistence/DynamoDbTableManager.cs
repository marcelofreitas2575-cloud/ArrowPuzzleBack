using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace ArrowPuzzle.BuildingBlocks.Persistence;

public sealed class DynamoDbTableManager
{
    private readonly IAmazonDynamoDB _client;

    public DynamoDbTableManager(IAmazonDynamoDB client)
    {
        _client = client;
    }

    public async Task EnsureTableExistsAsync(string tableName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.DescribeTableAsync(new DescribeTableRequest { TableName = tableName }, cancellationToken);
        }
        catch (ResourceNotFoundException)
        {
            try
            {
                await _client.CreateTableAsync(
                    new CreateTableRequest
                    {
                        TableName = tableName,
                        AttributeDefinitions =
                        [
                            new AttributeDefinition(ArrowPuzzleDynamoTables.PartitionKeyName, ScalarAttributeType.S),
                        ],
                        KeySchema =
                        [
                            new KeySchemaElement(ArrowPuzzleDynamoTables.PartitionKeyName, KeyType.HASH),
                        ],
                        BillingMode = BillingMode.PAY_PER_REQUEST,
                    },
                    cancellationToken);
            }
            catch (ResourceInUseException)
            {
                // Outro serviço criou a tabela em paralelo (Compose sobe 3 APIs ao mesmo tempo).
            }

            await WaitUntilActiveAsync(tableName, cancellationToken);
        }
    }

    private async Task WaitUntilActiveAsync(string tableName, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 30; attempt++)
        {
            var response = await _client.DescribeTableAsync(
                new DescribeTableRequest { TableName = tableName },
                cancellationToken);

            if (string.Equals(response.Table.TableStatus, TableStatus.ACTIVE, StringComparison.Ordinal))
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
        }

        throw new InvalidOperationException($"DynamoDB table '{tableName}' did not become ACTIVE in time.");
    }
}
