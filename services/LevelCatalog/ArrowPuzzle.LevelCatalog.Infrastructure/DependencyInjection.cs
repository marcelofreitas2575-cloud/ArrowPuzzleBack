using ArrowPuzzle.LevelCatalog.Application.Ports;
using ArrowPuzzle.LevelCatalog.Infrastructure.Levels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.LevelCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLevelCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = configuration;
        services.AddSingleton<ProceduralLevelGenerator>();
        services.AddSingleton<ILevelCatalogRepository, ProceduralLevelCatalogRepository>();
        return services;
    }
}
