using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.BuildingBlocks.Hosting;

public static class KestrelGrpcHostingExtensions
{
    /// <summary>
    /// Kestrel for microsservices: HTTP/1 for Dapr + health; optional HTTP/2 (h2c) when <see cref="InternalGrpcOptions.Enabled"/> is true.
    /// </summary>
    public static WebApplicationBuilder ConfigureKestrelForArrowPuzzleService(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<InternalGrpcOptions>(builder.Configuration.GetSection(InternalGrpcOptions.SectionName));
        var grpcEnabled = builder.Configuration.GetValue($"{InternalGrpcOptions.SectionName}:Enabled", false);

        ConfigureListenPort(builder, grpcEnabled ? HttpProtocols.Http1AndHttp2 : HttpProtocols.Http1);

        return builder;
    }

    /// <summary>
    /// Binds Kestrel to HTTP/2 on the URL port from <c>ASPNETCORE_URLS</c> (gRPC over cleartext / h2c).
    /// </summary>
    public static WebApplicationBuilder ConfigureKestrelForInsecureGrpc(this WebApplicationBuilder builder)
    {
        ConfigureListenPort(builder, HttpProtocols.Http1AndHttp2);
        return builder;
    }

    private static void ConfigureListenPort(WebApplicationBuilder builder, HttpProtocols protocols)
    {
        var urls = builder.Configuration["ASPNETCORE_URLS"];
        if (string.IsNullOrWhiteSpace(urls))
        {
            throw new InvalidOperationException(
                "Set ASPNETCORE_URLS (e.g. in launchSettings.json) before configuring Kestrel.");
        }

        var firstUrl = urls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        var bindUri = new Uri(firstUrl.Replace("+", "127.0.0.1", StringComparison.Ordinal).Replace("*", "127.0.0.1", StringComparison.Ordinal));

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(bindUri.Port, listenOptions => listenOptions.Protocols = protocols);
        });
    }
}
