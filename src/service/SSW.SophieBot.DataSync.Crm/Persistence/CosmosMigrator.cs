using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.Persistence;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Persistence
{
    public class CosmosMigrator : IPersistenceMigrator<Container, SyncFunctionOptions>
    {
        private readonly IServiceProvider _serviceProvider;

        public CosmosMigrator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<Container> MigrateAsync(SyncFunctionOptions options, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var cosmosClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();

            //var migrateOptions = new SyncOptions();
            //action?.Invoke(migrateOptions);

            var dbResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
                options.DatabaseId,
                cancellationToken: cancellationToken);

            if (!HttpClientHelper.IsSuccessStatusCode(dbResponse.StatusCode))
            {
                throw new SophieBotDataSyncCrmException($"Failed to migrate Cosmos DB on database creation: {dbResponse.Resource.Id}, " +
                    $"status code: {dbResponse.StatusCode}");
            }

            var containerResponse = await dbResponse.Database.CreateContainerIfNotExistsAsync(
                options.ContainerId,
                "/organizationId",
                cancellationToken: cancellationToken);

            if (!HttpClientHelper.IsSuccessStatusCode(containerResponse.StatusCode))
            {
                throw new SophieBotDataSyncCrmException($"Failed to migrate Cosmos DB on container creation: {containerResponse.Resource.Id}, " +
                    $"status code: {containerResponse.StatusCode}");
            }

            return containerResponse.Container;
        }
    }
}
