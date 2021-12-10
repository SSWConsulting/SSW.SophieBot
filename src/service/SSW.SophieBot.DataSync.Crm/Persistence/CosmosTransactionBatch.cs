using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using SSW.SophieBot.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Persistence
{
    public class CosmosTransactionBatch : ITransactionBatch<PatchOperation>
    {
        private TransactionalBatch _batch;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancellationToken;

        public CosmosTransactionBatch(TransactionalBatch batch, ILogger logger = null, CancellationToken cancellationToken = default)
        {
            _batch = Check.NotNull(batch, nameof(batch));
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
