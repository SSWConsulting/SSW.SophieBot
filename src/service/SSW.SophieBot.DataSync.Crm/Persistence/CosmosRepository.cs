using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Persistence
{
    public abstract class CosmosRepository<TDocument> : ITransactionalBulkRepository<TDocument, PatchOperation>, IDisposable
    {
        private FeedIterator<TDocument> _currentIterator = null;
        private string _currentQuery = null;
        private bool _internalHasMoreResults = true;

        public bool HasMoreResults => _internalHasMoreResults;

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
            var container = await GetContainerAsync();
            var queryDefinition = GetQueryDefinition(query, parameters);

            using var snapshotsIterator = container.GetItemQueryIterator<TDocument>(queryDefinition);
            var snapshots = new List<TDocument>();

            while (snapshotsIterator.HasMoreResults)
            {
                var currentSnapshotSet = await snapshotsIterator.ReadNextAsync(cancellationToken);
                foreach (var snapshot in currentSnapshotSet)
                {
                    snapshots.Add(snapshot);
                }
            }

            _internalHasMoreResults = false;
            return snapshots;
        }

        public async Task<List<TDocument>> GetNextAsync(
            string query,
            IEnumerable<(string name, object value)> parameters,
            CancellationToken cancellationToken = default)
        {
            var container = await GetContainerAsync();

            if (_currentIterator == null || _currentQuery != query)
            {
                var queryDefinition = GetQueryDefinition(query, parameters);
                _currentIterator = container.GetItemQueryIterator<TDocument>(queryDefinition);
            }

            var currentSnapshotSet = await _currentIterator.ReadNextAsync(cancellationToken);

            _internalHasMoreResults = _currentIterator.HasMoreResults;

            if (!_internalHasMoreResults)
            {
                _currentIterator.Dispose();
                _currentIterator = null;
                _currentQuery = null;
                return new List<TDocument>();
            }

            return currentSnapshotSet.ToList();
        }

        public async Task<IBulkOperations<TDocument>> BeginBulkAsync()
        {
            return new CosmosBulkOperations(await GetContainerAsync(), Options.OrganizationId, Logger);
        }

        public virtual async Task<ITransactionBatch<PatchOperation>> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var container = await GetContainerAsync();
            var partitionKey = new PartitionKey(Options.OrganizationId);
            var cosmosBatch = container.CreateTransactionalBatch(partitionKey);

            return new CosmosTransactionBatch(cosmosBatch, Logger, cancellationToken);
        }

        public void Dispose()
        {
            _currentIterator?.Dispose();
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

        public class CosmosBulkOperations : IBulkOperations<TDocument>
        {
            private readonly List<Task<ItemResponse<TDocument>>> _itemResponses = new();
            private readonly Container _container;
            private readonly PartitionKey _partitionKey;
            private readonly ILogger _logger;

            public CosmosBulkOperations(Container container, string partitionKey, ILogger logger)
            {
                _container = container ?? throw new ArgumentNullException(nameof(container));
                _partitionKey = new PartitionKey(partitionKey ?? throw new ArgumentNullException(nameof(partitionKey)));
                _logger = logger;
            }

            public void BulkInsert(TDocument item, CancellationToken cancellationToken = default)
            {
                _itemResponses.Add(_container.UpsertItemAsync(
                    item,
                    _partitionKey,
                    cancellationToken: cancellationToken)
                    .ContinueWith(GetBulkOperationContinueFunc(), cancellationToken));
            }

            public void BulkUpdate(TDocument item, CancellationToken cancellationToken = default)
            {
                BulkInsert(item, cancellationToken);
            }

            public void BulkDelete(string id, CancellationToken cancellationToken = default)
            {
                _itemResponses.Add(_container.DeleteItemAsync<TDocument>(
                    id,
                    _partitionKey,
                    cancellationToken: cancellationToken)
                    .ContinueWith(GetBulkOperationContinueFunc(), cancellationToken));
            }

            public async Task<List<TDocument>> ExecuteBulkAsync()
            {
                var successfulDocuments = new List<TDocument>();

                if (!_itemResponses.IsNullOrEmpty())
                {
                    await Task.WhenAll(_itemResponses);

                    successfulDocuments = _itemResponses
                        .Where(task => task.IsCompletedSuccessfully)
                        .Select(task => task.Result.Resource)
                        .ToList();

                    _itemResponses.Clear();
                }

                return successfulDocuments;
            }

            private Func<Task<ItemResponse<TDocument>>, ItemResponse<TDocument>> GetBulkOperationContinueFunc()
            {
                return responseTask =>
                {
                    if (!responseTask.IsCompletedSuccessfully)
                    {
                        var innerExceptions = responseTask.Exception.Flatten();
                        if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                        {
                            _logger?.LogError("Error occurred during Cosmos DB bulk operation: [{StatusCode}] {Message}",
                                cosmosException.StatusCode,
                                cosmosException.Message);
                        }
                        else
                        {
                            _logger?.LogError("Error occurred during Cosmos DB bulk operation: {Exception}",
                                innerExceptions.InnerExceptions.FirstOrDefault());
                        }
                    }

                    return responseTask.Result;
                };
            }
        }

        protected class CosmosTransactionBatch : ITransactionBatch<PatchOperation>
        {
            private TransactionalBatch _batch;
            private readonly ILogger _logger;
            private readonly CancellationToken _cancellationToken;

            public CosmosTransactionBatch(TransactionalBatch batch, ILogger logger = null, CancellationToken cancellationToken = default)
            {
                _batch = batch ?? throw new ArgumentNullException(nameof(batch));
                _logger = logger;
                _cancellationToken = cancellationToken;
            }

            public ITransactionBatch<PatchOperation> Patch(string id, PatchOperation operation)
            {
                var cosmosBatch = _batch.PatchItem(id, new List<PatchOperation> { operation });
                _batch = cosmosBatch;

                return this;
            }

            public async Task<bool> SaveChangesAsync()
            {
                using var batchResponse = await _batch.ExecuteAsync(_cancellationToken);

                if (!batchResponse.IsSuccessStatusCode)
                {
                    _logger?.LogError("Failed on Cosmos DB transaction batch: [{StatusCode}] {Message}", batchResponse.StatusCode, batchResponse.ErrorMessage);
                }

                return batchResponse.IsSuccessStatusCode;
            }
        }
    }
}
