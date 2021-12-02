using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Sync
{
    public interface ISyncService<T>
    {
        bool HasMoreResults { get; }

        Task<T> GetNextAsync(CancellationToken cancellationToken = default);
    }
}
