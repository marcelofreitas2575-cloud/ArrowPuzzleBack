using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ArrowPuzzle.BuildingBlocks.Observability;
using ArrowPuzzle.Gateway.Application.Sessions;
using Dapr.Client;
using Google.Protobuf;

namespace ArrowPuzzle.Gateway.Infrastructure.Dapr;

internal static class DaprProtoHttpInvoke
{
    public static async Task<TResponse> PostAsync<TRequest, TResponse>(
        DaprClient daprClient,
        string appId,
        string methodName,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : IMessage<TRequest>
        where TResponse : IMessage<TResponse>, new()
    {
        using var httpRequest = daprClient.CreateInvokeMethodRequest(
            HttpMethod.Post,
            appId,
            methodName);

        var json = JsonFormatter.Default.Format(request);
        httpRequest.Content = new StringContent(json, Encoding.UTF8);
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var correlationId = CorrelationIdContext.CurrentId;
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            httpRequest.Headers.TryAddWithoutValidation(CorrelationIdContext.HeaderName, correlationId);
        }

        using var httpResponse = await daprClient.InvokeMethodWithResponseAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException(string.IsNullOrWhiteSpace(responseJson) ? "Not found." : responseJson);
        }

        if (httpResponse.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new SessionAccessDeniedException(
                string.IsNullOrWhiteSpace(responseJson) ? "Session access denied." : responseJson);
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Dapr invoke {appId}/{methodName} failed with {(int)httpResponse.StatusCode}: {responseJson}");
        }

        return JsonParser.Default.Parse<TResponse>(responseJson);
    }
}
