using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.DataSync.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Persistence
{
    public abstract class CosmosRepository<TDocument> : ITransactionalBulkRepository<TDocument, PatchOperation>, IDisposable
    {
        private List<Task> _currentBulk = null;
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

        public virtual async Task<ITransactionBatch<PatchOperation>> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var container = await GetContainerAsync(cancellationToken);
            var partitionKey = new PartitionKey(Options.OrganizationId);
            var cosmosBatch = container.CreateTransactionalBatch(partitionKey);

            return new CosmosTransactionBatch(cosmosBatch, Logger, cancellationToken);
        }

        public async Task<List<TDocument>> GetAllAsync(string query, CancellationToken cancellationToken = default)
        {
            var container = await GetContainerAsync(cancellationToken);
            var queryDefinition = new QueryDefinition(query);

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

        public async Task<List<TDocument>> GetNextAsync(string query, CancellationToken cancellationToken = default)
        {
            var container = await GetContainerAsync(cancellationToken);

            if (_currentIterator == null || _currentQuery != query)
            {
                var queryDefinition = new QueryDefinition(query);
                _currentIterator = container.GetItemQueryIterator<TDocument>(queryDefinition);
            }

            _internalHasMoreResults = _currentIterator.HasMoreResults;

            if (!_internalHasMoreResults)
            {
                _currentIterator.Dispose();
                _currentIterator = null;
                _currentQuery = null;
                return new List<TDocument>();
            }

            var currentSnapshotSet = await _currentIterator.ReadNextAsync(cancellationToken);

            return currentSnapshotSet.ToList();
        }

        public void BeginBulk()
        {
            _currentBulk = new List<Task>();
        }

        public async Task BulkInsertAsync(
            TDocument item,
            Action<Task> callbackAction = null,
            CancellationToken cancellationToken = default)
        {
            AssertAllowBulk();
            var container = await GetContainerAsync(cancellationToken);
            var partitionKey = new PartitionKey(Options.OrganizationId);

            _currentBulk.Add(container.UpsertItemAsync(
                item,
                partitionKey,
                cancellationToken: cancellationToken)
                .ContinueWith(callbackAction, cancellationToken));
        }

        public async Task BulkUpdateAsync(
            TDocument item,
            Action<Task> callbackAction = null,
            CancellationToken cancellationToken = default)
        {
            await BulkInsertAsync(item, callbackAction, cancellationToken);
        }

        public async Task BulkDeleteAsync(
            string id,
            Action<Task> callbackAction = null,
            CancellationToken cancellationToken = default)
        {
            AssertAllowBulk();
            var container = await GetContainerAsync(cancellationToken);
            var partitionKey = new PartitionKey(Options.OrganizationId);

            _currentBulk.Add(container.DeleteItemAsync<TDocument>(
                id,
                partitionKey,
                cancellationToken: cancellationToken)
                .ContinueWith(callbackAction, cancellationToken));
        }

        public async Task ExecuteBulkAsync()
        {
            if (!_currentBulk.IsNullOrEmpty())
            {
                await Task.WhenAll(_currentBulk);
            }

            _currentBulk = null;
        }

        public void Dispose()
        {
            _currentIterator?.Dispose();
        }

        protected abstract Task<Container> GetContainerAsync(CancellationToken cancellationToken = default);

        private void AssertAllowBulk()
        {
            if (_currentBulk == null)
            {
                throw new InvalidOperationException($"Please use {nameof(BeginBulk)} before bulk operations");
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
