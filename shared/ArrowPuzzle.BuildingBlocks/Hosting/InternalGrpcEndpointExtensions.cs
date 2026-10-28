using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.BuildingBlocks.Hosting;

public static class InternalGrpcEndpointExtensions
{
    public static bool IsInternalGrpcEnabled(this WebApplication app) =>
        app.Configuration.GetValue($"{InternalGrpcOptions.SectionName}:Enabled", false);

    public static WebApplication MapInternalGrpcService<TService>(this WebApplication app)
        where TService : class
    {
        if (!app.IsInternalGrpcEnabled())
        {
            return app;
        }

        app.MapGrpcService<TService>();
        return app;
    }

    public static IServiceCollection AddInternalGrpcIfEnabled(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration.GetValue($"{InternalGrpcOptions.SectionName}:Enabled", false))
        {
            services.AddGrpc();
        }

        return services;
    }
}
