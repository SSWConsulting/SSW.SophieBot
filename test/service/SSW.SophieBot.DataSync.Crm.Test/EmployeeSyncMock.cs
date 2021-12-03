using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Options;
using Moq;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Persistence;
using SSW.SophieBot.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Test
{
    public class EmployeeSyncMock
    {
        public List<CrmEmployee> CrmEmployees = new()
        {
            new CrmEmployee
            {
                Systemuserid = "a",
                Organizationid = "ssw",
                Firstname = "Jim",
                Lastname = "Northwind",
                Fullname = "Jim Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "b",
                Organizationid = "ssw",
                Firstname = "Jack",
                Lastname = "Northwind",
                Fullname = "Jack Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "d",
                Organizationid = "ssw",
                Firstname = "John",
                Lastname = "Northwind",
                Fullname = "John Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "e",
                Organizationid = "ssw",
                Firstname = "Hans",
                Lastname = "Northwind",
                Fullname = "Hans Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "f",
                Organizationid = "ssw",
                Firstname = "Alex",
                Lastname = "Northwind",
                Fullname = "Alex Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "g",
                Organizationid = "ssw",
                Firstname = "Taya",
                Lastname = "Northwind",
                Fullname = "Taya Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "h",
                Organizationid = "ssw",
                Firstname = "Nick",
                Lastname = "Northwind",
                Fullname = "Nick Northwind",
                Modifiedon = DateTime.MinValue
            }
        };

        public List<SyncSnapshot> InitialSnapshots = new()
        {

        };

        public List<MqMessage<Employee>> MqMessages = new();

        private readonly List<SyncSnapshot> _successfulBulkSnapshots = new();

        public ISyncVersionGenerator MockSyncVersionGenerator(string syncVersion)
        {
            var mock = new Mock<ISyncVersionGenerator>();
            mock.Setup(generator => generator.GenerateAsync(It.IsAny<CancellationToken>()).Result).Returns(syncVersion);
            return mock.Object;
        }

        public Mock<ITransactionalBulkRepository<SyncSnapshot, PatchOperation>> GetBasicSyncSnapshotRepositoryMock(string syncVersion)
        {
            var mock = new Mock<ITransactionalBulkRepository<SyncSnapshot, PatchOperation>>();
            mock.Setup(repo => repo.GetAllAsync(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<(string, object)>>(),
                It.IsAny<CancellationToken>()).Result)
                .Returns(() => InitialSnapshots);

            mock.Setup(repo => repo.GetAsyncPages(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<(string, object)>>(),
                It.IsAny<CancellationToken>()))
                .Returns(() => new List<IEnumerable<SyncSnapshot>>
                {
                    InitialSnapshots.Where(snapshot => snapshot.SyncVersion != syncVersion)
                }.ToAsyncEnumerable());

            var upsertCallback = (SyncSnapshot snapshot, CancellationToken _) =>
            {
                var oldSnapshot = InitialSnapshots.FirstOrDefault(s => s.Id == snapshot.Id);
                if (oldSnapshot != null)
                {
                    oldSnapshot.OrganizationId = snapshot.OrganizationId;
                    oldSnapshot.Modifiedon = snapshot.Modifiedon;
                    oldSnapshot.SyncVersion = snapshot.SyncVersion;
                }
                else
                {
                    InitialSnapshots.Add(snapshot);
                }

                _successfulBulkSnapshots.Add(snapshot);
            };

            var deleteCallback = (string id, CancellationToken _) =>
            {
                InitialSnapshots.RemoveAll(s => s.Id == id);
                _successfulBulkSnapshots.Add(new SyncSnapshot
                {
                    Id = id
                });
            };

            var batchMock = new Mock<ITransactionBatch<PatchOperation>>();
            batchMock.Setup(batch => batch.SaveChangesAsync().Result).Callback(() =>
            {
                InitialSnapshots.ForEach(snapshot => snapshot.SyncVersion = syncVersion);
            }).Returns(true);
            mock.Setup(repo => repo.BeginTransactionAsync(It.IsAny<CancellationToken>()).Result).Returns(batchMock.Object);

            var bulkMock = new Mock<IBulkOperations<SyncSnapshot>>();
            bulkMock.Setup(bulk => bulk.BulkInsert(It.IsAny<SyncSnapshot>(), It.IsAny<CancellationToken>()))
                .Callback(upsertCallback);
            bulkMock.Setup(bulk => bulk.BulkUpdate(It.IsAny<SyncSnapshot>(), It.IsAny<CancellationToken>()))
                .Callback(upsertCallback);
            bulkMock.Setup(bulk => bulk.BulkDelete(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback(deleteCallback);
            bulkMock.Setup(bulk => bulk.ExecuteBulkAsync().Result).Returns(() => _successfulBulkSnapshots);

            mock.Setup(repo => repo.BeginBulkAsync().Result)
                .Callback(() => _successfulBulkSnapshots.Clear())
                .Returns(bulkMock.Object);

            return mock;
        }

        public IPagedOdataSyncService<CrmEmployee> MockEmployeeOdataSyncService()
        {
            var mock = new Mock<IPagedOdataSyncService<CrmEmployee>>();
            mock.SetupSequence(service => service.HasMoreResults)
                .Returns(true)
                .Returns(false);
            mock.SetupSequence(service => service.GetNextAsync(It.IsAny<CancellationToken>()).Result)
                .Returns(() => new OdataPagedResponse<CrmEmployee>
                {
                    Value = CrmEmployees.Take(3).ToList(),
                    OdataNextLink = "next page"
                })
                .Returns(() => new OdataPagedResponse<CrmEmployee>
                {
                    Value = CrmEmployees.Skip(3).ToList()
                });

            return mock.Object;
        }

        public IBatchMessageService<MqMessage<Employee>, string> MockServiceBusClient()
        {
            var mock = new Mock<IBatchMessageService<MqMessage<Employee>, string>>();
            mock.Setup(service => service.SendMessageAsync(
                It.IsAny<IEnumerable<MqMessage<Employee>>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .Callback((IEnumerable<MqMessage<Employee>> messages, string _, CancellationToken _) =>
                {
                    MqMessages = MqMessages.Concat(messages).ToList();
                });
            return mock.Object;
        }

        public IOptions<SyncOptions> MockSyncOptions()
        {
            var mock = new Mock<IOptions<SyncOptions>>();
            mock.SetupGet(option => option.Value).Returns(new SyncOptions
            {
                OrganizationId = "ssw",
                EmployeeSync = new SyncFunctionOptions()
            });
            return mock.Object;
        }

        public TimerInfo MockTimerInfo()
        {
            return new TimerInfo(null, null);
        }
    }
}
