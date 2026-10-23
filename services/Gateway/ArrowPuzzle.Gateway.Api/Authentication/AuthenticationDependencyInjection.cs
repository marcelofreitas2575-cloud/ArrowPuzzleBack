using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ArrowPuzzle.Gateway.Api.Authentication;

public static class AuthenticationDependencyInjection
{
    public static IServiceCollection AddGatewayJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtAuthenticationOptions.SectionName);
        services.Configure<JwtAuthenticationOptions>(jwtSection);
        var options = jwtSection.Get<JwtAuthenticationOptions>() ?? new JwtAuthenticationOptions();

        if (string.IsNullOrWhiteSpace(options.Authority))
        {
            throw new InvalidOperationException(
                $"{JwtAuthenticationOptions.SectionName}:Authority must be configured for the Gateway BFF.");
        }

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.Authority = options.Authority.TrimEnd('/');
                jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
                jwt.IncludeErrorDetails = true;
                jwt.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");
                        logger.LogWarning(context.Exception, "JWT authentication failed.");
                        return Task.CompletedTask;
                    },
                };

                if (!string.IsNullOrWhiteSpace(options.MetadataAddress))
                {
                    jwt.MetadataAddress = options.MetadataAddress.TrimEnd('/');
                    if (!jwt.MetadataAddress.EndsWith("/.well-known/openid-configuration", StringComparison.OrdinalIgnoreCase))
                    {
                        jwt.MetadataAddress += "/.well-known/openid-configuration";
                    }
                }

                var allowedAudiences = new HashSet<string>(StringComparer.Ordinal);
                if (!string.IsNullOrWhiteSpace(options.Audience))
                {
                    allowedAudiences.Add(options.Audience);
                }

                foreach (var audience in options.ValidAudiences)
                {
                    if (!string.IsNullOrWhiteSpace(audience))
                    {
                        allowedAudiences.Add(audience);
                    }
                }

                var validateAudience = allowedAudiences.Count > 0;
                var allowedIssuers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                allowedIssuers.Add(jwt.Authority.TrimEnd('/'));
                foreach (var issuer in options.ValidIssuers)
                {
                    if (!string.IsNullOrWhiteSpace(issuer))
                    {
                        allowedIssuers.Add(issuer.Trim().TrimEnd('/'));
                    }
                }

                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Authority,
                    ValidateAudience = validateAudience,
                    IssuerValidator = (issuer, _, _) =>
                    {
                        var normalized = issuer?.Trim().TrimEnd('/');
                        if (string.IsNullOrWhiteSpace(normalized) || !allowedIssuers.Contains(normalized))
                        {
                            throw new SecurityTokenInvalidIssuerException(
                                $"Issuer '{issuer}' is not in the allowed issuers list.");
                        }

                        return normalized;
                    },
                    ValidAudience = string.IsNullOrWhiteSpace(options.Audience) ? null : options.Audience,
                    ValidAudiences = options.ValidAudiences.Length > 0 ? options.ValidAudiences : null,
                    NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,
                    RoleClaimType = "roles",
                    AudienceValidator = validateAudience
                        ? (audiences, securityToken, _) =>
                        {
                            if (audiences.Any(allowedAudiences.Contains))
                            {
                                return true;
                            }

                            var authorizedParty = securityToken switch
                            {
                                JwtSecurityToken jwtSecurityToken => jwtSecurityToken.Payload.TryGetValue("azp", out var azpObj)
                                    ? azpObj as string
                                    : null,
                                JsonWebToken jsonWebToken => jsonWebToken.GetClaim("azp")?.Value,
                                _ => null,
                            };

                            return !string.IsNullOrEmpty(authorizedParty) && allowedAudiences.Contains(authorizedParty);
                        }
                        : null,
                };

                if (!string.IsNullOrWhiteSpace(options.JwksUri))
                {
                    var jwksUri = options.JwksUri.Trim();
                    jwt.TokenValidationParameters.IssuerSigningKeyResolver =
                        (_, _, keyId, _) => JwksSigningKeyProvider.Resolve(jwksUri, keyId);
                }
            });

        services.AddAuthorization();

        return services;
    }
}
