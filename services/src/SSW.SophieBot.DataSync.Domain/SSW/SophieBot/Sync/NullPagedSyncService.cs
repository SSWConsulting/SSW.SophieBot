using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Sync
{
    public sealed class NullPagedSyncService<T> : IPagedSyncService<T>
    {
        public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<T>());
        }

        public IAsyncEnumerable<IEnumerable<T>> GetAsyncPage(CancellationToken cancellationToken = default)
        {
            return Enumerable.Empty<IEnumerable<T>>().ToAsyncEnumerable();
        }
    }
}
