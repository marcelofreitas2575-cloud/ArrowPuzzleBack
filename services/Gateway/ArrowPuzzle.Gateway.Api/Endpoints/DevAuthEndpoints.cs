using System.Net.Http.Headers;
using System.Text.Json;
using ArrowPuzzle.Gateway.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ArrowPuzzle.Gateway.Api.Endpoints;

public static class DevAuthEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
    };

    public static void MapDevAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/auth/dev-token", RequestDevTokenAsync).AllowAnonymous();
    }

    private static async Task<IResult> RequestDevTokenAsync(
        DevTokenRequest? body,
        IHttpClientFactory httpClientFactory,
        IOptions<KeycloakDevAuthOptions> options,
        CancellationToken cancellationToken)
    {
        var authOptions = options.Value;
        if (!authOptions.Enabled)
        {
            return Results.NotFound();
        }

        var username = string.IsNullOrWhiteSpace(body?.Username) ? "guest-player" : body.Username.Trim();
        var password = string.IsNullOrWhiteSpace(body?.Password) ? username : body.Password.Trim();

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = authOptions.DevClientId,
            ["username"] = username,
            ["password"] = password,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, authOptions.TokenEndpointUrl)
        {
            Content = new FormUrlEncodedContent(form),
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var client = httpClientFactory.CreateClient(nameof(DevAuthEndpoints));
        using var response = await client.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem(
                detail: payload,
                statusCode: StatusCodes.Status502BadGateway,
                title: "Keycloak token request failed");
        }

        var token = JsonSerializer.Deserialize<JsonElement>(payload, JsonOptions);
        return Results.Json(token, JsonOptions);
    }

    public sealed record DevTokenRequest(string? Username = null, string? Password = null);
}
