using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.DataSync.Crm.HttpClients;
using SSW.SophieBot.DataSync.Domain.Dto;
using SSW.SophieBot.DataSync.Domain.Employees;
using SSW.SophieBot.DataSync.Domain.Sync;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Test
{
    public class EmployeeSyncMock
    {
        public List<CrmEmployee> _crmEmployees = new()
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
                Systemuserid = "c",
                Organizationid = "ssw",
                Firstname = "Taya",
                Lastname = "Northwind",
                Fullname = "Taya Northwind"
            }
        };

        public List<SyncSnapshot> _initialSnapshots = new();

        public CrmClient MockCrmClient()
        {
            var mock = new Mock<CrmClient>();
            mock.Setup(client => client.GetPagedEmployeesAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()).Result)
                .Returns((string nextLink, CancellationToken _) =>
                {
                    if (string.IsNullOrEmpty(nextLink))
                    {
                        return new OdataPagedResponse<CrmEmployee>
                        {
                            Value = _crmEmployees.Take(3).ToList(),
                            OdataNextLink = "next page"
                        };
                    }
                    else
                    {
                        return new OdataPagedResponse<CrmEmployee>
                        {
                            Value = _crmEmployees.Skip(3).ToList()
                        };
                    }
                });

            return mock.Object;
        }

        public CosmosClient MockCosmosClient(string syncVersion)
        {
            var feedResponseMock = new Mock<FeedResponse<SyncSnapshot>>();
            feedResponseMock.Setup(feedResponse => feedResponse.GetEnumerator()).Returns(_initialSnapshots.GetEnumerator());
            //feedResponseMock.Setup(feedResponse => ((IEnumerable)feedResponse).GetEnumerator()).Returns(_initialSnapshots.GetEnumerator());

            var iteratorMock = new Mock<FeedIterator<SyncSnapshot>>();
            var retrived = true;
            iteratorMock.SetupGet(iterator => iterator.HasMoreResults)
                .Callback(() => retrived = false)
                .Returns(retrived);
            iteratorMock.Setup(iterator => iterator.ReadNextAsync(It.IsAny<CancellationToken>()).Result)
                .Returns(feedResponseMock.Object);

            var batchResMock = new Mock<TransactionalBatchResponse>();
            batchResMock.SetupGet(res => res.IsSuccessStatusCode).Returns(true);

            var itemResMock = new Mock<ItemResponse<SyncSnapshot>>();
            var itemResTaskMock = Task.FromResult(itemResMock.Object);

            var transactionBatchMock = new Mock<TransactionalBatch>();
            transactionBatchMock.Setup(batch => batch.PatchItem(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<PatchOperation>>(),
                It.IsAny<TransactionalBatchPatchItemRequestOptions>()))
                .Callback((string id, IReadOnlyList<PatchOperation> _, TransactionalBatchPatchItemRequestOptions _) =>
                {
                    var snapShot = _initialSnapshots.FirstOrDefault(snapshot => snapshot.Id == id);
                    if (snapShot != null)
                    {
                        snapShot.SyncVersion = syncVersion;
                    }
                });
            transactionBatchMock.Setup(batch => batch.ExecuteAsync(It.IsAny<CancellationToken>()).Result)
                .Returns(batchResMock.Object);

            var containerMock = new Mock<Container>();
            containerMock.Setup(container => container.GetItemQueryIterator<SyncSnapshot>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
                .Returns(iteratorMock.Object);
            containerMock.Setup(container => container.CreateTransactionalBatch(It.IsAny<PartitionKey>()))
                .Returns(transactionBatchMock.Object);
            containerMock.Setup(container => container.UpsertItemAsync(
                It.IsAny<SyncSnapshot>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
                .Callback((SyncSnapshot snapshot, PartitionKey _, ItemRequestOptions _, CancellationToken _) =>
                {
                    var oldSnapshot = _initialSnapshots.FirstOrDefault(s => s.Id == snapshot.Id);
                    if (oldSnapshot != null)
                    {
                        oldSnapshot.Modifiedon = snapshot.Modifiedon;
                        oldSnapshot.SyncVersion = snapshot.SyncVersion;
                    }
                    else
                    {
                        _initialSnapshots.Add(snapshot);
                    }
                })
                .Returns(itemResTaskMock);
            containerMock.Setup(container => container.DeleteItemAsync<SyncSnapshot>(
                It.IsAny<string>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
                .Callback((string id, PartitionKey _, ItemRequestOptions _, CancellationToken _) =>
                {
                    _initialSnapshots.RemoveAll(s => s.Id == id);
                })
                .Returns(itemResTaskMock);

            var clientMock = new Mock<CosmosClient>();
            clientMock.Setup(client => client.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(containerMock.Object);

            return clientMock.Object;
        }

        public ServiceBusClient MockServiceBusClient()
        {
            var mock = new Mock<ServiceBusClient>();
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
            var mock = new Mock<TimerInfo>();
            mock.SetupGet(timer => timer.IsPastDue).Returns(false);
            return mock.Object;
        }
    }
}
