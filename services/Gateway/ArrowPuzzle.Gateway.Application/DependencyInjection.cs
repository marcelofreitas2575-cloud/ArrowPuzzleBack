using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.Gateway.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddGatewayApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
