namespace ArrowPuzzle.Gateway.Api.Authentication;

public sealed class JwtAuthenticationOptions
{
    public const string SectionName = "Authentication:Jwt";

    /// <summary>OIDC issuer / authority (e.g. https://host/realms/realm-name).</summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Optional metadata base URL when JWKS discovery runs on an internal host (Docker) while tokens keep the public issuer.
    /// </summary>
    public string? MetadataAddress { get; set; }

    /// <summary>
    /// JWKS URL reachable from the Gateway container (e.g. http://keycloak:8080/.../certs).
    /// Required when OIDC metadata advertises localhost JWKS but tokens keep the public issuer.
    /// </summary>
    public string? JwksUri { get; set; }

    public string? Audience { get; set; }

    public string[] ValidAudiences { get; set; } = [];

    /// <summary>
    /// Issuers aceitos no token (ex.: IP da LAN no celular). Se vazio, usa só <see cref="Authority"/>.
    /// </summary>
    public string[] ValidIssuers { get; set; } = [];

    public bool RequireHttpsMetadata { get; set; } = true;
}
