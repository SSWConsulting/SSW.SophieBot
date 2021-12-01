using Microsoft.Extensions.Logging.Abstractions;
using SSW.SophieBot.DataSync.Crm.Functions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SSW.SophieBot.DataSync.Crm.Test
{
    public class EmployeeSyncTests
    {
        [Fact]
        public async Task Should_Sync_All_Profiles_When_No_Snapshot()
        {
            var mock = new EmployeeSyncMock();
            var syncVersion = Guid.NewGuid().ToString();
            var function = new EmployeeSync(
                mock.MockCrmClient(),
                mock.MockCosmosClient(syncVersion),
                mock.MockServiceBusClient(),
                mock.MockSyncOptions(),
                NullLogger<EmployeeSync>.Instance);

            await function.SyncEmployeeProfileAsync(mock.MockTimerInfo(), default);
        }
    }
}
