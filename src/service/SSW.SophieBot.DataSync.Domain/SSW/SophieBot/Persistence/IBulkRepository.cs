using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Persistence
{
    public interface IBulkRepository<T> : IPagedRepository<T>
    {
        void BeginBulk();

        Task BulkInsertAsync(T item, Action<Task> callbackAction = null, CancellationToken cancellationToken = default);

        Task BulkUpdateAsync(T item, Action<Task> callbackAction = null, CancellationToken cancellationToken = default);

        Task BulkDeleteAsync(string id, Action<Task> callbackAction = null, CancellationToken cancellationToken = default);

        Task ExecuteBulkAsync();
    }
}
