using ArrowPuzzle.BuildingBlocks.Observability;
using Google.Protobuf;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ArrowPuzzle.BuildingBlocks.Dapr;

public static class DaprHttpProtoInvoke
{
    public static RouteHandlerBuilder MapProtoPost<TRequest, TResponse>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<TRequest, CancellationToken, Task<TResponse>> handler)
        where TRequest : IMessage<TRequest>, new()
        where TResponse : IMessage<TResponse>
    {
        return endpoints.MapPost(
            pattern,
            async (HttpContext httpContext, CancellationToken cancellationToken) =>
            {
                try
                {
                    TRequest request;
                    await using (var body = httpContext.Request.Body)
                    {
                        using var reader = new StreamReader(body);
                        var json = await reader.ReadToEndAsync(cancellationToken);
                        request = string.IsNullOrWhiteSpace(json)
                            ? new TRequest()
                            : JsonParser.Default.Parse<TRequest>(json);
                    }

                    var response = await handler(request, cancellationToken);
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(JsonFormatter.Default.Format(response), cancellationToken);
                }
                catch (KeyNotFoundException ex)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    await httpContext.Response.WriteAsync(ex.Message, cancellationToken);
                }
                catch (ArgumentException ex)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await httpContext.Response.WriteAsync(ex.Message, cancellationToken);
                }
                catch (Exception ex) when (ex.GetType().Name == "SessionAccessDeniedException")
                {
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await httpContext.Response.WriteAsync(ex.Message, cancellationToken);
                }
            });
    }
}
