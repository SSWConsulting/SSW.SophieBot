using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SSW.SophieBot.DataSync.Crm.Functions;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SSW.SophieBot.DataSync.Crm.Test
{
    public class EmployeeSyncTests
    {
        [Fact]
        public async Task Should_Sync_All_Profiles_Initially()
        {
            // Arrange
            var employeeSyncMock = new EmployeeSyncMock();
            var function = GetEmployeesSyncFunction(employeeSyncMock, null, Guid.NewGuid().ToString(), out var timerInfoMock);

            // Act
            await function.SyncEmployeeProfileAsync(timerInfoMock, default);

            // Assert
            employeeSyncMock.InitialSnapshots.Count.ShouldBe(7);
            employeeSyncMock.MqMessages.Count.ShouldBe(7);
            employeeSyncMock.MqMessages.All(message => message.SyncMode == SyncMode.Create).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Sync_New_Profile()
        {
            // Arrange
            var employeeSyncMock = new EmployeeSyncMock();
            var firstVersion = Guid.NewGuid().ToString();
            var syncVersion = Guid.NewGuid().ToString();
            var newId = Guid.NewGuid().ToString();

            var function = GetEmployeesSyncFunction(employeeSyncMock, null, syncVersion, out var timerInfoMock);

            // Act
            ManuallySyncSnapshots(employeeSyncMock, firstVersion);
            employeeSyncMock.CrmEmployees.Add(new CrmEmployee
            {
                Systemuserid = newId,
                Organizationid = "ssw",
                Firstname = "New",
                Lastname = "Northwind",
                Fullname = "New Northwind",
                Modifiedon = DateTime.Now
            });
            await function.SyncEmployeeProfileAsync(timerInfoMock, default);

            // Assert
            employeeSyncMock.InitialSnapshots.Count.ShouldBe(8);
            employeeSyncMock.InitialSnapshots.Single(snapshot => snapshot.Id == newId).ShouldNotBeNull();
            employeeSyncMock.InitialSnapshots.All(snapshot => snapshot.SyncVersion == syncVersion).ShouldBeTrue();
            employeeSyncMock.MqMessages.Single().SyncMode.ShouldBe(SyncMode.Create);
        }

        [Fact]
        public async Task Should_Sync_Modified_Profile()
        {
            // Arrange
            var employeeSyncMock = new EmployeeSyncMock();
            var firstVersion = Guid.NewGuid().ToString();
            var syncVersion = Guid.NewGuid().ToString();

            const string NewFullName = "NewFullName";
            const string NewOrganizationId = "sswtest";

            var function = GetEmployeesSyncFunction(employeeSyncMock, null, syncVersion, out var timerInfoMock);

            // Act
            ManuallySyncSnapshots(employeeSyncMock, firstVersion);
            var employeeToModify = employeeSyncMock.CrmEmployees.First();
            employeeToModify.Fullname = NewFullName;
            employeeToModify.Organizationid = NewOrganizationId;
            employeeToModify.Modifiedon = DateTime.Now;
            await function.SyncEmployeeProfileAsync(timerInfoMock, default);

            // Assert
            employeeSyncMock.InitialSnapshots
                .Single(snapshot => snapshot.Id == employeeToModify.Systemuserid)
                .OrganizationId
                .ShouldBe(NewOrganizationId);
            employeeSyncMock.InitialSnapshots.All(snapshot => snapshot.SyncVersion == syncVersion).ShouldBeTrue();
            employeeSyncMock.MqMessages.Single().SyncMode.ShouldBe(SyncMode.Update);
            employeeSyncMock.MqMessages.Single().Message.FullName.ShouldBe(NewFullName);
        }

        [Fact]
        public async Task Should_Sync_Deleted_Profile()
        {
            // Arrange
            var employeeSyncMock = new EmployeeSyncMock();
            var firstVersion = Guid.NewGuid().ToString();
            var syncVersion = Guid.NewGuid().ToString();

            var mockAction = (Mock<ITransactionalBulkRepository<SyncSnapshot, PatchOperation>> mock) =>
            {
                var batchMock = new Mock<ITransactionBatch<PatchOperation>>();
                batchMock.Setup(batch => batch.SaveChangesAsync().Result).Callback(() =>
                {
                    for (int i = 1; i < employeeSyncMock.InitialSnapshots.Count; i++)
                    {
                        employeeSyncMock.InitialSnapshots[i].SyncVersion = syncVersion;
                    }
                }).Returns(true);
                mock.Setup(repo => repo.BeginTransactionAsync(It.IsAny<CancellationToken>()).Result).Returns(batchMock.Object);
            };
            var function = GetEmployeesSyncFunction(employeeSyncMock, mockAction, syncVersion, out var timerInfoMock);

            // Act
            ManuallySyncSnapshots(employeeSyncMock, firstVersion);
            var employeeToDelete = employeeSyncMock.CrmEmployees.First();
            employeeSyncMock.CrmEmployees.Remove(employeeToDelete);
            await function.SyncEmployeeProfileAsync(timerInfoMock, default);

            // Assert
            employeeSyncMock.InitialSnapshots.Count.ShouldBe(6);
            employeeSyncMock.InitialSnapshots.All(snapshot => snapshot.SyncVersion == syncVersion).ShouldBeTrue();
            employeeSyncMock.InitialSnapshots.Any(snapshot => snapshot.Id == employeeToDelete.Systemuserid).ShouldBeFalse();
            employeeSyncMock.MqMessages.Single().SyncMode.ShouldBe(SyncMode.Delete);
        }

        private static void ManuallySyncSnapshots(EmployeeSyncMock mockInstance, string syncVersion)
        {
            mockInstance.InitialSnapshots = mockInstance.CrmEmployees.Select(employee => new SyncSnapshot
            {
                Id = employee.Systemuserid,
                OrganizationId = employee.Organizationid,
                Modifiedon = employee.Modifiedon,
                SyncVersion = syncVersion
            }).ToList();
        }

        private static EmployeeSync GetEmployeesSyncFunction(
            EmployeeSyncMock mockInstance,
            Action<Mock<ITransactionalBulkRepository<SyncSnapshot, PatchOperation>>> snapshotRepoAction,
            string syncVersion,
            out TimerInfo timerInfoMock)
        {
            var employeeOdataServiceMock = mockInstance.MockEmployeeOdataSyncService();
            var snapshotRepoMock = mockInstance.GetBasicSyncSnapshotRepositoryMock(syncVersion);
            var serviceBusMock = mockInstance.MockServiceBusClient();
            var syncVersionGeneratorMock = mockInstance.MockSyncVersionGenerator(syncVersion);
            var syncOptionsMock = mockInstance.MockSyncOptions();
            timerInfoMock = mockInstance.MockTimerInfo();

            snapshotRepoAction?.Invoke(snapshotRepoMock);

            return new EmployeeSync(
                employeeOdataServiceMock,
                snapshotRepoMock.Object,
                serviceBusMock,
                syncVersionGeneratorMock,
                syncOptionsMock,
                NullLogger<EmployeeSync>.Instance);
        }
    }
}
