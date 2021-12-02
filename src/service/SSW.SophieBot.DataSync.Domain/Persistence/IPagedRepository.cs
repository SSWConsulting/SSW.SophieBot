using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Domain.Persistence
{
    public interface IPagedRepository<T> : IRepository<T>
    {
        bool HasMoreResults { get; }

        Task<List<T>> GetNextAsync(string query, CancellationToken cancellationToken = default);
    }
}
