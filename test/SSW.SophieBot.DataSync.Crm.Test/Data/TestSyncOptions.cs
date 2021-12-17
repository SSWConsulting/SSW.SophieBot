using Microsoft.Extensions.Options;
using SSW.SophieBot.DataSync.Crm.Config;

namespace SSW.SophieBot.DataSync.Crm.Test.Data
{
    public class TestSyncOptions : IOptions<SyncOptions>
    {
        public virtual SyncOptions Value { get; } = new SyncOptions
        {
            OrganizationId = "ssw",
            EmployeeSync = new SyncFunctionOptions()
        };
    }
}
