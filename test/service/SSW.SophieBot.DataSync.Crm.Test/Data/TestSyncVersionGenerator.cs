using SSW.SophieBot.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Test.Data
{
    public class TestSyncVersionGenerator : ISyncVersionGenerator
    {
        private readonly string _syncVersion;

        public TestSyncVersionGenerator(string syncVersion)
        {
            _syncVersion = syncVersion;
        }

        public virtual Task<string> GenerateAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_syncVersion);
        }
    }
}
