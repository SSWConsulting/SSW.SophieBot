using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using SSW.SophieBot.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Persistence
{
    public class CosmosBulkOperations<TDocument> : IBulkOperations<TDocument>
    {
        private readonly List<Task<ItemResponse<TDocument>>> _itemResponses = new();
        private readonly Container _container;
        private readonly PartitionKey _partitionKey;
        private readonly ILogger _logger;

        public CosmosBulkOperations(Container container, string partitionKey, ILogger logger)
        {
            _container = Check.NotNull(container, nameof(container));
            _partitionKey = new PartitionKey(Check.NotNullOrWhiteSpace(partitionKey, nameof(partitionKey)));
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
}
