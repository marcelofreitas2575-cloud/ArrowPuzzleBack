using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ArrowPuzzle.BuildingBlocks.Observability;

/// <summary>
/// RED-friendly HTTP metrics (rate / errors / duration) via ASP.NET Core instrumentation + Prometheus scrape.
/// </summary>
public static class OpenTelemetryExtensions
{
    public const string MetricsPath = "/metrics";

    public static WebApplicationBuilder AddArrowPuzzleOpenTelemetry(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        return builder;
    }

    public static WebApplication UseArrowPuzzlePrometheusMetrics(this WebApplication app)
    {
        app.UseOpenTelemetryPrometheusScrapingEndpoint(context =>
            context.Request.Path.StartsWithSegments(MetricsPath, StringComparison.OrdinalIgnoreCase));
        return app;
    }
}
