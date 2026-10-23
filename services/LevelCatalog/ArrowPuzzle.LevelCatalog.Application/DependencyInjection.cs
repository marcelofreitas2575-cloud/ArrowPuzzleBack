using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.LevelCatalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddLevelCatalogApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
