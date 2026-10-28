namespace ArrowPuzzle.BuildingBlocks.Dapr;

public sealed class DaprAppApiOptions
{
    public const string SectionName = "DaprAppApi";

    /// <summary>
    /// When set, HTTP routes under /dapr/* require the dapr-api-token header (sidecar → app).
    /// </summary>
    public string? Token { get; set; }
}
