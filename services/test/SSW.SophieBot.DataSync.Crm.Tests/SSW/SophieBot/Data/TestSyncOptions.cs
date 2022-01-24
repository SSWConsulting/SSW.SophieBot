using Microsoft.Extensions.Options;
using SSW.SophieBot.DataSync.Crm.Config;

namespace SSW.SophieBot
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
