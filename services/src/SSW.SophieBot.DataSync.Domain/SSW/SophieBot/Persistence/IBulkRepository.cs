using System.Threading.Tasks;

namespace SSW.SophieBot.Persistence
{
    public interface IBulkRepository<T> : IPagedRepository<T>
    {
        Task<IBulkOperations<T>> BeginBulkAsync();
    }
}
