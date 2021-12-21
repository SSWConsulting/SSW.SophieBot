using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Persistence
{
    public interface IBulkOperations<T>
    {
        void BulkInsert(T item, CancellationToken cancellationToken = default);

        void BulkUpdate(T item, CancellationToken cancellationToken = default);

        void BulkDelete(string id, CancellationToken cancellationToken = default);

        Task<List<T>> ExecuteBulkAsync();
    }
}
