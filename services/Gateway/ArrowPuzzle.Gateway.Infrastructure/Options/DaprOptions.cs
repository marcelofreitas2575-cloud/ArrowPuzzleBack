namespace ArrowPuzzle.Gateway.Infrastructure.Options;

public sealed class DaprOptions
{
    public const string SectionName = "Dapr";

    public bool Enabled { get; set; }
}

public static class DaprAppIds
{
    public const string GameSession = "game-session";

    public const string LevelCatalog = "level-catalog";

    public const string PlayerProgress = "player-progress";
}

/// <summary>
/// Rotas HTTP expostas nos microsserviços para invocação Dapr (app-protocol http).
/// gRPC direto (BFF → serviço) continua disponível via overlay direct-grpc.
/// </summary>
internal static class DaprInvokeMethods
{
    public const string GameSessionCreate = "dapr/create-session";

    public const string GameSessionApplyMove = "dapr/apply-move";

    public const string GameSessionGet = "dapr/get-session";

    public const string LevelCatalogGetLevel = "dapr/get-level";

    public const string PlayerProgressGet = "dapr/get-progress";

    public const string PlayerProgressComplete = "dapr/complete-level";
}
