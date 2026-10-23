using ArrowPuzzle.BuildingBlocks.Persistence;
using ArrowPuzzle.GameSession.Application.Ports;
using ArrowPuzzle.GameSession.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.GameSession.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGameSessionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var persistence = configuration.GetSection(PersistenceOptions.SectionName).Get<PersistenceOptions>()
            ?? new PersistenceOptions();

        if (persistence.UseDynamoDb)
        {
            services.AddArrowPuzzleDynamoDb(configuration);
            services.AddSingleton<IGameSessionRepository, DynamoGameSessionRepository>();
        }
        else
        {
            services.AddSingleton<IGameSessionRepository, InMemoryGameSessionRepository>();
        }

        return services;
    }
}
