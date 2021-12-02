using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Sync
{
    public sealed class NullOdataSyncService<T> : IOdataSyncService<T>
    {
        public bool HasMoreResults => false;

        public Task<OdataResponse<T>> GetNextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new OdataResponse<T>());
        }
    }

    public sealed class NullPagedOdataSyncService<T> : IPagedOdataSyncService<T>
    {
        public bool HasMoreResults => false;

        public Task<OdataPagedResponse<T>> GetNextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new OdataPagedResponse<T>());
        }
    }
}
