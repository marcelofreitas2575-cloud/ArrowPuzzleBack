using ArrowPuzzle.BuildingBlocks.Persistence;
using ArrowPuzzle.PlayerProgress.Application.Ports;
using ArrowPuzzle.PlayerProgress.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.PlayerProgress.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlayerProgressInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var persistence = configuration.GetSection(PersistenceOptions.SectionName).Get<PersistenceOptions>()
            ?? new PersistenceOptions();

        if (persistence.UseDynamoDb)
        {
            services.AddArrowPuzzleDynamoDb(configuration);
            services.AddSingleton<IPlayerProgressRepository, DynamoPlayerProgressRepository>();
        }
        else
        {
            services.AddSingleton<IPlayerProgressRepository, InMemoryPlayerProgressRepository>();
        }

        return services;
    }
}
