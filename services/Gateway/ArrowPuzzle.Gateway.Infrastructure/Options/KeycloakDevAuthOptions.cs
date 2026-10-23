namespace ArrowPuzzle.Gateway.Infrastructure.Options;

public sealed class KeycloakDevAuthOptions
{
    public const string SectionName = "KeycloakDevAuth";

    /// <summary>URL interna (Compose) do endpoint token do Keycloak.</summary>
    public string TokenEndpointUrl { get; set; } =
        "http://keycloak:8080/realms/arrow-puzzle/protocol/openid-connect/token";

    public string DevClientId { get; set; } = "arrow-puzzle-dev-cli";

    public bool Enabled { get; set; } = true;
}
