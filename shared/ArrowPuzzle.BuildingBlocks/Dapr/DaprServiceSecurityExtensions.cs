using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.BuildingBlocks.Dapr;

public static class DaprServiceSecurityExtensions
{
    public static IServiceCollection AddArrowPuzzleDaprAppApiToken(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DaprAppApiOptions>(configuration.GetSection(DaprAppApiOptions.SectionName));
        return services;
    }

    public static WebApplication UseArrowPuzzleDaprAppApiToken(this WebApplication app)
    {
        app.UseMiddleware<DaprAppApiTokenMiddleware>();
        return app;
    }
}
