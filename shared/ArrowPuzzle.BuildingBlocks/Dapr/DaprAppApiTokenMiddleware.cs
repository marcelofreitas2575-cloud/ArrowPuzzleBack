using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ArrowPuzzle.BuildingBlocks.Dapr;

/// <summary>
/// Ensures only the Dapr sidecar (shared app API token) can call /dapr/* invoke routes.
/// </summary>
public sealed class DaprAppApiTokenMiddleware
{
    public const string TokenHeaderName = "dapr-api-token";

    private readonly RequestDelegate _next;
    private readonly string? _expectedToken;

    public DaprAppApiTokenMiddleware(RequestDelegate next, IOptions<DaprAppApiOptions> options)
    {
        _next = next;
        _expectedToken = options.Value.Token;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!string.IsNullOrWhiteSpace(_expectedToken)
            && context.Request.Path.StartsWithSegments("/dapr", StringComparison.OrdinalIgnoreCase))
        {
            var provided = context.Request.Headers[TokenHeaderName].ToString();
            if (!string.Equals(provided, _expectedToken, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Dapr app API token is missing or invalid.");
                return;
            }
        }

        await _next(context);
    }
}
