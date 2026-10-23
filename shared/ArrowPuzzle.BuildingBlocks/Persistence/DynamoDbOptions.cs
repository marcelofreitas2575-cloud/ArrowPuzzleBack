namespace ArrowPuzzle.BuildingBlocks.Persistence;

public sealed class DynamoDbOptions
{
    public const string SectionName = "DynamoDb";

    public string? ServiceUrl { get; set; }

    public string Region { get; set; } = "us-east-1";
}
