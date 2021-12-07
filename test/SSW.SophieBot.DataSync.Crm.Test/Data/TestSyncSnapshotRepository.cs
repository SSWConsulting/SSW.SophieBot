using Microsoft.Azure.Cosmos;
using Moq;
using SSW.SophieBot.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Test.Data
{
    public class TestSyncSnapshotRepository : ITransactionalBulkRepository<SyncSnapshot, PatchOperation>
    {
        private readonly string _syncVersion;
        private readonly TestData _testData;
        private readonly List<SyncSnapshot> _bulkSnapshots = new();

        public TestSyncSnapshotRepository(string syncVersion, TestData testData)
        {
            _syncVersion = syncVersion;
            _testData = testData;
        }

        public virtual Task<IBulkOperations<SyncSnapshot>> BeginBulkAsync()
        {
            var upsertCallback = (SyncSnapshot snapshot, CancellationToken _) =>
            {
                var oldSnapshot = _testData.Snapshots.FirstOrDefault(s => s.Id == snapshot.Id);
                if (oldSnapshot != null)
                {
                    oldSnapshot.OrganizationId = snapshot.OrganizationId;
                    oldSnapshot.Modifiedon = snapshot.Modifiedon;
                    oldSnapshot.SyncVersion = snapshot.SyncVersion;
                }
                else
                {
                    _testData.Snapshots.Add(snapshot);
                }

                _bulkSnapshots.Add(snapshot);
            };

            var deleteCallback = (string id, CancellationToken _) =>
            {
                _testData.Snapshots.RemoveAll(s => s.Id == id);
                _bulkSnapshots.Add(new SyncSnapshot
                {
                    Id = id
                });
            };

            var bulkMock = new Mock<IBulkOperations<SyncSnapshot>>();
            bulkMock.Setup(bulk => bulk.BulkInsert(It.IsAny<SyncSnapshot>(), It.IsAny<CancellationToken>()))
                .Callback(upsertCallback);
            bulkMock.Setup(bulk => bulk.BulkUpdate(It.IsAny<SyncSnapshot>(), It.IsAny<CancellationToken>()))
                .Callback(upsertCallback);
            bulkMock.Setup(bulk => bulk.BulkDelete(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback(deleteCallback);
            bulkMock.Setup(bulk => bulk.ExecuteBulkAsync().Result).Returns(() => _bulkSnapshots);

            _bulkSnapshots.Clear();

            return Task.FromResult(bulkMock.Object);
        }

        public virtual Task<ITransactionBatch<PatchOperation>> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var batchMock = new Mock<ITransactionBatch<PatchOperation>>();
            batchMock.Setup(batch => batch.SaveChangesAsync().Result).Callback(() =>
            {
                _testData.Snapshots.ForEach(snapshot => snapshot.SyncVersion = _syncVersion);
            }).Returns(true);

            return Task.FromResult(batchMock.Object);
        }

        public virtual Task<List<SyncSnapshot>> GetAllAsync(string query, IEnumerable<(string name, object value)> parameters, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_testData.Snapshots);
        }

        public virtual IAsyncEnumerable<IEnumerable<SyncSnapshot>> GetAsyncPages(string query, IEnumerable<(string name, object value)> parameters, CancellationToken cancellationToken = default)
        {
            return new List<IEnumerable<SyncSnapshot>>
            {
                _testData.Snapshots.Where(snapshot => snapshot.SyncVersion != _syncVersion)
            }.ToAsyncEnumerable();
        }
    }
}
