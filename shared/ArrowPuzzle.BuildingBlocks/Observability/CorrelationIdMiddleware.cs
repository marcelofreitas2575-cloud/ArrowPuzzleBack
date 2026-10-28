using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ArrowPuzzle.BuildingBlocks.Observability;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        CorrelationIdContext.CurrentId = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdContext.HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   [CorrelationIdContext.LogPropertyName] = correlationId,
               }))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdContext.HeaderName, out var incoming)
            && !string.IsNullOrWhiteSpace(incoming))
        {
            return incoming.ToString().Trim();
        }

        return Guid.NewGuid().ToString("N");
    }
}
