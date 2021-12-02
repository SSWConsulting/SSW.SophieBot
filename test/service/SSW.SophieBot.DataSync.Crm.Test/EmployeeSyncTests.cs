using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SSW.SophieBot.DataSync.Crm.Functions;
using System.Threading.Tasks;
using Xunit;

namespace SSW.SophieBot.DataSync.Crm.Test
{
    public class EmployeeSyncTests
    {
        [Fact]
        public async Task Should_Sync_All_Profiles_When_No_Snapshot()
        {
            var function = GetEmployeesSyncFunction(out var mockInstance, out var timerInfoMock);
            await function.SyncEmployeeProfileAsync(timerInfoMock, default);

            mockInstance.InitialSnapshots.Count.ShouldBe(7);
        }

        private static EmployeeSync GetEmployeesSyncFunction(out EmployeeSyncMock mockInstance, out TimerInfo timerInfoMock)
        {
            mockInstance = new EmployeeSyncMock();
            var employeeOdataServiceMock = mockInstance.MockEmployeeOdataSyncService();
            var snapshotRepoMock = mockInstance.MockUpsertSyncSnapshotRepository();
            var serviceBusMock = mockInstance.MockServiceBusClient();
            var syncOptionsMock = mockInstance.MockSyncOptions();
            timerInfoMock = mockInstance.MockTimerInfo();

            return new EmployeeSync(employeeOdataServiceMock, snapshotRepoMock, serviceBusMock, syncOptionsMock, NullLogger<EmployeeSync>.Instance);
        }
    }
}
