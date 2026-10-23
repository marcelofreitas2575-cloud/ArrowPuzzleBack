using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.PlayerProgress.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlayerProgressApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
