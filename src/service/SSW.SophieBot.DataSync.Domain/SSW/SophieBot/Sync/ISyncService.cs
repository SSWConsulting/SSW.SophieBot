using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Sync
{
    public interface ISyncService<T>
    {
        Task<T> GetAllAsync(CancellationToken cancellationToken = default);
    }

    public interface IPagedSyncService<T> : ISyncService<IEnumerable<T>>
    {
        IAsyncEnumerable<IEnumerable<T>> GetAsyncPage(CancellationToken cancellationToken = default);
    }
}
