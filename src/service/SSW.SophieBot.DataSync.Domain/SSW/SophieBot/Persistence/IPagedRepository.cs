using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Persistence
{
    public interface IPagedRepository<T> : IRepository<T>
    {
        // TODO: remove this
        bool HasMoreResults { get; }

        // TODO: return IEnumerable<IEnumerable<TDocument>> instead, and rename to e.g. GetPagesAsync
        Task<List<T>> GetNextAsync(string query, IEnumerable<(string name, object value)> parameters, CancellationToken cancellationToken = default);
    }
}
