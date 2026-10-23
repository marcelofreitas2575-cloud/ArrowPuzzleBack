using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.GameSession.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddGameSessionApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
