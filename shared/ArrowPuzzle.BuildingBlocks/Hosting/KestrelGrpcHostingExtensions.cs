using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace ArrowPuzzle.BuildingBlocks.Hosting;

public static class KestrelGrpcHostingExtensions
{
    /// <summary>
    /// Binds Kestrel to HTTP/2 on the URL port from <c>ASPNETCORE_URLS</c> (gRPC over cleartext / h2c).
    /// </summary>
    public static WebApplicationBuilder ConfigureKestrelForInsecureGrpc(this WebApplicationBuilder builder)
    {
        var urls = builder.Configuration["ASPNETCORE_URLS"];
        if (string.IsNullOrWhiteSpace(urls))
        {
            throw new InvalidOperationException(
                "Set ASPNETCORE_URLS (e.g. in launchSettings.json) before calling ConfigureKestrelForInsecureGrpc.");
        }

        var firstUrl = urls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        var bindUri = new Uri(firstUrl.Replace("+", "127.0.0.1", StringComparison.Ordinal).Replace("*", "127.0.0.1", StringComparison.Ordinal));

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(bindUri.Port, listenOptions =>
            {
                // gRPC (h2c) + HTTP/1.1 na mesma porta (health, invocação Dapr via HTTP).
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            });
        });

        return builder;
    }
}
