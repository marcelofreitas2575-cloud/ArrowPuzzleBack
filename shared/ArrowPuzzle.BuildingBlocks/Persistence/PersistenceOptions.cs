namespace ArrowPuzzle.BuildingBlocks.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    /// <summary>InMemory (default) or DynamoDb.</summary>
    public string Provider { get; set; } = "InMemory";

    public bool UseDynamoDb =>
        string.Equals(Provider, "DynamoDb", StringComparison.OrdinalIgnoreCase);
}
