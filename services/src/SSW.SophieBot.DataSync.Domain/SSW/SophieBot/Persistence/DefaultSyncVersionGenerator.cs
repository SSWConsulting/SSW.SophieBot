using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Persistence
{
    public class DefaultSyncVersionGenerator : ISyncVersionGenerator
    {
        public Task<string> GenerateAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }
}
