using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace ArrowPuzzle.Gateway.Api.OpenApi;

public static class GatewayOpenApiExtensions
{
    public const string DocumentName = "v1";

    public static IServiceCollection AddGatewayOpenApiDocument(this IServiceCollection services)
    {
        services.AddOpenApi(
            DocumentName,
            options =>
            {
                options.AddDocumentTransformer(BearerSecurityTransformer);
            });

        return services;
    }

    public static WebApplication MapGatewayOpenApiUi(this WebApplication app)
    {
        app.MapOpenApi("/openapi/{documentName}.json").AllowAnonymous();
        app.MapScalarApiReference(
            "/scalar",
            options =>
            {
                options.WithOpenApiRoutePattern("/openapi/v1.json");
                options.WithTitle("Arrow Puzzle BFF");
            }).AllowAnonymous();

        return app;
    }

    private static Task BearerSecurityTransformer(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Arrow Puzzle BFF (Gateway)",
            Version = "v1",
            Description =
                "HTTP API used by the Android client. Local dev: obtain a JWT via POST /api/v1/auth/dev-token, then call /api/v1/* with Authorization: Bearer.",
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "OIDC access token (Keycloak realm arrow-puzzle).",
        };

        document.SecurityRequirements.Add(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            });

        return Task.CompletedTask;
    }
}
