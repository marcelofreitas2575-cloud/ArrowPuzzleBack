using Dapr.Client;
using ArrowPuzzle.Gateway.Application.Ports;
using ArrowPuzzle.Gateway.Infrastructure.Dapr;
using ArrowPuzzle.Gateway.Infrastructure.Grpc;
using ArrowPuzzle.Gateway.Infrastructure.Options;
using ArrowPuzzle.Protos.GameSession.V1;
using ArrowPuzzle.Protos.LevelCatalog.V1;
using ArrowPuzzle.Protos.PlayerProgress.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArrowPuzzle.Gateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGatewayInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        services.Configure<ServiceEndpointsOptions>(configuration.GetSection(ServiceEndpointsOptions.SectionName));
        services.Configure<DaprOptions>(configuration.GetSection(DaprOptions.SectionName));
        services.Configure<ProductOptions>(configuration.GetSection(ProductOptions.SectionName));
        services.Configure<KeycloakDevAuthOptions>(configuration.GetSection(KeycloakDevAuthOptions.SectionName));

        var daprOptions = configuration.GetSection(DaprOptions.SectionName).Get<DaprOptions>() ?? new DaprOptions();

        // Padrão Compose: Dapr. gRPC direto (Services__*) só com Dapr__Enabled=false (diagnóstico).
        if (daprOptions.Enabled)
        {
            services.AddSingleton(_ => new DaprClientBuilder().Build());
            services.AddSingleton<IGameSessionBackend, DaprGameSessionBackend>();
            services.AddSingleton<ILevelCatalogBackend, DaprLevelCatalogBackend>();
            services.AddSingleton<IPlayerProgressBackend, DaprPlayerProgressBackend>();
        }
        else
        {
            AddDirectGrpcClients(services);
            services.AddSingleton<IGameSessionBackend, GrpcGameSessionBackend>();
            services.AddSingleton<ILevelCatalogBackend, GrpcLevelCatalogBackend>();
            services.AddSingleton<IPlayerProgressBackend, GrpcPlayerProgressBackend>();
        }

        return services;
    }

    private static void AddDirectGrpcClients(IServiceCollection services)
    {
        services
            .AddGrpcClient<GameSessionService.GameSessionServiceClient>((serviceProvider, options) =>
            {
                var endpoints = serviceProvider.GetRequiredService<IOptions<ServiceEndpointsOptions>>().Value;
                options.Address = new Uri(endpoints.GameSession);
            })
            .AddStandardResilienceHandler();

        services
            .AddGrpcClient<LevelCatalogService.LevelCatalogServiceClient>((serviceProvider, options) =>
            {
                var endpoints = serviceProvider.GetRequiredService<IOptions<ServiceEndpointsOptions>>().Value;
                options.Address = new Uri(endpoints.LevelCatalog);
            })
            .AddStandardResilienceHandler();

        services
            .AddGrpcClient<PlayerProgressService.PlayerProgressServiceClient>((serviceProvider, options) =>
            {
                var endpoints = serviceProvider.GetRequiredService<IOptions<ServiceEndpointsOptions>>().Value;
                options.Address = new Uri(endpoints.PlayerProgress);
            })
            .AddStandardResilienceHandler();
    }
}
