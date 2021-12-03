using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot.Persistence
{
    public interface IPagedRepository<T> : IRepository<T>
    {
        IAsyncEnumerable<IEnumerable<T>> GetAsyncPages(
            string query,
            IEnumerable<(string name, object value)> parameters,
            CancellationToken cancellationToken = default);
    }
}
