using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Options;
using Moq;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.DataSync.Domain.Employees;
using SSW.SophieBot.DataSync.Domain.Persistence;
using SSW.SophieBot.DataSync.Domain.Sync;
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
                Fullname = "Jim Northwind"
            },
            new CrmEmployee
            {
                Systemuserid = "b",
                Organizationid = "ssw",
                Firstname = "Jack",
                Lastname = "Northwind",
                Fullname = "Jack Northwind"
            },
            new CrmEmployee
            {
                Systemuserid = "d",
                Organizationid = "ssw",
                Firstname = "John",
                Lastname = "Northwind",
                Fullname = "John Northwind"
            },
            new CrmEmployee
            {
                Systemuserid = "e",
                Organizationid = "ssw",
                Firstname = "Hans",
                Lastname = "Northwind",
                Fullname = "Hans Northwind"
            },
            new CrmEmployee
            {
                Systemuserid = "f",
                Organizationid = "ssw",
                Firstname = "Alex",
                Lastname = "Northwind",
                Fullname = "Alex Northwind"
            },
            new CrmEmployee
            {
                Systemuserid = "g",
                Organizationid = "ssw",
                Firstname = "Taya",
                Lastname = "Northwind",
                Fullname = "Taya Northwind"
            },
            new CrmEmployee
            {
                Systemuserid = "h",
                Organizationid = "ssw",
                Firstname = "Nick",
                Lastname = "Northwind",
                Fullname = "Nick Northwind"
            }
        };

        public List<SyncSnapshot> InitialSnapshots = new()
        {
            
        };

        public IPagedOdataSyncService<CrmEmployee> MockEmployeeOdataSyncService()
        {
            var mock = new Mock<IPagedOdataSyncService<CrmEmployee>>();
            mock.SetupSequence(service => service.HasMoreResults)
                .Returns(true)
                .Returns(false);
            mock.SetupSequence(service => service.GetNextAsync(It.IsAny<CancellationToken>()).Result)
                .Returns(new OdataPagedResponse<CrmEmployee>
                {
                    Value = CrmEmployees.Take(3).ToList(),
                    OdataNextLink = "next page"
                })
                .Returns(new OdataPagedResponse<CrmEmployee>
                {
                    Value = CrmEmployees.Skip(3).ToList()
                });

            return mock.Object;
        }

        public ITransactionalBulkRepository<SyncSnapshot, PatchOperation> MockUpsertSyncSnapshotRepository()
        {
            var mock = new Mock<ITransactionalBulkRepository<SyncSnapshot, PatchOperation>>();
            mock.Setup(repo => repo.GetAllAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()).Result).Returns(InitialSnapshots);

            mock.Setup(repo => repo.GetNextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()).Result).Returns(InitialSnapshots);
            mock.SetupGet(repo => repo.HasMoreResults).Returns(false);

            Action<SyncSnapshot, Action<Task>, CancellationToken> upsertCallback = (SyncSnapshot snapshot, Action<Task> action, CancellationToken _) =>
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

                action?.Invoke(Task.CompletedTask);
            };

            Action<string, Action<Task>, CancellationToken> deleteCallback = (string id, Action<Task> action, CancellationToken _) =>
            {
                var oldSnapshot = InitialSnapshots.RemoveAll(s => s.Id == id);
                action?.Invoke(Task.CompletedTask);
            };

            var batchMock = new Mock<ITransactionBatch<PatchOperation>>();
            batchMock.Setup(batch => batch.SaveChangesAsync()).Callback(() =>
            {
                var newVersion = Guid.NewGuid().ToString();
                InitialSnapshots.ForEach(snapshot => snapshot.SyncVersion = newVersion);
            });
            mock.Setup(repo => repo.BeginTransactionAsync(It.IsAny<CancellationToken>()).Result).Returns(batchMock.Object);

            mock.Setup(repo => repo.BulkInsertAsync(It.IsAny<SyncSnapshot>(), It.IsAny<Action<Task>>(), It.IsAny<CancellationToken>()))
                .Callback(upsertCallback)
                .Returns(Task.CompletedTask);
            mock.Setup(repo => repo.BulkUpdateAsync(It.IsAny<SyncSnapshot>(), It.IsAny<Action<Task>>(), It.IsAny<CancellationToken>()))
                .Callback(upsertCallback)
                .Returns(Task.CompletedTask);
            mock.Setup(repo => repo.BulkDeleteAsync(It.IsAny<string>(), It.IsAny<Action<Task>>(), It.IsAny<CancellationToken>()))
                .Callback(deleteCallback)
                .Returns(Task.CompletedTask);

            return mock.Object;
        }

        public IBatchMessageService<MqMessage<Employee>, string> MockServiceBusClient()
        {
            var mock = new Mock<IBatchMessageService<MqMessage<Employee>, string>>();
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
