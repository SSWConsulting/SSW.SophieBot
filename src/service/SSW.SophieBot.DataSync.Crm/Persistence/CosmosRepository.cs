using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.Persistence;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Persistence
{
    public abstract class CosmosRepository<TDocument> : ITransactionalBulkRepository<TDocument, PatchOperation>
    {
        protected virtual IPersistenceMigrator<Container, SyncFunctionOptions> Migrator { get; }

        protected virtual SyncOptions Options { get; }

        protected virtual ILogger<CosmosRepository<TDocument>> Logger { get; }

        public CosmosRepository(
            IPersistenceMigrator<Container, SyncFunctionOptions> migrator,
            IOptions<SyncOptions> options,
            ILogger<CosmosRepository<TDocument>> logger)
        {
            Migrator = migrator;
            Options = options.Value;
            Logger = logger;
        }

        public async Task<List<TDocument>> GetAllAsync(
            string query,
            IEnumerable<(string name, object value)> parameters,
            CancellationToken cancellationToken = default)
        {
            var documents = new List<TDocument>();
            await foreach (var page in GetAsyncPages(query, parameters, cancellationToken))
            {
                documents.AddRange(page);
            }

            return documents;
        }

        public async IAsyncEnumerable<IEnumerable<TDocument>> GetAsyncPages(
            string query,
            IEnumerable<(string name, object value)> parameters,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var container = await GetContainerAsync();
            var queryDefinition = GetQueryDefinition(query, parameters);
            var iterator = container.GetItemQueryIterator<TDocument>(queryDefinition);

            while (iterator.HasMoreResults)
            {
                yield return await iterator.ReadNextAsync(cancellationToken);
            }
        }

        public async Task<IBulkOperations<TDocument>> BeginBulkAsync()
        {
            return new CosmosBulkOperations<TDocument>(await GetContainerAsync(), Options.OrganizationId, Logger);
        }

        public virtual async Task<ITransactionBatch<PatchOperation>> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var container = await GetContainerAsync();
            var partitionKey = new PartitionKey(Options.OrganizationId);
            var cosmosBatch = container.CreateTransactionalBatch(partitionKey);

            return new CosmosTransactionBatch(cosmosBatch, Logger, cancellationToken);
        }

        protected abstract Task<Container> GetContainerAsync();

        protected virtual QueryDefinition GetQueryDefinition(string query, IEnumerable<(string name, object value)> parameters)
        {
            var queryDefinition = new QueryDefinition(query);
            foreach (var (name, value) in parameters)
            {
                queryDefinition = queryDefinition.WithParameter(name, value);
            }

            return queryDefinition;
        }
    }
}
