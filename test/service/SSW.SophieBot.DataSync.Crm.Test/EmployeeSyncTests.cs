using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SSW.SophieBot.DataSync.Crm.Functions;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Persistence;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SSW.SophieBot.DataSync.Crm.Test
{
    public class EmployeeSyncTests
    {
        [Fact]
        public async Task Should_Sync_All_Profiles_When_No_Snapshot()
        {
            var employeeSyncMock = new EmployeeSyncMock();
            var function = GetEmployeesSyncFunction(employeeSyncMock, out var _, out var timerInfoMock);
            await function.SyncEmployeeProfileAsync(timerInfoMock, default);

            employeeSyncMock.InitialSnapshots.Count.ShouldBe(7);
        }

        [Fact]
        public async Task Should_Sync_New_Profile()
        {
            var employeeSyncMock = new EmployeeSyncMock();
            var firstVersion = Guid.NewGuid().ToString();
            var newId = Guid.NewGuid().ToString();

            employeeSyncMock.InitialSnapshots = employeeSyncMock.CrmEmployees.Select(employee => new SyncSnapshot
            {
                Id = employee.Systemuserid,
                OrganizationId = employee.Organizationid,
                Modifiedon = employee.Modifiedon,
                SyncVersion = firstVersion
            }).ToList();
            employeeSyncMock.CrmEmployees.Add(new CrmEmployee
            {
                Systemuserid = newId,
                Organizationid = "ssw",
                Firstname = "New",
                Lastname = "Northwind",
                Fullname = "New Northwind",
                Modifiedon = DateTime.Now
            });

            var function = GetEmployeesSyncFunction(employeeSyncMock, out var syncVersion, out var timerInfoMock);

            await function.SyncEmployeeProfileAsync(timerInfoMock, default);

            employeeSyncMock.InitialSnapshots.Count.ShouldBe(8);
            employeeSyncMock.InitialSnapshots.Single(snapshot => snapshot.Id == newId).ShouldNotBeNull();
            employeeSyncMock.InitialSnapshots
                .Select(snapshot => snapshot.SyncVersion)
                .Distinct()
                .Single()
                .ShouldBe(syncVersion);
        }

        private static EmployeeSync GetEmployeesSyncFunction(
            EmployeeSyncMock mockInstance,
            out string syncVersion,
            out TimerInfo timerInfoMock)
        {
            syncVersion = Guid.NewGuid().ToString();
            var employeeOdataServiceMock = mockInstance.MockEmployeeOdataSyncService();
            var snapshotRepoMock = mockInstance.MockUpsertSyncSnapshotRepository(syncVersion);
            var serviceBusMock = mockInstance.MockServiceBusClient();
            var syncVersionGeneratorMock = mockInstance.MockSyncVersionGenerator(syncVersion);
            var syncOptionsMock = mockInstance.MockSyncOptions();
            timerInfoMock = mockInstance.MockTimerInfo();

            return new EmployeeSync(
                employeeOdataServiceMock,
                snapshotRepoMock,
                serviceBusMock,
                syncVersionGeneratorMock,
                syncOptionsMock,
                NullLogger<EmployeeSync>.Instance);
        }
    }
}
