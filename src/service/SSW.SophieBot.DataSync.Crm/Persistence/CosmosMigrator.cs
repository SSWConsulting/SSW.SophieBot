using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.DataSync.Domain.Persistence;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Persistence
{
    public class CosmosMigrator : IPersistenceMigrator<Container, SyncFunctionOptions>
    {
        private readonly IServiceProvider _serviceProvider;
        private Container _container = null;

        public CosmosMigrator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<Container> MigrateAsync(SyncFunctionOptions options, CancellationToken cancellationToken = default)
        {
            if (_container != null)
            {
                return _container;
            }

            using var scope = _serviceProvider.CreateScope();
            var cosmosClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();

            //var migrateOptions = new SyncOptions();
            //action?.Invoke(migrateOptions);

            var dbResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
                options.DatabaseId,
                cancellationToken: cancellationToken);

            if (dbResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new SophieBotDataSyncCrmException($"Failed to migrate Cosmos DB on database creation: {dbResponse.Resource.Id}");
            }

            var containerResponse = await dbResponse.Database.CreateContainerIfNotExistsAsync(
                options.ContainerId,
                "/organizationId",
                cancellationToken: cancellationToken);

            if (containerResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new SophieBotDataSyncCrmException($"Failed to migrate Cosmos DB on container creation: {containerResponse.Resource.Id}");
            }

            _container = containerResponse.Container;

            return _container;
        }
    }
}
