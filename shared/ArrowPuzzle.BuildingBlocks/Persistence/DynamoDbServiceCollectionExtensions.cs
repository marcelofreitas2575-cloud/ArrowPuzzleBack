using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.BuildingBlocks.Persistence;

public static class DynamoDbServiceCollectionExtensions
{
    public static IServiceCollection AddArrowPuzzleDynamoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PersistenceOptions>(configuration.GetSection(PersistenceOptions.SectionName));
        services.Configure<DynamoDbOptions>(configuration.GetSection(DynamoDbOptions.SectionName));

        services.AddSingleton<IAmazonDynamoDB>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<DynamoDbOptions>>().Value;

            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region),
            };

            if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
            {
                config.ServiceURL = options.ServiceUrl;
            }

            // DynamoDB Local exige access key no request (valores fictícios são aceitos).
            return string.IsNullOrWhiteSpace(options.ServiceUrl)
                ? new AmazonDynamoDBClient(config)
                : new AmazonDynamoDBClient(new BasicAWSCredentials("local", "local"), config);
        });

        services.AddSingleton<DynamoDbTableManager>();
        services.AddHostedService<DynamoDbTablesHostedService>();

        return services;
    }
}
