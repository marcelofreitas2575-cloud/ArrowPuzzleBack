namespace ArrowPuzzle.BuildingBlocks.Hosting;

public sealed class InternalGrpcOptions
{
    public const string SectionName = "InternalGrpc";

    /// <summary>
    /// When false (default), gRPC is not mapped — only HTTP (/health, /dapr/*) for Dapr sidecar invoke.
    /// Enable for direct-grpc diagnostic overlay or local gRPC testing.
    /// </summary>
    public bool Enabled { get; set; }
}
